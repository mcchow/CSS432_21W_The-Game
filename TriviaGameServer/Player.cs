using System;
using System.Collections.Generic;
using System.Text;
using TriviaGameProtocol;

namespace TriviaGameServer
{
    public class Player
    {
        public Room Room;
        public string Name;
        public int Points;
        public Connection Connection;
        public Player()
        {
            Room = null;
            Name = "";
            Points = 0;
            Connection = null;
        }
        public Player(Register registration, Connection c)
        {
            Room = null;
            Name = registration.Name;
            Connection = c;
        }
    }
}
