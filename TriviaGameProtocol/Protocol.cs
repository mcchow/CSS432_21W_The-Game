using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameProtocol
{
    public class Protocol
    {
        private Dictionary<string, MessageReader> messageReaders;
        private Dictionary<string, List<MessageHandlerWrapper>> messageHandlers;
        private StringReader sr;
        private string messageID;

        public void RegisterMessageType(MessageType messageType, MessageReader messageReader)
        {
            messageReaders.Add(messageType.MessageID(), messageReader);
        }

        private struct MessageHandlerWrapper
        {
            public MessageHandlerWrapper(object messageHandler, MessageHandler<MessageType> wrapper)
            {
                this.messageHandler = messageHandler;
                this.wrapper = wrapper;
            }
            public override bool Equals(object obj)
            {
                return messageHandler.Equals(obj);
            }
            public override int GetHashCode()
            {
                return messageHandler.GetHashCode();
            }
            object messageHandler;
            public MessageHandler<MessageType> wrapper;
        }

        public delegate void MH(MessageType mt);
        public delegate void MessageHandler<T>(T message) where T : MessageType;

        public void parseBytes(byte[] bytes)
        {
            int offset = 0;
            while (offset < bytes.Length - 1)
            {
                int readSize = 0;
                if (messageID == null)
                {
                    messageID = sr.ParseMessage(out readSize, bytes, bytes.Length);
                }
                if (messageID == null)
                {
                    throw new Exception("Unable to parse message type!");
                }
                if (!messageReaders.ContainsKey(messageID))
                {
                    throw new Exception("Unsupported message type.");
                }
                MessageType msg = messageReaders[messageID].ParseMessage(out readSize, bytes, offset, bytes.Length - offset);
                if (msg != null)
                {
                    foreach (MessageHandlerWrapper i in messageHandlers[messageID])
                    {
                        i.wrapper(msg);
                    }
                }
                offset += readSize;
            }
        }

        public void RegisterMessageHandler<T>(MessageHandler<T> messageHandler) where T : MessageType, new()
        {
            T messageType = new T();
            string messageID = messageType.MessageID();
            if (!messageReaders.ContainsKey(messageID))
            {
                throw new Exception("Unsupported message type.");
            }
            if (!messageHandlers.ContainsKey(messageID))
            {
                messageHandlers[messageID] = new List<MessageHandlerWrapper>();
            }
            MessageHandlerWrapper mh = new MessageHandlerWrapper(messageHandler, t => messageHandler((T)t));
            if (!messageHandlers[messageID].Contains(mh))
            {
                messageHandlers[messageID].Add(mh);
            }
        }
    }

    public class StringReader
    {
        private string str;
        public string ParseMessage(out int readSize, byte[] buffer, int bufferSize)
        {
            bool foundNul = false;
            readSize = 0;
            for (int i = 0; i < bufferSize; ++i)
            {
                if (buffer[i] == (byte)'\0')
                {
                    readSize = i;
                    break;
                }
            }
            string recieved = Encoding.UTF8.GetString(buffer, 0, readSize);
            str += recieved;
            if (foundNul)
            {
                string res = str;
                str = "";
                return res;
            }
            return null;
        }
    }

    public abstract class MessageReader
    {
        public abstract MessageType ParseMessage(out int readSize, byte[] buffer, int offset, int bufferSize);
    }

    public abstract class MessageType {
        public abstract string MessageID();

        public abstract byte[] ToBytes();
    }
}
