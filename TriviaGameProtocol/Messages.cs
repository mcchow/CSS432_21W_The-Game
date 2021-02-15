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

    // mariana: TriviaQuestion, PlayerAnswer, and AnswerAndResult
    public class TriviaQuestion : MessageType    
    {
        public string question;
        public string optionA;
        public string optionB;
        public string optionC;
        public string optionD;

        int lenQuestion;
        int lenOpA;
        int lenOpB;
        int lenOpC;
        int lenOpD;
        int lenTotal;

        public override void FromBytes(byte[] bytes)
        {
            lenQuestion = question.Length;
            lenOpA = lenQuestion + optionA.Length;
            lenOpB = lenOpA + optionB.Length;
            lenOpC = lenOpB + optionC.Length;
            lenOpD = lenOpC + optionD.Length;

            question = BitConverter.ToString(bytes, 0);
            optionA = BitConverter.ToString(bytes, lenQuestion + 1);
            optionB = BitConverter.ToString(bytes, lenOpA + 1);
            optionC = BitConverter.ToString(bytes, lenOpB + 1);
            optionD = BitConverter.ToString(bytes, lenOpC + 1);
        }

        public override string MessageID()
        {
            return "TriviaQuestion";
        }

        public override byte[] ToBytes()
        {
            lenTotal = lenQuestion + lenOpA + lenOpB + lenOpC + lenOpD;

            byte[] trivQues = new byte[lenTotal];
            byte[] bytesQues = Encoding.UTF8.GetBytes(question);
            bytesQues.CopyTo(trivQues, 0);
            byte[] bytesOpA = Encoding.UTF8.GetBytes(optionA);
            bytesOpA.CopyTo(trivQues, lenQuestion + 1);
            byte[] bytesOpB = Encoding.UTF8.GetBytes(optionB);
            bytesOpB.CopyTo(trivQues, lenOpA + 1);
            byte[] bytesOpC = Encoding.UTF8.GetBytes(optionC);
            bytesOpC.CopyTo(trivQues, lenOpB + 1);
            byte[] bytesOpD = Encoding.UTF8.GetBytes(optionD);
            bytesOpD.CopyTo(trivQues, lenOpC + 1);
            return trivQues;
        }
    }

    public class PlayerAnswer : MessageType     
    {
        public char playerAns;

        public override void FromBytes(byte[] bytes)
        {
            playerAns = (char)bytes[0];
        }

        public override string MessageID()
        {
            return "PlayerAnswer";
        }

        public override byte[] ToBytes()
        {
            byte[] pAns = new byte[0];
            pAns[0] = (byte)playerAns;
            return pAns;
        }
    }

    public class AnswerAndResult : MessageType     
    {
        public char correctAnswer;
        private int numCards;
        private int whosTurn;

        public override void FromBytes(byte[] bytes)
        {
            correctAnswer = (char)bytes[0];
            numCards = BitConverter.ToInt32(bytes, 1);
            whosTurn = BitConverter.ToInt32(bytes, 5);
        }

        public override string MessageID()
        {
            return "AnswerAndResult";
        }

        public override byte[] ToBytes()
        {
            byte[] AnsAndRes = new byte[9];
            AnsAndRes[0] = (byte)correctAnswer;
            byte[] bArrNumCards = BitConverter.GetBytes(numCards);
            bArrNumCards.CopyTo(AnsAndRes, 1);
            byte[] bArrWhosTurn = BitConverter.GetBytes(whosTurn);
            bArrWhosTurn.CopyTo(AnsAndRes, 5);
            return AnsAndRes;
        }
    }
}

