using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using TriviaGameProtocol;
using System.Threading;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Collections.Concurrent;

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

        private ConcurrentDictionary<Connection, Player> connectionMap;
        // database connection here? is thread safe?

        public Server()
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint serverEndPoint = new IPEndPoint(0, 8080);
            socket.Bind(serverEndPoint);
            SetupProtocol();
            connectionPool = new SemaphoreSlim(MAX_WAITING_CONNECTIONS, MAX_WAITING_CONNECTIONS);
            connectionMap = new ConcurrentDictionary<Connection, Player>();
        }

        private void SetupProtocol()
        {
            protocol = new Protocol();
            protocol.RegisterMessageHandler<ChosenCard>((ChosenCard card, Connection c) =>
            {
                Console.WriteLine("Chose Card: " + card.Card); 

                List<string> message = setUpConnToDatabase(card.Card);
                
                string q = message[0];
                string opA = message[2];
                string opB = message[3];
                string opC = message[4];
                string opD = message[5];
                TriviaQuestion Q = new TriviaQuestion(); 
                Q.question = q;
                Q.optionA = opA;
                Q.optionA = opB;
                Q.optionA = opC;
                Q.optionA = opD;

                // string questionID = message[1];

                char corAns = char.Parse(message[1]);
                AnswerAndResult AandR = new AnswerAndResult();
                AandR.correctAnswer = corAns;
                // hashmap connection to player (get curNumCards from Player and whosTurn from Room in Player when call 
                // AnswerAndResult handler?) 


                //TODO Complete this message handler


            });
            protocol.RegisterMessageHandler<PlayerAnswer>((PlayerAnswer msg, Connection c) =>
            {
                //TODO
                // Get Room instance
                // Check if it is player's turn to answer (validation)
                // if answer is correct:
                //   increment players correct answer count
                //   check win condition
                //   if player won:
                //     Send Winner message to both players
                //     return
                // send AnswerAndResult mesage to both players
            });
            protocol.RegisterMessageHandler<Register>((Register registration, Connection c) =>
            {
                connectionMap.TryAdd(c, new Player(registration));

                Console.WriteLine("Welcome " + registration.Name + "!");
                c.Send(new AskForCard());
            });
            protocol.RegisterMessageHandler<Unregister>((Unregister msg, Connection c) =>
            {
                Player player;
                connectionMap.TryGetValue(c, out player);
                connectionMap.TryUpdate(c, null, player);
            });
            protocol.RegisterMessageHandler<ClientDisconnect>((ClientDisconnect msg, Connection c) =>
            {
                //TODO What do we need to do here?
            });
            protocol.RegisterMessageHandler<JoinRoom>((JoinRoom req, Connection c) =>
            {
                //TODO
                // Check if req.RoomID is a valid room ID
                // Get Room instance
                // if room full:
                //   send RoomFull message to player
                // else:
                //   Update connection-room mapping to reference room instance
                //   Update room instance to reference/include player info
                // choose who goes first, set turn info in room instance
                // send AskForCard to player who goes first
                // send NextPlayerTurn to player who goes second
            });
            protocol.RegisterMessageHandler<LeaveRoom>((LeaveRoom req, Connection c) =>
            {
                // Get Room instance
                // remove player info from room list
                // update connection room mapping to map c to null
                // Send OpponentQuit message to opponent
            });
            protocol.RegisterMessageHandler<ListRoomsRequest>((ListRoomsRequest req, Connection c) =>
            {
                /// Sends one RoomList message to the client for each room that exists.
                
                //TODO replace mock data below with actual room list data
                RoomEntry rl = new RoomEntry();
                rl.roomID = "First Room";
                rl.player1 = "Alice";
                rl.player2 = "Bob";
                c.Send(rl);
                rl.roomID = "Second Room";
                rl.player1 = "Josh";
                rl.player2 = "";
                c.Send(rl);
                rl.roomID = "Third Room";
                rl.player1 = "";
                rl.player2 = "";
                c.Send(rl);
                rl.roomID = "Another Room";
                rl.player1 = "";
                rl.player2 = "Player Two";
                c.Send(rl);
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
                // got rid of questionID
                command.CommandText =
                @"
                SELECT questionDescription, correctAnswer, optionA, optionB, optionC, optionD
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

                        for (int i = 0; i < 6; i++)
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
