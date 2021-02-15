using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace TriviaGameProtocol
{
    public class Connection
    {
        private Socket socket;
        private Protocol protocol;
        private bool runningRecieveLoop;
        private bool shouldStayConnected;
        public Connection(Socket socket, Protocol protocol)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            this.socket = socket;

            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }
            this.protocol = protocol;
            shouldStayConnected = true;
        }

        public Socket GetSocket()
        {
            return socket;
        }

        public void Disconnect()
        {
            shouldStayConnected = false;
            socket.Close(5);
        }

        /**
         * This method will block until the connection is closed or an invalid message is recieved.
         * As such, it should be called from its own thread.
         */
        public void RecieveLoop()
        {
            if (runningRecieveLoop)
            {
                throw new Exception("Cannot simultaneously run multiple recieve loops on a connection.");
            }
            try
            {
                runningRecieveLoop = true;
                while (socket.Connected && shouldStayConnected)
                {
                    byte[] messageLength = new byte[4];
                    int readSize = 0;
                    while (readSize < 4 && shouldStayConnected)
                    {
                        //Console.WriteLine("A: " + shouldStayConnected + socket.Connected);
                        int bytesRead = socket.Receive(messageLength, readSize, 4 - readSize, SocketFlags.None);
                        if (bytesRead == 0)
                        {
                            Disconnect();
                            break;
                        }
                        readSize += bytesRead;
                    }
                    if (!shouldStayConnected || !socket.Connected)
                    {
                        break;
                    }
                    Int32 messageSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(messageLength));
                    //TODO properly handle messages w/ size over some limit
                    readSize = 0;
                    byte[] message = new byte[messageSize];
                    while (readSize < messageSize && socket.Connected && shouldStayConnected)
                    {
                        //Console.WriteLine("B: " + shouldStayConnected);
                        int bytesRead = socket.Receive(message, readSize, messageSize - readSize, SocketFlags.None);
                        if (bytesRead == 0)
                        {
                            Disconnect();
                            break;
                        }
                        readSize += bytesRead;
                    }
                    if (!shouldStayConnected || !socket.Connected)
                    {
                        break;
                    }
                    protocol.ParseBytes(message, this);
                }
            } catch (SocketException e)
            {
                runningRecieveLoop = false;
                if (shouldStayConnected)
                {
                    throw e;
                }
            }
            runningRecieveLoop = false;
        }

        public void Send(MessageType message) {
            byte[] messageID = Encoding.UTF8.GetBytes(message.MessageID());
            byte[] messageBody = message.ToBytes();
            byte[] messageBytes = new byte[5 + messageID.Length + messageBody.Length];
            Int32 messageLength = IPAddress.HostToNetworkOrder(messageBody.Length + messageID.Length + 1);
            byte[] messageSize = BitConverter.GetBytes(messageLength);
            System.Buffer.BlockCopy(messageSize, 0, messageBytes, 0, 4);
            System.Buffer.BlockCopy(messageID, 0, messageBytes, 4, messageID.Length);
            messageBytes[4 + messageID.Length] = (byte)'\0';
            System.Buffer.BlockCopy(messageBody, 0, messageBytes, messageID.Length + 5, messageBody.Length);
            socket.Send(messageBytes);
        }
    }
}
