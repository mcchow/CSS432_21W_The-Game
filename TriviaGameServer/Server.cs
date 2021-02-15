using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using TriviaGameProtocol;
using System.Threading;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace TriviaGameServer
{
    public struct RecieveState {
        public Socket socket;
        public byte[] buffer;
    }

    public class Server
    {
        private Protocol protocol;
        private Socket socket;
        private SemaphoreSlim connectionPool;
        public const int MAX_WAITING_CONNECTIONS = 16;
        public const int MAX_CONCURRENT_CONNECTIONS = 1024;

        // database connection here? is thread safe?

        public Server()
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint serverEndPoint = new IPEndPoint(0, 8080);
            socket.Bind(serverEndPoint);
            SetupProtocol();
            connectionPool = new SemaphoreSlim(MAX_WAITING_CONNECTIONS, MAX_WAITING_CONNECTIONS);
        }

        private void SetupProtocol()
        {
            protocol = new Protocol();
            protocol.RegisterMessageHandler<ChosenCard>((ChosenCard card, Connection c) =>
            {
                Console.WriteLine("Chose Card: " + card.Card);  // here parse messgae in setupconn
                // call setUpConnToDatabase(string category) 
                //      where category = card
                // List<string> message = setUpConnToDatabase(string category)
                // string q = message[0]
                // TriviaQuestion Q = new TrviaQuestion();
                // Q.question = q;
                // string questionID = message[1];
                // ...

            });
            protocol.RegisterMessageHandler<Register>((Register registration, Connection c) =>
            {
                Console.WriteLine("Welcome " + registration.Name + "!");
                c.Send(new AskForCard());
            });
        }

        public void listen()
        {
            socket.Listen(MAX_CONCURRENT_CONNECTIONS);
            while (true)
            {
                socket.BeginAccept(OnConnect, null);
                connectionPool.Wait();
            }
        }
        static void Main(string[] args)
        {
            Server s = new Server();
            s.listen();
        }

        private void OnConnect(IAsyncResult ar)
        {
            Console.WriteLine("Connection Opened!");
            Socket sd = socket.EndAccept(ar);
            Connection c = new Connection(sd, protocol);
            try
            {
                c.RecieveLoop();
            } catch (SocketException e)
            {
                Console.WriteLine("Connection closed unexpectedly: " + e.Message);
            }
            connectionPool.Release();
        }

        private List<string> setUpConnToDatabase(string category)
        {
            List<string> message = new List<string>();

            // using Microsoft.Data.Sqlite
            using (var connection = new SqliteConnection("Data Source=TriviaGame.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT questionDescription, questionID, correctAnswer, optionA, optionB, optionC, optionD
                FROM Question
                WHERE category = $catCard  
                ORDER BY RANDOM()
                LIMIT 1
                ";
                command.Parameters.AddWithValue("$catCard", category);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string result;

                        for (int i = 0; i < 7; i++)
                        {
                            result = reader.GetString(i);
                            message.Add(result);
                        }
                    }
                }
            }

            return message;
        }


    }
}
