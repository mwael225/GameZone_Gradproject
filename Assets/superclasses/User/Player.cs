using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GameSystem;

namespace GameSystem
{
    public class Player : User
    {
        private int mmr;
        private int gamesPlayed;
        private int wins;
        private List<string> friends;
        private List<string> friendRequests;
        private Player_DB_Manager playerDbManager;
        private BanStatus banStatus;
        private Game currentGame;
        

        public enum BanStatus
        {
            PermanentlyBanned,
            TemporarilyBanned,
            NotBanned
        }

        public Player(string username, string email) : base(username, email)
        {
            mmr = 1000;
            gamesPlayed = 0;
            wins = 0;
            banStatus = BanStatus.NotBanned;
            friends = new List<string>();
            friendRequests = new List<string>();
            playerDbManager = Player_DB_Manager.Instance;
        }

        public int GetMMR() => mmr;

        public void SetMMR(int newMMR)
        {
            mmr = newMMR;
            _ = SaveToDatabase();
        }

        public int GetGamesPlayed() => gamesPlayed;

        public void IncrementGamesPlayed()
        {
            gamesPlayed++;
            _ = SaveToDatabase();
        }

        public int GetWins() => wins;

        public void IncrementWins()
        {
            wins++;
            _ = SaveToDatabase();
        }

        public float GetWinRate() => gamesPlayed > 0 ? (float)wins / gamesPlayed : 0f;

        public string GetBanStatus() => banStatus.ToString();

        public void SendFriendReq(string username)
        {
            if (!friends.Contains(username) && !friendRequests.Contains(username))
            {
                friendRequests.Add(username);
                _ = playerDbManager.UpdatePlayerData(this.username, new PlayerData {
                    username = this.username,
                    email = this.email,
                    status = this.banStatus.ToString(),
                    mmr = this.mmr,
                    gamesPlayed = this.gamesPlayed,
                    wins = this.wins,
                    friends = this.friends,
                    friendRequests = this.friendRequests
                });
            }
        }

        public void AcceptFriendReq(string username)
        {
            if (friendRequests.Contains(username))
            {
                friendRequests.Remove(username);
                friends.Add(username);
                _ = playerDbManager.UpdatePlayerData(this.username, new PlayerData {
                    username = this.username,
                    email = this.email,
                    status = this.banStatus.ToString(),
                    mmr = this.mmr,
                    gamesPlayed = this.gamesPlayed,
                    wins = this.wins,
                    friends = this.friends,
                    friendRequests = this.friendRequests
                });
            }
        }

        public void RejectFriendReq(string username)
        {
            if (friendRequests.Contains(username))
            {
                friendRequests.Remove(username);
                _ = playerDbManager.UpdatePlayerData(this.username, new PlayerData {
                    username = this.username,
                    email = this.email,
                    status = this.banStatus.ToString(),
                    mmr = this.mmr,
                    gamesPlayed = this.gamesPlayed,
                    wins = this.wins,
                    friends = this.friends,
                    friendRequests = this.friendRequests
                });
            }
        }

        public List<string> GetFriends() => friends;
        public List<string> GetFriendRequests() => friendRequests;
        public Game GetCurrentGame() => currentGame;
        public void SetCurrentGame(Game game) => currentGame = game;

        public override async Task<bool> LoadFromDatabase()
        {
            bool success = await base.LoadFromDatabase();
            if (success)
            {
                var playerData = await playerDbManager.GetPlayerData(username);
                if (playerData != null)
                {
                    mmr = playerData.mmr;
                    gamesPlayed = playerData.gamesPlayed;
                    wins = playerData.wins;
                    banStatus = (BanStatus)Enum.Parse(typeof(BanStatus), playerData.status);
                    friends = playerData.friends;
                    friendRequests = playerData.friendRequests;
                }
            }
            return success;
        }

        public override async Task<bool> SaveToDatabase()
        {
            bool success = await base.SaveToDatabase();
            if (success)
            {
                var playerData = new PlayerData
                {
                    username = username,
                    email = email,
                    status = banStatus.ToString(),
                    mmr = mmr,
                    gamesPlayed = gamesPlayed,
                    wins = wins,
                    friends = friends,
                    friendRequests = friendRequests
                };
                await playerDbManager.UpdatePlayerData(username, playerData);
            }
            return success;
        }
    }
} 