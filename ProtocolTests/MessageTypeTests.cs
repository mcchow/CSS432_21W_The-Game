using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using TriviaGameProtocol;

namespace TriviaGameProtocolTests
{
    public class MessageTypeTests
    {
        private void testByteConversion<T>(T message) where T : MessageType, new()
        {
            T message2 = new T();
            message2.FromBytes(message.ToBytes());
            Assert.True(message.Equals(message2));
        }

        [Theory]
        [InlineData("")]
        [InlineData("History")]
        [InlineData("Art")]
        public void TestSingleStringMessages(string str)
        {
            testByteConversion(new ChosenCard(str));
            testByteConversion(new Register(str));
            testByteConversion(new JoinRoom(str));
            testByteConversion(new Winner(str));
            testByteConversion(new RoomUnavailable(str));
        }

        [Theory]
        [InlineData("","","","","")]
        [InlineData("Q","","","","")]
        [InlineData("Question?","Answer A.","Answer B.","Answer C.","Answer D.")]
        public void TestTriviaQuestion(string q, string a, string b, string c, string d)
        {
            testByteConversion(new TriviaQuestion(q,a,b,c,d));
        }

        [Fact]
        public void TestPlayerAnswer()
        {
            testByteConversion(new PlayerAnswer('C'));
        }

        [Theory]
        [InlineData('A', 0, 1)]
        [InlineData('D', 4, 2)]
        public void TestAnswerAndResult(char correctAnswer, int numCards, int whosTurn)
        {
            testByteConversion(new AnswerAndResult(correctAnswer, numCards, whosTurn));
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 5)]
        public void TestNextPlayerTurn(int whosTurn, int curNumCards)
        {
            testByteConversion(new NextPlayerTurn(whosTurn, curNumCards));
        }
        [Theory]
        [InlineData("", "", "")]
        [InlineData("TheRoomID", "Player1", "")]
        [InlineData("TheRoomID", "", "Player2")]
        [InlineData("TheRoomID", "Player1", "Player2")]
        public void TestRoomEntry(string roomID, string player1, string player2)
        {

        }
    }
}
