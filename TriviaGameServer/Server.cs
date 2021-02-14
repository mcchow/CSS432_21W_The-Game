using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using TriviaGameProtocol;
using System.Threading;

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
                Console.WriteLine("Chose Card: " + card.Card);
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
    }
}
