using System;
using Xunit;
using TriviaGameProtocol;
using System.Text;

namespace ProtocolTests
{
    public class ProtocolTests
    {
        private class MockMessageType1 : MessageType
        {
            public byte[] from = null;
            public byte[] to = null;
            public override void FromBytes(byte[] bytes)
            {
                from = bytes;
            }

            public override string MessageID()
            {
                return "MockMessageOne";
            }

            public override byte[] ToBytes()
            {
                return to;
            }
        }

        private class MockMessageType2 : MessageType
        {
            public byte[] from = null;
            public byte[] to = null;
            public override void FromBytes(byte[] bytes)
            {
                from = bytes;
            }

            public override string MessageID()
            {
                return "MockMessageTwo";
            }

            public override byte[] ToBytes()
            {
                return to;
            }
        }

        [Fact]
        public void RegisterMessageHandlerThrowsOnNullMessageHandler()
        {
            Protocol p = new Protocol();
            Assert.Throws<ArgumentNullException>(() => {
                p.RegisterMessageHandler<MockMessageType1>(null);
            });
        }

        [Fact]
        public void ParseBytesThrowsOnNullByteArray()
        {
            Protocol p = new Protocol();
            Assert.ThrowsAny<Exception>(() =>
            {
                p.ParseBytes(null, null);
            });
        }

        [Fact]
        public void ParseBytesIgnoresUnregisteredMessageTypes()
        {
            Protocol p = new Protocol();
            byte[] bytes = Encoding.UTF8.GetBytes("MockMessageOne\01234");
            p.ParseBytes(bytes, null);
        }

        [Fact]
        public void ParseBytesCallsCorrectMessageHandlers()
        {
            Protocol p = new Protocol();
            int handler1Calls = 0;
            int handler2Calls = 0;
            int handler3Calls = 0;
            int handler4Calls = 0;
            MockMessageType1 msg1 = new MockMessageType1();
            MockMessageType2 msg2 = new MockMessageType2();
            MessageSender sender1 = (MessageType mt) => { };
            MessageSender sender2 = (MessageType mt) => { };
            p.RegisterMessageHandler<MockMessageType1>((MockMessageType1 msg, MessageSender send) =>
            {
                Assert.Equal("12\034", Encoding.UTF8.GetString(msg.from));
                Assert.Equal(sender1, send);
                handler1Calls++;
            });
            p.RegisterMessageHandler<MockMessageType1>((MockMessageType1 msg, MessageSender send) =>
            {
                Assert.Equal("12\034", Encoding.UTF8.GetString(msg.from));
                Assert.Equal(sender1, send);
                handler2Calls++;
            });
            p.RegisterMessageHandler<MockMessageType2>((MockMessageType2 msg, MessageSender send) =>
            {
                Assert.Equal("12\034", Encoding.UTF8.GetString(msg.from));
                Assert.Equal(sender2, send);
                handler3Calls++;
            });
            p.RegisterMessageHandler<MockMessageType2>((MockMessageType2 msg, MessageSender send) =>
            {
                Assert.Equal("12\034", Encoding.UTF8.GetString(msg.from));
                Assert.Equal(sender2, send);
                handler4Calls++;
            });

            byte[] bytes = Encoding.UTF8.GetBytes("MockMessageOne\012\034");
            p.ParseBytes(bytes, sender1);
            Assert.Equal(1, handler1Calls);
            Assert.Equal(1, handler2Calls);
            Assert.Equal(0, handler3Calls);
            Assert.Equal(0, handler4Calls);

            bytes = Encoding.UTF8.GetBytes("MockMessageTwo\012\034");
            p.ParseBytes(bytes, sender2);
            Assert.Equal(1, handler1Calls);
            Assert.Equal(1, handler2Calls);
            Assert.Equal(1, handler3Calls);
            Assert.Equal(1, handler4Calls);
        }
    }
}
