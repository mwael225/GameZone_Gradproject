using System.Collections.Generic;

namespace GameSystem
{
    public class PlayerData
    {
        public string username;
        public string email;
        public string status;
        public int mmr;
        public int gamesPlayed;
        public int wins;
        public List<string> friends;
        public List<string> friendRequests;

        public PlayerData()
        {
            mmr = 1000;
            gamesPlayed = 0;
            wins = 0;
            friends = new List<string>();
            friendRequests = new List<string>();
        }
    }
}