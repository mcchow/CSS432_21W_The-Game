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
        Connection(Socket socket, Protocol protocol)
        {
            this.socket = socket;
            this.protocol = protocol;
        }

        public void RecieveLoop()
        {
            while (true) // Should loop until connection is closed.
            {
                byte[] messageLength = new byte[4];
                int readSize = 0;
                while (readSize < 4)
                {
                    readSize += socket.Receive(messageLength, readSize, 4 - readSize, SocketFlags.None);
                }
                Int32 messageSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(messageLength));
                readSize = 0;
                byte[] message = new byte[messageSize];
                while (readSize < messageSize)
                {
                    readSize += socket.Receive(message, readSize, messageSize - readSize, SocketFlags.None);
                }
                protocol.ParseBytes(message, this.Send);
            }
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
