using System;
using Xunit;
using TriviaGameServer;
using TriviaGameProtocol;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace TriviaGameServerTests
{
    
    public class ServerIntegrationTests : IDisposable
    {

        private readonly static object portLock = new object();
        static int port = 8080;

        ClientServerSeq seq;
        public ServerIntegrationTests()
        {
            lock (portLock)
            {
                seq = new ClientServerSeq(port++);
            }
        }

        public void Dispose()
        {
            seq.Dispose();
        }

        private List<RoomEntry> getRooms()
        {
            Boolean gettingRooms = new bool();
            gettingRooms = true;
            List<RoomEntry> rooms = new List<RoomEntry>();
            seq.clients[0].protocol.RegisterMessageHandler<RoomEntry>((RoomEntry roomEntry, Connection c) =>
            {
                if (gettingRooms)
                {
                    rooms.Add(roomEntry);
                }
            });
            ChosenCard choseCard = new ChosenCard();
            choseCard.Card = "Category"; //TODO use correct card category here!
            seq.send(new ListRoomsRequest())
                .expect<RoomEntry>()
                .clearRecieved()
                .test();
            gettingRooms = false; // Prevent further modification of returned room list by future RoomEntry events.
            seq.clearErrorPrefix();
            return rooms;
        }

        [Theory]
        [InlineData("History")]
        [InlineData("Art")]
        [InlineData("Science")]
        [InlineData("Geography")]
        [InlineData("Sports")]
        [InlineData("Entertainment")]
        public void TestChosenCardWithValidCategory(string category)  // Currently fails because JoinRoom handler is not implemented
        {
            seq.addQuestion(new TriviaQuestion("Q?", "A", "B", "C", "D"), 'C');
            TriviaQuestion expected = seq.questions[0].question;
            seq.send(new Register("Player One"))
                .send(new CreateRoom())
                .test();
            seq.send(1, new Register("Player Two"))
                .send(1, new JoinRoom(getRooms()[0].roomID))
                .expect<AskForCard>(new AskForCard())
                .send(new ChosenCard(category))
                .expect<TriviaQuestion>(expected)
                .test();
        }

        // PlayerAnswer:
        // does nothing if player is not in a room
        // does nothing if it is not player's turn
        // responds with AnswerAndResult if winner not yet chosen
        // sends AnswerAndResult to opponent if winner not yet chosen
        // responds with Winner if player wins
        // sends Winner to opponent if player wins

        [Fact]
        public void TestPlayerAnswer_RespondsWithAnswerAndResultIfNoWinner()
        {
            //This test case assumes that player one goes first!
            AnswerAndResult answerAndResult = new AnswerAndResult();
            seq.addQuestion(new TriviaQuestion("Q?", "A", "B", "C", "D"), 'A');
            answerAndResult.correctAnswer = 'A';
            answerAndResult.whosTurn = 2; //TODO should be player two's turn, is this zero-indexed?
            answerAndResult.numCards = 0;
            seq.send(new Register("Player One"))
                .send(new CreateRoom())
                .send(1, new Register("Player Two"))
                .send(1, new JoinRoom(getRooms()[0].roomID))
                .expect<AskForCard>(new AskForCard())
                .send(new ChosenCard("Category"))
                .expect<TriviaQuestion>(seq.questions[0].question)
                .send(new PlayerAnswer('B'))
                .expect<AnswerAndResult>(answerAndResult)
                .expect<AnswerAndResult>(1, answerAndResult)
                .test();
        }

        [Fact]
        public void TestPlayerAnswer_RespondsWithWinnerIfWon()
        {
            TriviaQuestion tq = new TriviaQuestion("Q?", "A", "B", "C", "D");
            seq.addQuestionSequence(tq, "BAAABBCCCCDDAA");
            seq.send(new Register("P1"))
                .send(new CreateRoom())
                .send(1, new Register("P2"))
                .send(1, new JoinRoom(getRooms()[0].roomID))
                // Player 1 (zero-indexed id=0) goes first
                .expectRight(0, 1, "History", 'B')
                .expectRight(0, 1, "Art", 'A')
                .expectRight(0, 1, "Science", 'A')
                .expectRight(0, 1, "Geography", 'A')
                .expectRight(0, 1, "Sports", 'B')
                .expectWrong(0, 1, "Entertainment", 'A')
                // Player 2
                .expectRight(1, 0, "Geography", 'C')
                .expectRight(1, 0, "Sports", 'C')
                .expectRight(1, 0, "Entertainment", 'C')
                .expectRight(1, 0, "Science", 'C')
                .expectRight(1, 0, "Art", 'D')
                .expectWrong(1, 0, "History", 'C')
                .log("Both players should now be tied w/ 5 cards each.")
                // Player 1
                .expectWrong(0, 1, "History", 'B')
                // Player 2
                .expectWin(1, 0, "History", 'A')
                .test();
        }

        [Fact]
        public void TestPlayerAnswer_SendsAnswerAndResultToOtherPlayerIfNoWinner()
        {
            //TODO
        }

        [Fact]
        public void TestPlayerAnswer_SendsWinnerToOtherPlayerIfWon()
        {
            //TODO
        }

        [Fact]
        public void TestRegister()
        {
            // Ignores duplicate Register messages from same connection
            // Adds player to the connection map (api calls that require
            // the player to be in the connection map should work now)
            //TODO
        }

        [Fact]
        public void TestUnregister()
        {
            // Removes player from connection map (api calls that require
            // the player to be in the connection map should be ignored)
            //TODO
        }

        [Fact]
        public void TestClientDisconnect()
        {
            // Closes connection
            //TODO
        }

        [Fact]
        public void TestJoinRoom_Ignores_Unregistered()
        {
            seq.send(new Register("p1"))
                .send(new CreateRoom())
                .send(1, new JoinRoom(getRooms()[0].roomID))
                .test();
        }

        [Fact]
        public void TestJoinRoom_Ignores_Already_Joined()
        {
            seq.send(new Register("p1"))
                .send(new CreateRoom())
                .send(new JoinRoom(getRooms()[0].roomID))
                .test();
        }


        [Fact]
        public void TestJoinRoom_Sends_RoomUnavailable_If_Full()
        {
            seq.addClient()
                .send(new Register("p1"))
                .send(1, new Register("p2"))
                .send(new CreateRoom())
                .test();
            RoomEntry room = getRooms()[0];
            seq.send(1, new JoinRoom(room.roomID))
                .expect<AskForCard>()
                .expect<NextPlayerTurn>(1)
                .send(2, new Register("p3"))
                .send(2, new JoinRoom(room.roomID))
                .expect<RoomUnavailable>(2)
                .test();
        }

        [Fact]
        public void TestLeaveRoom_Ignored_If_Not_Registered()
        {
            seq.send(new LeaveRoom()).test();
        }

        [Fact]
        public void TestLeaveRoom_Ignored_If_Not_In_Room()
        {
            seq.send(new Register("p1"))
                .send(new LeaveRoom())
                .test();
        }
        
        [Fact]
        public void TestLeaveRoom_JoinRoom()
        {
            seq.addClient()
                .send(new Register("p1"))
                .send(new CreateRoom())
                .test();
            seq.send(1, new Register("p2"))
                .send(1, new CreateRoom())
                .test();
            List<RoomEntry> roomEntrys = getRooms();
            RoomEntry room0;
            RoomEntry room1;
            if (roomEntrys[0].player1.Equals("p1"))
            {
                room0 = roomEntrys[0];
                room1 = roomEntrys[1];
            } else
            {
                room0 = roomEntrys[1];
                room1 = roomEntrys[0];
            }
            seq.send(2, new Register("p3"))
                .send(2, new JoinRoom(room0.roomID))
                .expect<AskForCard>(0)
                .expect<NextPlayerTurn>(2)
                .send(2, new LeaveRoom())
                .send(2, new JoinRoom(room1.roomID))
                .expect<AskForCard>(1)
                .expect<NextPlayerTurn>(2)
                .test();
        }

        [Fact]
        public void TestLeaveRoom_Removes_Room()
        {
            //TODO
        }

        [Fact]
        public void TestListRoomsRequest()
        {
            seq.send(new Register("P1"))
                .send(new CreateRoom())
                .send(new ListRoomsRequest())
                .expect<RoomEntry>()
                .clearRecieved()
                .test();
        }
    }

}
