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

    public class ServerIntegrationTests : IDisposable
    {
        MockServer server;
        MockClient client;
        public ServerIntegrationTests()
        {
            // Retrieves expected question category
            server = new MockServer();
            client = new MockClient();
        }

        [Fact]
        public void TestChosenCard()
        {
            ChosenCard msg = new ChosenCard();
            msg.Card = "CategoryName";
            client.connection.Send(msg);
            TriviaQuestion expected = server.questions.questions[0].question;
            bool triviaQuestionRecieved = false;
            client.protocol.RegisterMessageHandler<TriviaQuestion>((TriviaQuestion res, Connection c) =>
            {
                Assert.Equal(expected, res);
                triviaQuestionRecieved = true;
            });
            client.protocol.HandleMessages();
            Assert.Equal(1, server.questions.questionIndex);
            Assert.True(triviaQuestionRecieved);
        }

        public void Dispose()
        {
            server.Stop();
            client.Stop();
        }
    }

    public class TestPlayerAnswer
    {
        // does nothing if player is not in a room
        // does nothing if it is not player's turn
        // responds with AnswerAndResult if winner not yet chosen
        // sends AnswerAndResult to opponent if winner not yet chosen
        // responds with Winner if player wins
        // sends Winner to opponent if player wins
    }

    public class TestRegister
    {
        // Ignores duplicate Register messages from same connection
        // Adds player to the connection map (api calls that require
        // the player to be in the connection map should work now)
    }

    public class TestUnregister
    {
        // Removes player from connection map (api calls that require
        // the player to be in the connection map should be ignored)
    }

    public class TestClientDisconnect
    {
        // Closes connection
    }

    public class TestJoinRoom
    {
        // Ignored if player not in connection map
        // Ignored if player already in room
        // Sends RoomFull message if room is full
        // Sends AskForCard message to one player, NextPlayerTurn message to other player
    }

    public class TestLeaveRoom
    {
        // Ignored if player not in connection map
        // Ignored if player not in a room
        // JoinRoom will no longer be ignored
    }

    public class TestListRoomsRequest
    {
        // Responds with sequence of RoomEntry messages
    }

}
