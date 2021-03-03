using System;
using Xunit;
using TriviaGameServer;
using TriviaGameProtocol;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace TriviaGameServerTests
{

    public class MockClient {
        public QueuedProtocol protocol;
        public Connection connection;
        public Thread connectionThread;

        public MockClient()
        {
            protocol = new QueuedProtocol();
            connect();
        }

        private void connect()
        {
            Socket sd = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPHostEntry serverHostEntry = Dns.GetHostEntry("127.0.0.1");
            IPAddress serverIP = serverHostEntry.AddressList[0];
            IPEndPoint serverEndPoint = new IPEndPoint(serverIP, 8080);
            sd.Connect(serverEndPoint);

            connection = new Connection(sd, protocol);

            connectionThread = new Thread(new ThreadStart(connection.RecieveLoop));
            connectionThread.Start();
        }

        public void Stop()
        {
            connection.Disconnect();
            connectionThread.Join();
        }
    }

    public class MockQuestionSource : QuestionSource
    {
        public List<string> questionRequests;
        public List<Question> questions;
        public int questionIndex = 0;
        public MockQuestionSource()
        {
            questionRequests = new List<string>();
            questions = new List<Question>();
        }
        public Question GetQuestion(string category)
        {
            if (questionIndex > questions.Count)
            {
                return questions[0];
            }
            return questions[questionIndex++];
        }
    }

    public class MockServer
    {
        public Thread serverThread;
        public Server server;
        public MockQuestionSource questions;
        public MockServer()
        {
            questions = new MockQuestionSource();

            Question q = new Question();
            TriviaQuestion tq = new TriviaQuestion();
            tq.question = "Question?";
            tq.optionA = "Option A.";
            tq.optionB = "Option B.";
            tq.optionC = "Option C.";
            tq.optionD = "Option D.";
            q.question = tq;
            q.answer = 'C';
            questions.questions.Add(q);
            server = new Server(questions);
            serverThread = new Thread(new ThreadStart(server.listen));
            serverThread.Start();
        }

        public void Stop()
        {
            server.shutdown();
            serverThread.Join();
        }
    }

    public class ClientServerSeq : IDisposable
    {
        public MockServer server;
        public MockClient client;
        public MockClient client2;
        public List<SeqEvent> sequence;
        private HashSet<String> messageTypes;
        private List<MessageTypeWrapper> recieved;
        private List<MessageTypeWrapper> recieved2;

        public struct MessageTypeWrapper
        {
            public bool PlayerOne;
            public MessageType Message;
        }

        public struct SeqEvent
        {
            public bool PlayerOne;
            public bool Expectation;
            public MessageType Message;
        }

        public ClientServerSeq()
        {
            // Retrieves expected question category
            server = new MockServer();
            client = new MockClient();
            client2 = new MockClient();
            sequence = new List<SeqEvent>();
            messageTypes = new HashSet<string>();
            recieved = new List<MessageTypeWrapper>();
            recieved2 = new List<MessageTypeWrapper>();
        }

        public void Dispose()
        {
            server.Stop();
            client.Stop();
            client2.Stop();
        }

        public ClientServerSeq expect<T>(T message) where T : MessageType, new()
        {
            if (!messageTypes.Contains(message.MessageID())) {
                client.protocol.RegisterMessageHandler<T>((T m, Connection c) =>
                {
                    MessageTypeWrapper mtw = new MessageTypeWrapper();
                    mtw.PlayerOne = true;
                    mtw.Message = m;
                    recieved.Add(mtw);
                });
            }
            SeqEvent e = new SeqEvent();
            e.PlayerOne = true;
            e.Expectation = true;
            e.Message = message;
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq expect2<T>(T message) where T : MessageType, new()
        {
            if (!messageTypes.Contains(message.MessageID()))
            {
                client.protocol.RegisterMessageHandler<T>((T m, Connection c) =>
                {
                    MessageTypeWrapper mtw = new MessageTypeWrapper();
                    mtw.PlayerOne = false;
                    mtw.Message = m;
                    recieved2.Add(mtw);
                });
            }
            SeqEvent e = new SeqEvent();
            e.PlayerOne = false;
            e.Expectation = true;
            e.Message = message;
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq send(MessageType message)
        {
            SeqEvent e = new SeqEvent();
            e.PlayerOne = true;
            e.Expectation = false;
            e.Message = message;
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq send2(MessageType message)
        {
            SeqEvent e = new SeqEvent();
            e.PlayerOne = false;
            e.Expectation = false;
            e.Message = message;
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq test()
        {
            foreach (SeqEvent e in sequence) {
                if (!e.Expectation)
                {
                    Assert.True(recieved.Count == 0, "Recieved an unexpected message.");
                    Assert.True(recieved2.Count == 0, "Recieved an unexpected message.");
                    if (e.PlayerOne)
                    {
                        client.connection.Send(e.Message);
                    } else
                    {
                        client2.connection.Send(e.Message);
                    }
                } else
                {
                    System.Threading.Thread.Sleep(10);
                    client.protocol.HandleMessages();
                    client2.protocol.HandleMessages();
                    if (e.PlayerOne)
                    {
                        Assert.True(recieved.Count > 0, "Expected to recieve a " + e.Message.MessageID() + " message.");
                        Assert.True(recieved[0].Equals(e.Message), "Recieved message of expected type, but message contents were unexpected.");
                        recieved.RemoveAt(0);
                    } else {
                        Assert.True(recieved2.Count > 0, "Expected to recieve a " + e.Message.MessageID() + " message.");
                        Assert.True(recieved2[0].Equals(e.Message), "Recieved message of expected type, but message contents were unexpected.");
                        recieved2.RemoveAt(0);
                    }
                }
            }
            Assert.True(recieved.Count == 0, "Recieved an unexpected message.");
            Assert.True(recieved2.Count == 0, "Recieved an unexpected message.");
            return this;
        }

    }


    public class ServerIntegrationTests : IDisposable
    {
        ClientServerSeq seq;
        public ServerIntegrationTests()
        {
            seq = new ClientServerSeq();
        }

        public void Dispose()
        {
            seq.Dispose();
        }

        [Fact]
        public void TestChosenCard()
        {
            ChosenCard msg = new ChosenCard();
            msg.Card = "CategoryName";
            TriviaQuestion expected = seq.server.questions.questions[0].question;

            seq.send(msg).expect(expected).test();

        }

        // PlayerAnswer:
        // does nothing if player is not in a room
        // does nothing if it is not player's turn
        // responds with AnswerAndResult if winner not yet chosen
        // sends AnswerAndResult to opponent if winner not yet chosen
        // responds with Winner if player wins
        // sends Winner to opponent if player wins

        [Fact]
        public void TestPlayerAnswer_RespondsWithAnswerAndResultIfNoWinner()
        {

        }

        [Fact]
        public void TestPlayerAnswer_RespondsWithWinnerIfWon()
        {

        }

        [Fact]
        public void TestPlayerAnswer_SendsAnswerAndResultToOtherPlayerIfNoWinner()
        {

        }

        [Fact]
        public void TestPlayerAnswer_SendsWinnerToOtherPlayerIfWon()
        {

        }

        [Fact]
        public void TestRegister()
        {
            // Ignores duplicate Register messages from same connection
            // Adds player to the connection map (api calls that require
            // the player to be in the connection map should work now)
        }

        [Fact]
        public void TestUnregister()
        {
            // Removes player from connection map (api calls that require
            // the player to be in the connection map should be ignored)
        }

        [Fact]
        public void TestClientDisconnect()
        {
            // Closes connection
        }

        [Fact]
        public void TestJoinRoom()
        {
            // Ignored if player not in connection map
            // Ignored if player already in room
            // Sends RoomFull message if room is full
            // Sends AskForCard message to one player, NextPlayerTurn message to other player
        }

        [Fact]
        public void TestLeaveRoom()
        {
            // Ignored if player not in connection map
            // Ignored if player not in a room
            // JoinRoom will no longer be ignored
        }

        [Fact]
        public void TestListRoomsRequest()
        {
            // Responds with sequence of RoomEntry messages
        }
    }

}
