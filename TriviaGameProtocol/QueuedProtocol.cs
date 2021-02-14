using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace TriviaGameProtocol
{
    public class QueuedProtocol : Protocol
    {
        private struct QueuedMessage
        {
            public QueuedMessage(MessageType message, Connection connection)
            {
                this.message = message;
                this.connection = connection;
            }
            public MessageType message;
            public Connection connection;
        }

        private delegate void MessageHandlerWrapperFunc(MessageType message, Connection connection);

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

        private ConcurrentQueue<QueuedMessage> messageQueue;
        private Dictionary<string, List<MessageHandlerWrapper>> messageHandlers;

        public QueuedProtocol()
        {
            messageQueue = new ConcurrentQueue<QueuedMessage>();
            messageHandlers = new Dictionary<string, List<MessageHandlerWrapper>>();
        }

        public override void RegisterMessageHandler<T>(MessageHandler<T> messageHandler)
        {
            T messageType = new T();
            string messageID = messageType.MessageID();
            if (!messageHandlers.ContainsKey(messageID))
            {
                // Register a message handler to add the message to our queue only if we haven't
                // already registered one.
                base.RegisterMessageHandler<T>((message, connection) =>
                {
                    messageQueue.Enqueue(new QueuedMessage(message, connection));
                });
                messageHandlers[messageID] = new List<MessageHandlerWrapper>();
            }
            MessageHandlerWrapper mh = new MessageHandlerWrapper(messageHandler, (m, c) =>
            {
                messageHandler((T)m, c);
            });
            if (!messageHandlers[messageID].Contains(mh))
            {
                messageHandlers[messageID].Add(mh);
            }
        }

        /** Handles all queued messages. */
        public void HandleMessages()
        {
            QueuedMessage qm;
            while (messageQueue.TryDequeue(out qm))
            {
                string messageID = qm.message.MessageID();
                if (!messageHandlers.ContainsKey(messageID))
                {
                    continue;
                }
                foreach (MessageHandlerWrapper mh in messageHandlers[messageID])
                {
                    mh.wrapper(qm.message, qm.connection);
                }
            }
        }
    }
}
