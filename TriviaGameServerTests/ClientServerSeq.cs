using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TriviaGameProtocol;
using TriviaGameServer;
using Xunit;

namespace TriviaGameServerTests
{
    public class MockClient
    {
        public QueuedProtocol protocol;
        public Connection connection;
        public Thread connectionThread;
        private int port;

        public MockClient(int port)
        {
            protocol = new QueuedProtocol();
            this.port = port;
            Thread.Sleep(50);
            connect();
        }

        private void connect()
        {
            Console.WriteLine("Attempting to connect to port " + port);
            Socket sd = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEndPoint = new IPEndPoint(serverIP, port);
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
        public MockServer(int port)
        {
            questions = new MockQuestionSource();
            server = new Server(port, questions);
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
            }
            set
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
        private List<HashSet<String>> messageTypes;
        public List<List<MessageType>> recieved;
        private string errorMessagePrefix;
        public int port;

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

        public ClientServerSeq(int port)
        {
            this.port = port;
            // Retrieves expected question category
            server = new MockServer(port);
            clients = new List<MockClient>();
            clients.Add(new MockClient(port));
            clients.Add(new MockClient(port));
            sequence = new List<SeqEvent>();
            messageTypes = new List<HashSet<string>>();
            messageTypes.Add(new HashSet<string>());
            messageTypes.Add(new HashSet<string>());
            recieved = new List<List<MessageType>>();
            recieved.Add(new List<MessageType>());
            recieved.Add(new List<MessageType>());
            errorMessagePrefix = "";
        }

        public ClientServerSeq addClient()
        {
            clients.Add(new MockClient(port));
            messageTypes.Add(new HashSet<string>());
            recieved.Add(new List<MessageType>());
            return this;
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
            if (!messageTypes[playerIndex].Contains(mtype.MessageID()))
            {
                clients[playerIndex].protocol.RegisterMessageHandler<T>((T m, Connection c) =>
                {
                    recieved[playerIndex].Add(m);
                });
                messageTypes[playerIndex].Add(mtype.MessageID());
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
            this.log(".expectRight(" + playerIndex + "," + nextPlayerIndex + "," + category + "," + answer + ") {")
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
            this.log(".expectWin(" + playerIndex + "," + nextPlayerIndex + "," + category + "," + answer + ") {")
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
            foreach (SeqEvent e in sequence)
            {
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
                if (e.Expectation == true && e.MessageID.Equals("AskForCard"))
                {
                    Console.WriteLine("...");
                }
                if (!e.Expectation)
                {
                    errorMessagePrefix += ".send(" + e.PlayerIndex + "," + e.Message.MessageID() + ")\n";
                    checkForUnexpectedMessage();
                    clients[e.PlayerIndex].connection.Send(e.Message);
                }
                else
                {
                    errorMessagePrefix += ".expect<" + e.MessageID + ">(" + e.PlayerIndex + ", ...)\n";
                    Thread.Sleep(50);
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

}
