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

    public class JoinRoom : MessageType
    {
        public string RoomID;

        public override void FromBytes(byte[] bytes)
        {
            RoomID = Encoding.UTF8.GetString(bytes);
        }

        public override string MessageID()
        {
            return "JoinRoom";
        }

        public override byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(RoomID);
        }
    }

    public class LeaveRoom : MessageType
    {
        public override void FromBytes(byte[] bytes)
        {
        }

        public override string MessageID()
        {
            return "LeaveRoom";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }
    public class OpponentQuit : MessageType
    {
        public override void FromBytes(byte[] bytes)
        {
        }

        public override string MessageID()
        {
            return "OpponentQuit";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
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

        public override void FromBytes(byte[] bytes)
        {
            string bigString = Encoding.UTF8.GetString(bytes);
            string[] splitString = bigString.Split("\0");
            question = splitString[0];
            optionA = splitString[1];
            optionB = splitString[2];
            optionC = splitString[3];
            optionD = splitString[4];
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TriviaQuestion))
            {
                return false;
            }
            TriviaQuestion tq = (TriviaQuestion)obj;
            return question.Equals(tq.question) &&
                optionA.Equals(tq.optionA) &&
                optionB.Equals(tq.optionB) &&
                optionC.Equals(tq.optionC) &&
                optionD.Equals(tq.optionD);
        }

        public override string MessageID()
        {
            return "TriviaQuestion";
        }

        public override byte[] ToBytes()
        {
            int countBytesQuestion = ASCIIEncoding.Unicode.GetByteCount(question);
            int countBytesOpA = ASCIIEncoding.Unicode.GetByteCount(optionA);
            int countBytesOpB = ASCIIEncoding.Unicode.GetByteCount(optionB);
            int countBytesOpC = ASCIIEncoding.Unicode.GetByteCount(optionC);
            int countBytesOpD = ASCIIEncoding.Unicode.GetByteCount(optionD);
            
            string delim = "\0";
            int countBytesDelim = ASCIIEncoding.Unicode.GetByteCount(delim);
            byte[] delimBytes = Encoding.UTF8.GetBytes(delim);

            int totalByteCount = countBytesQuestion + countBytesOpA + countBytesOpB + countBytesOpC + countBytesOpD + (4 * countBytesDelim);

            byte[] triviaQues = new byte[totalByteCount];

            byte[] quesByte = Encoding.UTF8.GetBytes(question);
            byte[] opAByte = Encoding.UTF8.GetBytes(optionA);
            byte[] opBByte = Encoding.UTF8.GetBytes(optionB);
            byte[] opCByte = Encoding.UTF8.GetBytes(optionC);
            byte[] opDByte = Encoding.UTF8.GetBytes(optionD);

            quesByte.CopyTo(triviaQues, 0);
            delimBytes.CopyTo(triviaQues, countBytesQuestion);

            opAByte.CopyTo(triviaQues, countBytesQuestion + countBytesDelim);
            delimBytes.CopyTo(triviaQues, countBytesQuestion + countBytesDelim + countBytesOpA);

            opBByte.CopyTo(triviaQues, countBytesQuestion + (2 * countBytesDelim) + countBytesOpA);
            delimBytes.CopyTo(triviaQues, countBytesQuestion + (2 * countBytesDelim) + countBytesOpA + countBytesOpB);

            opCByte.CopyTo(triviaQues, countBytesQuestion + (3 * countBytesDelim) + countBytesOpA + countBytesOpB);
            delimBytes.CopyTo(triviaQues, countBytesQuestion + (3 * countBytesDelim) + countBytesOpA + countBytesOpB + countBytesOpC);

            opDByte.CopyTo(triviaQues, countBytesQuestion + (4 * countBytesDelim) + countBytesOpA + countBytesOpB + countBytesOpC);
            delimBytes.CopyTo(triviaQues, countBytesQuestion + (4 * countBytesDelim) + countBytesOpA + countBytesOpB + countBytesOpC + countBytesOpD);

            return triviaQues;
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

    public class NextPlayerTurn : MessageType
    {
        public int whosTurn;
        public int curNumCards;   // curNumCards of other player

        public override void FromBytes(byte[] bytes)
        {
            byte[] whosTurnBytes = new byte[4];
            byte[] curNumCardsBytes = new byte[4];

            for (int i = 0; i < bytes.Length; i++)
            {
                if (i <= 4)
                {
                    whosTurnBytes[i] = bytes[i];
                }
                else
                {
                    curNumCardsBytes[i] = bytes[i];
                }
            }

            whosTurn = BitConverter.ToInt32(whosTurnBytes);
            curNumCards = BitConverter.ToInt32(curNumCardsBytes);
        }

        public override string MessageID()
        {
            return "NextPlayerTurn";
        }

        public override byte[] ToBytes()
        {
            // an int in a 64 bit system is 4 bytes
            byte[] nextPlayerTurn = new byte[8];

            byte[] whosTurnByte = BitConverter.GetBytes(whosTurn);
            whosTurnByte.CopyTo(nextPlayerTurn, 0);

            byte[] curNumCardsByte = BitConverter.GetBytes(curNumCards);
            curNumCardsByte.CopyTo(nextPlayerTurn, 4);

            return nextPlayerTurn;
        }
    }

    public class Winner : MessageType
    {
        public string winner;

        public override void FromBytes(byte[] bytes)
        {
            winner = Encoding.UTF8.GetString(bytes);
        }

        public override string MessageID()
        {
            return "Winner";
        }

        public override byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(winner);
        }
    }

    public class ListRoomsRequest : MessageType
    {
        public override void FromBytes(byte[] bytes)
        {}

        public override string MessageID()
        {
            return "ListRoomsRequest";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }

    public class RoomEntry : MessageType
    {
        public string roomID;
        public string player1;
        public string player2;

        public override void FromBytes(byte[] bytes)
        {
            string bigString = Encoding.UTF8.GetString(bytes);
            string[] splitString = bigString.Split("\0");
            roomID = splitString[0];
            player1 = splitString[1];
            player2 = splitString[2];
        }

        public override string MessageID()
        {
            return "RoomEntry";
        }

        public override byte[] ToBytes()
        {
            int countBytesRoomID = ASCIIEncoding.Unicode.GetByteCount(roomID);
            int countBytesP1 = ASCIIEncoding.Unicode.GetByteCount(player1);
            int countBytesP2 = ASCIIEncoding.Unicode.GetByteCount(player2);

            string delim = "\0";
            int countBytesDelim = ASCIIEncoding.Unicode.GetByteCount(delim);
            byte[] delimBytes = Encoding.UTF8.GetBytes(delim);

            int totalByteCount = countBytesRoomID + countBytesP1 + countBytesP2 + (2 * countBytesDelim);

            byte[] roomEntry = new byte[totalByteCount];

            byte[] roomIDByte = Encoding.UTF8.GetBytes(roomID);
            byte[] player1Byte = Encoding.UTF8.GetBytes(player1);
            byte[] player2Byte = Encoding.UTF8.GetBytes(player2);

            roomIDByte.CopyTo(roomEntry, 0);
            delimBytes.CopyTo(roomEntry, countBytesRoomID);

            player1Byte.CopyTo(roomEntry, countBytesRoomID + countBytesDelim);

            delimBytes.CopyTo(roomEntry, countBytesRoomID + countBytesDelim + countBytesP1);

            player2Byte.CopyTo(roomEntry, countBytesRoomID + countBytesP1 + (2 * countBytesDelim));

            return roomEntry;
        }
    }

    public class Unregister : MessageType
    {
        public string name;

        public override void FromBytes(byte[] bytes)
        {
            name = Encoding.UTF8.GetString(bytes);
        }

        public override string MessageID()
        {
            return "Unregister";
        }

        public override byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(name);
        }
    }

    public class ClientDisconnect : MessageType
    {
        public override void FromBytes(byte[] bytes)
        {}

        public override string MessageID()
        {
            return "ClientDisconnect";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }

}

