using System;
using System.Collections.Generic;
using System.Text;
using TriviaGameProtocol;

namespace TriviaGameServer
{
    class Player
    {
        public Room Room;
        public string Name;
        public Player()
        {
            Room = null;
            Name = "";
        }
        public Player(Register registration)
        {
            Room = null;
            Name = registration.Name;
        }
    }
}
