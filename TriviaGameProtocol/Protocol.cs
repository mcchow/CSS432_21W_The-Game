using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameProtocol
{
    /**
     * 
     */
    public class Protocol
    {
        private Dictionary<string, List<MessageHandlerWrapper>> messageHandlers;

        private struct MessageHandlerWrapper
        {
            public MessageHandlerWrapper(object messageHandler, MessageHandlerWrapperFunc wrapper)
            {
                this.messageHandler = messageHandler;
                this.wrapper = wrapper;
            }
            public override bool Equals(object obj)
            {
                return messageHandler.Equals(((MessageHandlerWrapper)obj).messageHandler);
            }
            public override int GetHashCode()
            {
                return messageHandler.GetHashCode();
            }
            object messageHandler;
            public MessageHandlerWrapperFunc wrapper;
        }

        public delegate void MH(MessageType mt);
        private delegate void MessageHandlerWrapperFunc(byte[] b, Connection c);
        public delegate void MessageHandler<T>(T message, Connection C) where T : MessageType;

        public Protocol()
        {
            messageHandlers = new Dictionary<string, List<MessageHandlerWrapper>>();
        }
        /**
         * bytes must contain one complete message.
         */
        public void ParseBytes(byte[] bytes, Connection connection)
        {
            int messageIDSize = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (bytes[i] == (byte)'\0')
                {
                    messageIDSize = i;
                    break;
                }
            }
            string messageID = Encoding.UTF8.GetString(bytes, 0, messageIDSize);
            if (!messageHandlers.ContainsKey(messageID))
            {
                return;
            }
            int bodySize = bytes.Length - messageIDSize - 1;
            byte[] messageBody = new byte[bodySize];
            System.Buffer.BlockCopy(bytes, messageIDSize + 1, messageBody, 0, bodySize);
            foreach (MessageHandlerWrapper i in messageHandlers[messageID])
            {
                i.wrapper(messageBody, connection);
            }
        }

        /**
         * This method serves the dual purpose of registering message types and message handlers.
         * When parseBytes recieves a message of type T, it will convert the bytes into an instance
         * of the appropriate message type and pass that message, along with a MessageSender
         * delegate (to be used to send replies of any message type) to all the registered message
         * handlers associated with the message type. Note that message types must be uniquely
         * identifiable by the string returned by their MessageID method.
         */
        virtual public void RegisterMessageHandler<T>(MessageHandler<T> messageHandler) where T : MessageType, new()
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            T messageType = new T();
            string messageID = messageType.MessageID();
            if (!messageHandlers.ContainsKey(messageID))
            {
                messageHandlers[messageID] = new List<MessageHandlerWrapper>();
            }
            MessageHandlerWrapper mh = new MessageHandlerWrapper(messageHandler, (b, c) => {
                T t = new T();
                t.FromBytes(b);
                messageHandler(t, c);
            });
            if (!messageHandlers[messageID].Contains(mh))
            {
                messageHandlers[messageID].Add(mh);
            }
        }
    }

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public abstract class MessageType
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public abstract string MessageID();

        public abstract void FromBytes(byte[] bytes);

        public abstract byte[] ToBytes();

        public abstract override bool Equals(object obj);
    }
}
