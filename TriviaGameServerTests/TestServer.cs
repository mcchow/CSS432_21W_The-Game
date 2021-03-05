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
        public List<Question> questions
        {
            get
            {
                return server.questions.questions;
            } set
            {
                server.questions.questions = value;
            }
        }
        public List<string> questionRequests
        {
            get
            {
                return server.questions.questionRequests;
            }
            set
            {
                server.questions.questionRequests = value;
            }
        }
        public MockServer server;
        public List<MockClient> clients;
        public List<SeqEvent> sequence;
        private HashSet<String> messageTypes;
        public List<List<MessageType>> recieved;
        private string errorMessagePrefix;

        public struct SeqEvent
        {
            public bool LogEvent;
            public string logMessage;
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

        public void addQuestion(TriviaQuestion triviaQuestion, char answer)
        {
            Question q = new Question();
            q.question = triviaQuestion;
            q.answer = answer;
            questions.Add(q);
        }

        public void addQuestionSequence(TriviaQuestion triviaQuestion, string answers)
        {
            foreach (char c in answers)
            {
                addQuestion(triviaQuestion, c);
            }
        }

        public void Dispose()
        {
            server.Stop();
            foreach (MockClient client in clients)
            {
                client.Stop();
            }
        }

        public void clearErrorPrefix()
        {
            errorMessagePrefix = "";
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

        public ClientServerSeq log(string message)
        {
            SeqEvent e = new SeqEvent();
            e.LogEvent = true;
            e.logMessage = message;
            sequence.Add(e);
            return this;
        }

        public ClientServerSeq expectWrong(int playerIndex, int nextPlayerIndex, string category, char answer)
        {
            //TODO fix expectations
            this.log(".expectWrong(" + playerIndex + "," + nextPlayerIndex + "," + category + "," + answer + ") {")
                .expect<AskForCard>(playerIndex, new AskForCard())
                .send(playerIndex, new ChosenCard(category))
                .expect<TriviaQuestion>(playerIndex)
                .send(playerIndex, new PlayerAnswer(answer))
                .expect<AnswerAndResult>(playerIndex)
                .expect<AnswerAndResult>(nextPlayerIndex)
                .log("}");
            return this;
        }

        public ClientServerSeq expectRight(int playerIndex, int nextPlayerIndex, string category, char answer)
        {
            //TODO fix expectations
            this.log(".expectWrong(" + playerIndex + "," + nextPlayerIndex + "," + category + "," + answer + ") {")
                .expect<AskForCard>(playerIndex, new AskForCard())
                .send(playerIndex, new ChosenCard(category))
                .expect<TriviaQuestion>(playerIndex)
                .send(playerIndex, new PlayerAnswer(answer))
                .expect<AnswerAndResult>(playerIndex)
                .expect<AnswerAndResult>(nextPlayerIndex)
                .log("}");
            return this;
        }

        public ClientServerSeq expectWin(int playerIndex, int nextPlayerIndex, string category, char answer)
        {
            //TODO fix expectations
            this.log(".expectWrong(" + playerIndex + "," + nextPlayerIndex + "," + category + "," + answer + ") {")
                .expect<AskForCard>(playerIndex, new AskForCard())
                .send(playerIndex, new ChosenCard(category))
                .expect<TriviaQuestion>(playerIndex)
                .send(playerIndex, new PlayerAnswer(answer))
                .expect<Winner>(playerIndex)
                .expect<Winner>(nextPlayerIndex)
                .log("}");
            return this;
        }

        public ClientServerSeq test()
        {
            foreach (SeqEvent e in sequence) {
                if (e.LogEvent)
                {
                    errorMessagePrefix += e.logMessage + "\n";
                    continue;
                }
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
                    Thread.Sleep(10);
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
            sequence.Clear();
            return this;
        }

        private void checkForUnexpectedMessage()
        {
            for (int i = 0; i < recieved.Count; ++i)
            {
                string unexpectedMessageType = "";
                if (recieved[i].Count > 0)
                {
                     unexpectedMessageType = recieved[i][0].MessageID();
                }
                Assert.True(recieved[i].Count == 0, errorMessagePrefix + ": Client " + i + " recieved an unexpected message of type " +
                    unexpectedMessageType + ".");
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

        private List<RoomEntry> getRooms()
        {
            Boolean gettingRooms = new bool();
            gettingRooms = true;
            List<RoomEntry> rooms = new List<RoomEntry>();
            seq.clients[0].protocol.RegisterMessageHandler<RoomEntry>((RoomEntry roomEntry, Connection c) =>
            {
                if (gettingRooms)
                {
                    rooms.Add(roomEntry);
                }
            });
            ChosenCard choseCard = new ChosenCard();
            choseCard.Card = "Category"; //TODO use correct card category here!
            seq.send(new ListRoomsRequest())
                .expect<RoomEntry>()
                .clearRecieved()
                .test();
            gettingRooms = false; // Prevent further modification of returned room list by future RoomEntry events.
            seq.clearErrorPrefix();
            return rooms;
        }

        [Theory]
        [InlineData("History")]
        [InlineData("Art")]
        [InlineData("Science")]
        [InlineData("Geography")]
        [InlineData("Sports")]
        [InlineData("Entertainment")]
        public void TestChosenCardWithValidCategory(string category)  // Currently fails because JoinRoom handler is not implemented
        {
            seq.addQuestion(new TriviaQuestion("Q?", "A", "B", "C", "D"), 'C');
            TriviaQuestion expected = seq.questions[0].question;
            seq.send(new Register("Player One"))
                .send(new CreateRoom())
                .test();
            seq.send(1, new Register("Player Two"))
                .send(1, new JoinRoom(getRooms()[0].roomID))
                .expect<AskForCard>(new AskForCard())
                .send(new ChosenCard(category))
                .expect<TriviaQuestion>(expected)
                .test();
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
            //This test case assumes that player one goes first!
            AnswerAndResult answerAndResult = new AnswerAndResult();
            //TODO need to set answerAndResult.numCards
            answerAndResult.correctAnswer = 'C';
            answerAndResult.whosTurn = 1; //TODO should be player two's turn, is this zero-indexed?
            seq.send(new Register("Player One"))
                .send(new JoinRoom(getRooms()[0].roomID))
                .send(1, new Register("Player Two"))
                .send(1, new JoinRoom(getRooms()[0].roomID))
                .expect<AskForCard>(new AskForCard())
                .send(new ChosenCard("Category"))
                .expect<AnswerAndResult>(answerAndResult)
                .expect<AnswerAndResult>(1, answerAndResult)
                .test();
        }

        [Fact]
        public void TestPlayerAnswer_RespondsWithWinnerIfWon()
        {
            TriviaQuestion tq = new TriviaQuestion("Q?", "A", "B", "C", "D");
            seq.addQuestionSequence(tq, "AAAABBCCCCDDAA");
            seq.send(new Register("P1"))
                .send(new CreateRoom())
                .send(1, new Register("P2"))
                .send(1, new JoinRoom(getRooms()[0].roomID))
                // Player 1 (zero-indexed id=0) goes first
                .expectRight(0, 1, "History", 'A')
                .expectRight(0, 1, "Art", 'A')
                .expectRight(0, 1, "Science", 'A')
                .expectRight(0, 1, "Geography", 'A')
                .expectRight(0, 1, "Sports", 'B')
                .expectWrong(0, 1, "Entertainment", 'A')
                // Player 2
                .expectRight(1, 0, "Geography", 'C')
                .expectRight(1, 0, "Sports", 'C')
                .expectRight(1, 0, "Entertainment", 'C')
                .expectRight(1, 0, "Science", 'C')
                .expectRight(1, 0, "Art", 'D')
                .expectWrong(1, 0, "History", 'C')
                .log("Both players should now be tied w/ 5 cards each.")
                // Player 1
                .expectWrong(0, 1, "History", 'A')
                // Player 2
                .expectWin(1, 0, "Entertainment", 'A')
                .test();
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
            seq.send(new Register("P1"))
                .send(new CreateRoom())
                .send(new ListRoomsRequest())
                .expect<RoomEntry>()
                .clearRecieved()
                .test();
        }
    }

}
