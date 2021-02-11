using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameProtocol
{
    public class AskForCard : MessageType
    {
        public override string MessageID()
        {
            return "AskForCard";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }

    public class AskForCardReader : MessageReader
    {
        public override MessageType ParseMessage(out int readSize, byte[] buffer, int offset, int bufferSize)
        {
            readSize = 0;
            return new AskForCard();
        }
    }


    public class ChosenCard : MessageType
    {
        public string Card;

        public override string MessageID()
        {
            return "ChosenCard";
        }

        public override byte[] ToBytes()
        {
            byte[] bytes = new Byte[Card.Length+1];
            bytes[Card.Length] = (byte)'\0';
            Encoding.UTF8.GetBytes(Card, 0, Card.Length, bytes, 4);
            return bytes;
        }
    }

    public class ChosenCardReader : MessageReader
    {
        private string Card = "";
        public override MessageType ParseMessage(out int readSize, byte[] buffer, int offset, int bufferSize)
        {
            bool foundNul = false;
            readSize = bufferSize;
            for (int i = offset; i < offset + bufferSize; ++i)
            {
                if (buffer[i] == (byte)'\0')
                {
                    readSize = i - offset;
                    break;
                }
            }
            string recieved = Encoding.UTF8.GetString(buffer, offset, readSize);
            if (foundNul)
            {
                ChosenCard choice = new ChosenCard();
                choice.Card = Card;
            }
            return null;
        }
    }
}
