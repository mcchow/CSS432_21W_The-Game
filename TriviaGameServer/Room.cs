using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameServer
{

    public enum RoomPlayer { PlayerOne, PlayerTwo };

    public class Room
    {
        public string cardCategory;
        public string roomID;
        private Player mPlayerOne;
        private Player mPlayerTwo;
        private readonly object playerLock = new object();

        public Player playerOne {
            get
            {
                return mPlayerOne;
            }
        }

        public Player playerTwo
        {
            get
            {
                return mPlayerTwo;
            }
        }

        public bool TryJoin(Player player)
        {
            lock (playerLock)
            {
                if (mPlayerOne == null)
                {
                    mPlayerOne = player;
                    return true;
                }
                if (mPlayerTwo == null)
                {
                    mPlayerTwo = player;
                    return true;
                }
                return false;
            }
        }

        public bool TryLeave(Player player)
        {
            lock (playerLock)
            {
                if (mPlayerOne != null && mPlayerOne == player)
                {
                    mPlayerOne = null;
                    return true;
                }
                if (mPlayerTwo != null && mPlayerTwo == player)
                {
                    mPlayerTwo = null;
                    return true;
                }
                return false;
            }
        }

        private RoomPlayer currentPlayer;

        public RoomPlayer WhosTurn
        {
            get
            {
                return currentPlayer;
            }
            set
            {
                lock(playerLock)
                {
                    currentPlayer = value;
                }
            }
        }

        public char answer;
    }

}
