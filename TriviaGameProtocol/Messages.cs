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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is AskForCard))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class ChosenCard : MessageType
    {
        public string Card;

        public ChosenCard()
        {
            Card = null;
        }

        public ChosenCard(string Card)
        {
            this.Card = Card;
        }

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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ChosenCard))
            {
                return false;
            }

            ChosenCard cc = (ChosenCard)obj;

            return Card.Equals(cc.Card);
        }
    }

    public class Register : MessageType
    {
        public string Name;

        public Register()
        {
            Name = null;
        }

        public Register(string name)
        {
            Name = name;
        }

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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Register))
            {
                return false;
            }

            Register r = (Register)obj;

            return Name.Equals(r.Name);
        }
    }

    public class CreateRoom : MessageType
    {
        public override bool Equals(object obj)
        {
            return obj != null && (obj is CreateRoom);
        }

        public override void FromBytes(byte[] bytes)
        {
        }

        public override string MessageID()
        {
            return "CreateRoom";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }

    public class JoinRoom : MessageType
    {
        public string RoomID;

        public JoinRoom()
        {
            RoomID = null;
        }

        public JoinRoom(string roomID)
        {
            RoomID = roomID;
        }
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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is JoinRoom))
            {
                return false;
            }

            JoinRoom jr = (JoinRoom)obj;

            return RoomID.Equals(jr.RoomID);
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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LeaveRoom))
            {
                return false;
            }
            else
            {
                return true;
            }
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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is OpponentQuit))
            {
                return false;
            }
            else
            {
                return true;
            }
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

        public TriviaQuestion()
        {

        }

        public TriviaQuestion(string question, string optionA, string optionB, string optionC, string optionD)
        {
            this.question = question;
            this.optionA = optionA;
            this.optionB = optionB;
            this.optionC = optionC;
            this.optionD = optionD;
        }

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
            int countBytesQuestion = Encoding.UTF8.GetByteCount(question);
            int countBytesOpA = Encoding.UTF8.GetByteCount(optionA);
            int countBytesOpB = Encoding.UTF8.GetByteCount(optionB);
            int countBytesOpC = Encoding.UTF8.GetByteCount(optionC);
            int countBytesOpD = Encoding.UTF8.GetByteCount(optionD);
            
            string delim = "\0";
            int countBytesDelim = Encoding.UTF8.GetByteCount(delim);
            byte[] delimBytes = Encoding.UTF8.GetBytes(delim);

            int totalByteCount = countBytesQuestion + countBytesOpA + countBytesOpB + countBytesOpC + countBytesOpD + (5 * countBytesDelim);

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

        public PlayerAnswer()
        {
        }
        public PlayerAnswer(char playerAns)
        {
            this.playerAns = playerAns;
        }

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
            byte[] pAns = new byte[1];
            pAns[0] = (byte)playerAns;
            return pAns;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PlayerAnswer))
            {
                return false;
            }

            PlayerAnswer pa = (PlayerAnswer)obj;

            return playerAns.Equals(pa.playerAns);
        }
    }

    public class AnswerAndResult : MessageType     
    {
        public char correctAnswer;
        public int numCards;
        public int whosTurn;

        public AnswerAndResult()
        {

        }

        public AnswerAndResult(char correctAnswer, int numCards, int whosTurn)
        {
            this.correctAnswer = correctAnswer;
            this.numCards = numCards;
            this.whosTurn = whosTurn;
        }

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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is AnswerAndResult))
            {
                return false;
            }

            AnswerAndResult aar = (AnswerAndResult)obj;

            return correctAnswer.Equals(aar.correctAnswer) && numCards.Equals(aar.numCards) && whosTurn.Equals(aar.whosTurn);
        }
    }

    public class NextPlayerTurn : MessageType
    {
        public int whosTurn;
        public int curNumCards;   // curNumCards of other player

        public NextPlayerTurn()
        {

        }
        public NextPlayerTurn(int whosTurn, int curNumCards)
        {
            this.whosTurn = whosTurn;
            this.curNumCards = curNumCards;
        }

        public override void FromBytes(byte[] bytes)
        {
            byte[] whosTurnBytes = new byte[4];
            byte[] curNumCardsBytes = new byte[4];

            for (int i = 0; i < bytes.Length; i++)
            {
                if (i < 4)
                {
                    whosTurnBytes[i] = bytes[i];
                }
                else
                {
                    curNumCardsBytes[i-4] = bytes[i];
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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is NextPlayerTurn))
            {
                return false;
            }

            NextPlayerTurn npt = (NextPlayerTurn)obj;

            return whosTurn.Equals(npt.whosTurn) && curNumCards.Equals(npt.curNumCards);
        }
    }

    public class Winner : MessageType
    {
        public string winner;
        public Winner()
        {

        }

        public Winner(string winner)
        {
            this.winner = winner;
        }

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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Winner))
            {
                return false;
            }

            Winner w = (Winner)obj;

            return winner.Equals(w.winner);
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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ListRoomsRequest))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class RoomEntry : MessageType
    {
        public string roomID;
        public string player1;
        public string player2;

        public RoomEntry()
        {

        }

        public RoomEntry(string roomID, string player1, string player2)
        {
            this.roomID = roomID;
            this.player1 = player1;
            this.player2 = player2;
        }

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
            string delim = "\0";
            int countBytesDelim = Encoding.UTF8.GetByteCount(delim);
            byte[] delimBytes = Encoding.UTF8.GetBytes(delim);


            byte[] roomIDByte = Encoding.UTF8.GetBytes(roomID);
            byte[] player1Byte = Encoding.UTF8.GetBytes(player1);
            byte[] player2Byte = Encoding.UTF8.GetBytes(player2);
            
            int totalByteCount = roomIDByte.Length + player1Byte.Length + player2Byte.Length + (2 * countBytesDelim);

            byte[] roomEntry = new byte[totalByteCount];

            roomIDByte.CopyTo(roomEntry, 0);
            delimBytes.CopyTo(roomEntry, roomIDByte.Length);

            player1Byte.CopyTo(roomEntry, roomIDByte.Length + countBytesDelim);

            delimBytes.CopyTo(roomEntry, roomIDByte.Length + countBytesDelim + player1Byte.Length);

            player2Byte.CopyTo(roomEntry, roomIDByte.Length + player1Byte.Length + (2 * countBytesDelim));

            return roomEntry;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is RoomEntry))
            {
                return false;
            }

            RoomEntry re = (RoomEntry)obj;

            return roomID.Equals(re.roomID) && player1.Equals(re.player1) && player2.Equals(re.player2);
        }
    }

    public class Unregister : MessageType
    {
        public override void FromBytes(byte[] bytes)
        {}

        public override string MessageID()
        {
            return "Unregister";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Unregister))
            {
                return false;
            }
            else
            {
                return true;
            }
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

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ClientDisconnect))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class RoomUnavailable : MessageType
    {
        public string Reason;

        public RoomUnavailable()
        {
            Reason = "";
        }

        public RoomUnavailable(string reason)
        {
            Reason = reason;
        }

        public override bool Equals(object obj)
        {
            return obj != null && (obj is RoomUnavailable);
        }

        public override void FromBytes(byte[] bytes)
        {
            Reason = Encoding.UTF8.GetString(bytes);
        }

        public override string MessageID()
        {
            return "RoomUnavailable";
        }

        public override byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(Reason);
        }
    }

}

