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
        public HashSet<string> CollectedCards;
        

        public HashSet<string> UncollectedCards
        {
            get
            {
                string[] allCards = { "History", "Art", "Science", "Geography", "Sports", "Entertainment" };
                HashSet<string> cards = new HashSet<string>();
                foreach (string i in allCards)
                {
                    if (!CollectedCards.Contains(i))
                    {
                        cards.Add(i);
                    }
                }
                return cards;
            }
        }
        public Player()
        {
            Room = null;
            Name = "";
            Points = 0;
            Connection = null;
            CollectedCards = new HashSet<string>();
        }
        public Player(Register registration, Connection c)
        {
            Room = null;
            Name = registration.Name;
            Connection = c;
            CollectedCards = new HashSet<string>();
        }

        public void resetCards()
        {
            CollectedCards.Clear();
        }
    }
}
