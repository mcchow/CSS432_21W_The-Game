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
        private readonly object serverLock = new object();
        private bool mListening = false;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        public bool listening
        {
            get
            {
                return mListening;
            }
            set
            {
                lock(serverLock)
                {
                    mListening = value;
                }
            }
        }
        private int port;

        private ConcurrentDictionary<Connection, Player> connectionMap;
        private ConcurrentDictionary<string, Room> rooms;

        public Server(int port, QuestionSource qsrc)
        {
            this.port = port;
            questionSource = qsrc;
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(serverEndPoint);
            SetupProtocol();
            connectionPool = new SemaphoreSlim(MAX_WAITING_CONNECTIONS, MAX_WAITING_CONNECTIONS);
            connectionMap = new ConcurrentDictionary<Connection, Player>();
            rooms = new ConcurrentDictionary<string, Room>();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }
        private void handleConnectionClosedException(ConnectionClosedException e)
        {
            Player player = null;
            connectionMap.TryGetValue(e.Connection(), out player);
            if (player == null)
            {
                Console.WriteLine("Connection to unknown player closed");
            }
            else
            {
                Console.WriteLine("Connection to Player: " + player.Name + "Closed");

                Room room = player.Room;
                if (room != null)
                {
                    player.Room = null;
                    room.TryLeave(player);
                    if (room.playerOne != null)
                    {
                        room.playerOne.Room = null;
                        room.playerOne.Connection.Send(new OpponentQuit());
                    }
                    if (room.playerTwo != null)
                    {
                        room.playerTwo.Room = null;
                        room.playerTwo.Connection.Send(new OpponentQuit());
                    }
                    Room removed;
                    rooms.TryRemove(room.roomID, out removed);
                }
            }
        }
        private void SetupProtocol()
        {
            protocol = new Protocol();
            protocol.RegisterMessageHandler<ChosenCard>((ChosenCard card, Connection c) =>
            {
                try
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
                    room.cardCategory = card.Card;
                    c.Send(Q);
                }
                catch (ConnectionClosedException e)
                {
                    Console.Write("In ChosenCard Handler: ");
                    handleConnectionClosedException(e);
                }
            });
            protocol.RegisterMessageHandler<PlayerAnswer>((PlayerAnswer msg, Connection c) =>
            {
                try
                {
                    Player player = null;
                    connectionMap.TryGetValue(c, out player);
                    Room room = player.Room;

                    RoomPlayer roomPlayer = room.playerOne == player ? RoomPlayer.PlayerOne : RoomPlayer.PlayerTwo;

                    if (room == null || room.WhosTurn != roomPlayer)
                    {
                        return;
                    }

                    //TODO perhaps game rule logic should be moved into Room?
                    if (msg.playerAns == room.answer)
                    {
                        player.Points++;
                        player.CollectedCards.Add(room.cardCategory);
                        if (player.CollectedCards.Count >= 6)
                        {
                            Winner winner = new Winner();
                            winner.winner = player.Name;
                            room.playerOne.Connection.Send(winner);
                            room.playerTwo.Connection.Send(winner);
                            return;
                        }
                    }
                    else
                    {
                        // set next player's turn
                        room.WhosTurn = roomPlayer == RoomPlayer.PlayerOne ? RoomPlayer.PlayerTwo : RoomPlayer.PlayerOne;
                    }

                    AnswerAndResult answerAndResult = new AnswerAndResult();
                    answerAndResult.correctAnswer = room.answer;
                    answerAndResult.whosTurn = roomPlayer == RoomPlayer.PlayerTwo ? 1 : 2;
                    answerAndResult.numCards = player.Points;

                    room.playerOne.Connection.Send(answerAndResult);
                    room.playerTwo.Connection.Send(answerAndResult);
                    if (msg.playerAns == room.answer)
                    {
                        // got it right, get to choose another category
                        c.Send(new AskForCard());
                    }
                    else
                    {
                        // send opponent request for category choice
                        if (roomPlayer == RoomPlayer.PlayerOne)
                        {
                            room.playerTwo.Connection.Send(new AskForCard());
                        }
                        else
                        {
                            room.playerOne.Connection.Send(new AskForCard());
                        }
                    }
                }
                catch (ConnectionClosedException e)
                {
                    Console.Write("In PlayerAnswer Handler: ");
                    handleConnectionClosedException(e);
                }

            });
            protocol.RegisterMessageHandler<Register>((Register registration, Connection c) =>
            {
                connectionMap.TryUpdate(c, new Player(registration, c), null);

                Console.WriteLine("Welcome " + registration.Name + "!");
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

                if (player != null)
                {
                    Room room = player.Room;
                    if (room != null)
                    {
                        player.Room = null;
                        room.TryLeave(player);
                        if (room.playerOne != null)
                        {
                            room.playerOne.Room = null;
                            room.playerOne.Connection.Send(new OpponentQuit());
                        }
                        if (room.playerTwo != null)
                        {
                            room.playerTwo.Room = null;
                            room.playerTwo.Connection.Send(new OpponentQuit());
                        }
                        Room removed;
                        rooms.TryRemove(room.roomID, out removed);
                    }
                }

                c.Disconnect();
            });
            protocol.RegisterMessageHandler<CreateRoom>((CreateRoom req, Connection c) =>
            {
                try
                {
                    Player player = null;
                    connectionMap.TryGetValue(c, out player);
                    if (player == null)
                    {
                        return;
                    }

                    Room room = new Room();
                    player.Room = room;
                    room.TryJoin(player);
                    string roomID = Guid.NewGuid().ToString();
                    room.roomID = roomID;
                    rooms.TryAdd(roomID, room);
                }
                catch (ConnectionClosedException e)
                {
                    Console.Write("In CreateRoom Handler: ");
                    handleConnectionClosedException(e);
                }
            });
            protocol.RegisterMessageHandler<JoinRoom>((JoinRoom req, Connection c) =>
            {
                try
                {
                    Room room;
                    if (!rooms.ContainsKey(req.RoomID))
                    {
                        c.Send(new RoomUnavailable("Room closed."));
                        return;
                    }

                    room = rooms[req.RoomID];

                    Player player = null;
                    connectionMap.TryGetValue(c, out player);
                    if (player == null)
                    {
                        return;
                    }

                    if (!room.TryJoin(player))
                    {
                        c.Send(new RoomUnavailable("Room full."));
                        return;
                    }

                    player.Room = room;

                    room.WhosTurn = RoomPlayer.PlayerOne;

                    room.playerOne.Connection.Send(new AskForCard()); // player one goes first
                    room.playerTwo.Connection.Send(new NextPlayerTurn(1, 0));
                } catch (ConnectionClosedException e)
                {
                    Console.Write("In JoinRoom Handler: ");
                    handleConnectionClosedException(e);
                }
            });

            protocol.RegisterMessageHandler<LeaveRoom>((LeaveRoom req, Connection c) =>
            {
                try
                {
                    Player player;
                    connectionMap.TryGetValue(c, out player);
                    if (player == null)
                    {
                        return;
                    }
                    Room room = player.Room;
                    if (room == null)
                    {
                        return;
                    }
                    player.Room = null;
                    room.TryLeave(player);
                    if (room.playerOne != null)
                    {
                        room.playerOne.Room = null;
                        room.playerOne.Connection.Send(new OpponentQuit());
                    }
                    if (room.playerTwo != null)
                    {
                        room.playerTwo.Room = null;
                        room.playerTwo.Connection.Send(new OpponentQuit());
                    }
                    Room removed;
                    rooms.TryRemove(room.roomID, out removed);
                }
                catch (ConnectionClosedException e)
                {
                    Console.Write("In LeaveRoom Handler: ");
                    handleConnectionClosedException(e);
                }
            });
            protocol.RegisterMessageHandler<ListRoomsRequest>((ListRoomsRequest req, Connection c) =>
            {
                try
                {
                    RoomEntry roomEntry = new RoomEntry();
                    foreach (KeyValuePair<string, Room> i in rooms)
                    {
                        roomEntry.roomID = i.Key;
                        roomEntry.player1 = "";
                        roomEntry.player2 = "";
                        if (i.Value.playerOne != null)
                        {
                            roomEntry.player1 = i.Value.playerOne.Name;
                        }
                        if (i.Value.playerTwo != null)
                        {
                            roomEntry.player2 = i.Value.playerTwo.Name;
                        }
                        c.Send(roomEntry);
                    }
                }
                catch (ConnectionClosedException e)
                {
                    Console.Write("In ListRoomsRequest Handler: ");
                    handleConnectionClosedException(e);
                }
            });
        }

        public void shutdown()
        {
            listening = false;
            cancellationTokenSource.Cancel();
            foreach (Connection c in connectionMap.Keys)
            {
                c.Disconnect();
            }
        }

        public void listen()
        {
            Console.WriteLine("Starting CSS432 Trivia Game Server");
            Console.WriteLine("Server is Listening on port "+port+"!");
            listening = true;
            socket.Listen(MAX_CONCURRENT_CONNECTIONS);
            while (listening)
            {
                socket.BeginAccept(OnConnect, null);
                try
                {
                    connectionPool.Wait(cancellationToken);
                } catch
                {
                    break;
                }
            }
        }
        static void Main(string[] args)
        {
            Server s = new Server(8087, new SqliteQuestionSource());
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
