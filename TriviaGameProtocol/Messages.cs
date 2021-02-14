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
            return Encoding.UTF8.GetBytes(Card);
        }
    }

    public class Register : MessageType
    {
        public string Name;
        public override void FromBytes(byte[] bytes)
        {
            Name = Encoding.UTF8.GetString(bytes);
        }

        public override string MessageID()
        {
            return "Register";
        }

        public override byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(Name);
        }
    }

}
