using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace TriviaGameServer
{

    public struct ConnectionState {
        public ConnectionState(Socket sd, string foo)
        {
            this.socket = sd;
            this.foo = foo;
        }
        public Socket socket;
        public string foo;
    }

    public struct RecieveState {
        public Socket socket;
        public byte[] buffer;
    }

    public class Program
    {
        public static int foo()
        {
            return 3;
        }

        static void Main(string[] args)
        {
            Socket sd = new Socket(SocketType.Stream, ProtocolType.Tcp);
            sd.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint serverEndPoint = new IPEndPoint(0, 8080);
            sd.Bind(serverEndPoint);
            sd.Listen(16);
            ConnectionState cs;
            while (true)
            {
                sd.BeginAccept(new AsyncCallback(OnConnect), new ConnectionState(sd, "TEST 1 2 3"));
            }
        }

        static void OnConnect(IAsyncResult ar)
        {
            ConnectionState cs = (ConnectionState)ar.AsyncState;
            Console.WriteLine(cs.foo);
            Socket sd = cs.socket.EndAccept(ar);

            RecieveState msg = new RecieveState();
            msg.socket = sd;
            msg.buffer = new byte[128];
            sd.BeginReceive(msg.buffer, 0, msg.buffer.Length, 0, new AsyncCallback(OnRecieve), msg);
        }

        static void OnRecieve(IAsyncResult ar)
        {
            RecieveState rs = (RecieveState)ar.AsyncState;
            Socket sd = rs.socket;
            int bytesRead = sd.EndReceive(ar);
            string recieved = Encoding.UTF8.GetString(rs.buffer, 0, bytesRead);
            Console.Out.WriteLine(recieved);
        }
    }
}
