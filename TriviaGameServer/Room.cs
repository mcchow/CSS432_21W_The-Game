using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameServer
{
    public class Room
    {
        public List<Player> playerList;
        public int roomID;
        // public HashSet<int> questionIDs;  // load in all the questions from database at beginning?
        public int whosTurn;
        // public int numRounds;  // leave for now
        public char answer;
    }

}
