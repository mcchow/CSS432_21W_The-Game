using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameServer
{
    public class Room
    {
        public List<string> playerList;
        public int roomID;
        public Set<int> questionIDs;  // load in all the questions from database at beginning?
        public int whosTurn;
        public int numRounds;  // leave for now
    }

}
