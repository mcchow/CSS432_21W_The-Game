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
        private QuestionSource questionSource;
        public const int MAX_WAITING_CONNECTIONS = 16;
        public const int MAX_CONCURRENT_CONNECTIONS = 1024;
        public volatile bool listening = false;

        private ConcurrentDictionary<Connection, Player> connectionMap;
        // database connection here? is thread safe?

        public Server(QuestionSource qsrc)
        {
            questionSource = qsrc;
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

                Question question = questionSource.GetQuestion(card.Card);
                TriviaQuestion Q = question.question;
                char corAns = question.answer;

                // save the correct answer into the room using the player's connection
                Player player = null;
                connectionMap.TryGetValue(c, out player);
                Room room = player.Room;
                room.answer = corAns;
            });
            protocol.RegisterMessageHandler<PlayerAnswer>((PlayerAnswer msg, Connection c) =>
            {
                Player player = null;
                connectionMap.TryGetValue(c, out player);
                Room room = player.Room;

                if (room == null || room.playerList[room.whosTurn] != player)
                {
                    return;
                }
                //TODO perhaps game rule logic should be moved into Room?
                if (msg.playerAns == room.answer)
                {
                    player.Points++;
                    if (player.Points > 6) //TODO 
                    {
                        Winner winner = new Winner();
                        winner.winner = player.Name;
                        foreach (Player p in room.playerList)
                        {
                            p.Connection.Send(winner);
                            return;
                        }
                    }
                }
                AnswerAndResult answerAndResult = new AnswerAndResult();
                answerAndResult.correctAnswer = room.answer;
                foreach (Player p in room.playerList)
                {
                    p.Connection.Send(answerAndResult);
                }
            });
            protocol.RegisterMessageHandler<Register>((Register registration, Connection c) =>
            {
                connectionMap.TryUpdate(c, new Player(registration, c), null);

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
                Player player = null;

                // check if player in connection map, if so remove
                if (connectionMap.TryGetValue(c, out player))
                {
                    connectionMap.TryUpdate(c, null, player);
                }

                c.Disconnect();
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
                Player player;
                connectionMap.TryGetValue(c, out player);
                Room room = player.Room;
                if (room == null)
                {
                    return;
                }
                room.playerList.Remove(player);
                player.Room = null;
                room.playerList[0].Connection.Send(new OpponentQuit());
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

        public void shutdown()
        {
            listening = false;
            foreach (Connection c in connectionMap.Keys)
            {
                c.Disconnect();
            }
        }

        public void listen()
        {
            Console.WriteLine("Server is Listening!");
            listening = true;
            socket.Listen(MAX_CONCURRENT_CONNECTIONS);
            while (listening)
            {
                socket.BeginAccept(OnConnect, null);
                connectionPool.Wait();
            }
        }
        static void Main(string[] args)
        {
            Server s = new Server(new SqliteQuestionSource());
            s.listen();
        }

        private void OnConnect(IAsyncResult ar)
        {
            Console.WriteLine("Connection Opened!");
            Socket sd = socket.EndAccept(ar);
            Connection c = new Connection(sd, protocol);
            connectionMap.TryAdd(c, null);
            try
            {
                c.RecieveLoop();
                Console.WriteLine("Exited RecieveLoop.");
            } catch (SocketException e)
            {
                Console.WriteLine("Connection closed unexpectedly: " + e.Message);
            }
            connectionPool.Release();
        }

    }
}
