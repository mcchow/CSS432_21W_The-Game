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
        public List<MockClient> clients;
        public List<SeqEvent> sequence;
        private HashSet<String> messageTypes;
        public List<List<MessageType>> recieved;
        private string errorMessagePrefix;

        public struct SeqEvent
        {
            public bool ClearRecieved;
            public int PlayerIndex;
            public bool Expectation;
            public MessageType Message;
            public string MessageID;
        }

        public ClientServerSeq()
        {
            // Retrieves expected question category
            server = new MockServer();
            clients = new List<MockClient>();
            clients.Add(new MockClient());
            clients.Add(new MockClient());
            sequence = new List<SeqEvent>();
            messageTypes = new HashSet<string>();
            recieved = new List<List<MessageType>>();
            recieved.Add(new List<MessageType>());
            recieved.Add(new List<MessageType>());
            errorMessagePrefix = "";
        }

        public void Dispose()
        {
            server.Stop();
            foreach (MockClient client in clients)
            {
                client.Stop();
            }
        }

        public ClientServerSeq expect<T>(T message) where T : MessageType, new()
        {
            return expect<T>(0, message);
        }

        public ClientServerSeq expect<T>(int playerIndex = 0, T message = null) where T : MessageType, new()
        {
            MessageType mtype = new T();
            if (!messageTypes.Contains(mtype.MessageID())) {
                clients[playerIndex].protocol.RegisterMessageHandler<T>((T m, Connection c) =>
                {
                    recieved[playerIndex].Add(m);
                });
            }
            SeqEvent e = new SeqEvent();
            e.PlayerIndex = playerIndex;
            e.Expectation = true;
            e.Message = message;
            e.MessageID = mtype.MessageID();
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq send(MessageType message)
        {
            return send(0, message);
        }

        public ClientServerSeq send(int playerIndex, MessageType message)
        {
            SeqEvent e = new SeqEvent();
            e.PlayerIndex = playerIndex;
            e.Expectation = false;
            e.Message = message;
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq clearRecieved()
        {
            SeqEvent e = new SeqEvent();
            e.ClearRecieved = true;
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq test()
        {
            foreach (SeqEvent e in sequence) {
                if (e.ClearRecieved)
                {
                    foreach (List<MessageType> recievedList in recieved)
                    {
                        recievedList.Clear();
                    }
                    errorMessagePrefix += ".clearRecieved()\n";
                    continue;
                }
                if (!e.Expectation)
                {
                    errorMessagePrefix += ".send("+e.PlayerIndex+","+e.Message.MessageID()+ ")\n";
                    checkForUnexpectedMessage();
                    clients[e.PlayerIndex].connection.Send(e.Message);
                } else
                {
                    errorMessagePrefix += ".expect<"+e.MessageID+">(" + e.PlayerIndex + ", ...)\n";
                    Thread.Sleep(1);
                    foreach (MockClient c in clients)
                    {
                        c.protocol.HandleMessages();
                    }
                    Assert.True(recieved[e.PlayerIndex].Count > 0, errorMessagePrefix + ": Client " + e.PlayerIndex + " expected to recieve a " + e.MessageID + " message.");
                    Assert.True(recieved[e.PlayerIndex][0].MessageID().Equals(e.MessageID), errorMessagePrefix + ": Client " + e.PlayerIndex + " expected to recieve a " + e.MessageID + " message.");
                    if (e.Message != null)
                    {
                        Assert.True(recieved[e.PlayerIndex][0].Equals(e.Message), errorMessagePrefix + ": Client " + e.PlayerIndex + " recieved message of expected type, but message contents were unexpected.");
                    }
                    recieved[e.PlayerIndex].RemoveAt(0);
                }
            }
            checkForUnexpectedMessage();
            errorMessagePrefix += ".test()\n";
            return this;
        }

        private void checkForUnexpectedMessage()
        {
            for (int i = 0; i < recieved.Count; ++i)
            {
                Assert.True(recieved[i].Count == 0, errorMessagePrefix + ": Client " + i + " recieved an unexpected message.");
            }
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
            Register register = new Register();
            register.Name = "PlayerName!";
            Register register2 = new Register();
            register2.Name = "Player2Name!";
            ListRoomsRequest listRoomsRequest = new ListRoomsRequest();
            JoinRoom joinRoom = new JoinRoom();
            AnswerAndResult answerAndResult = new AnswerAndResult();
            answerAndResult.correctAnswer = 'C';
            seq.clients[0].protocol.RegisterMessageHandler<RoomEntry>((RoomEntry roomEntry, Connection c) =>
            {
                joinRoom.RoomID = roomEntry.roomID;
            });
            ChosenCard choseCard = new ChosenCard();
            choseCard.Card = "Category"; //TODO use correct card category here!
            seq.send(register)
                .send(listRoomsRequest)
                .expect<RoomEntry>() // We should get at least one RoomEntry message, but might get more, so
                .clearRecieved() // we clear the recieved message queue to prevent test failure from unexpected messages.
                .send(joinRoom)
                .send(1, register2)
                .send(1, joinRoom)
                .expect<AskForCard>(new AskForCard())
                .send(choseCard)
                .expect<AnswerAndResult>(answerAndResult)
                .expect<AnswerAndResult>(1, answerAndResult)
                .test();
        }

        [Fact]
        public void TestPlayerAnswer_RespondsWithWinnerIfWon()
        {

            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestPlayerAnswer_SendsAnswerAndResultToOtherPlayerIfNoWinner()
        {

            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestPlayerAnswer_SendsWinnerToOtherPlayerIfWon()
        {

            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestRegister()
        {
            // Ignores duplicate Register messages from same connection
            // Adds player to the connection map (api calls that require
            // the player to be in the connection map should work now)
            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestUnregister()
        {
            // Removes player from connection map (api calls that require
            // the player to be in the connection map should be ignored)
            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestClientDisconnect()
        {
            // Closes connection
            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestJoinRoom()
        {
            // Ignored if player not in connection map
            // Ignored if player already in room
            // Sends RoomFull message if room is full
            // Sends AskForCard message to one player, NextPlayerTurn message to other player
            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestLeaveRoom()
        {
            // Ignored if player not in connection map
            // Ignored if player not in a room
            // JoinRoom will no longer be ignored
            Assert.True(false, "Test not implemented");
        }

        [Fact]
        public void TestListRoomsRequest()
        {
            seq.send(new ListRoomsRequest())
                .expect<RoomEntry>()
                .clearRecieved()
                .test();
        }
    }

}
