using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameProtocol
{
    public class AskForCard : MessageType
    {
        public override void FromBytes(byte[] bytes)
        {
        }

        public override string MessageID()
        {
            return "AskForCard";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }

    public class ChosenCard : MessageType
    {
        public string Card;

        public override void FromBytes(byte[] bytes)
        {
            Card = Encoding.UTF8.GetString(bytes);
        }

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

}
