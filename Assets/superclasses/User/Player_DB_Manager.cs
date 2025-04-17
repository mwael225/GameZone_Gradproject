using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;


namespace GameSystem
{
    public class Player_DB_Manager
    {
        private static Player_DB_Manager instance;
        private DatabaseReference dbReference;
        private Dictionary<string, PlayerData> playerCache;
        private const float CACHE_DURATION = 300f; // 5 minutes

        public static Player_DB_Manager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Player_DB_Manager();
                }
                return instance;
            }
        }

        public DatabaseReference DatabaseReference => dbReference;

        private Player_DB_Manager()
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            playerCache = new Dictionary<string, PlayerData>();
        }

        public async Task<PlayerData> GetPlayerData(string username)
        {
            if (playerCache.TryGetValue(username, out PlayerData cachedData))
            {
                return cachedData;
            }

            try
            {
                var snapshot = await dbReference.Child("users").Child(username).GetValueAsync();
                if (snapshot.Exists)
                {
                    var playerData = JsonUtility.FromJson<PlayerData>(snapshot.GetRawJsonValue());
                    playerCache[username] = playerData;
                    return playerData;
                }
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting player data: {e.Message}");
                return null;
            }
        }

        public async Task UpdatePlayerData(string username, PlayerData playerData)
        {
            try
            {
                string json = JsonUtility.ToJson(playerData);
                await dbReference.Child("users").Child(username).SetRawJsonValueAsync(json);
                playerCache[username] = playerData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating player data: {e.Message}");
            }
        }

        public async Task SendFriendRequest(string sender, string receiver)
        {
            try
            {
                var receiverData = await GetPlayerData(receiver);
                if (receiverData == null)
                {
                    Debug.LogWarning($"Cannot send friend request: {receiver} not found");
                    return;
                }

                if (!receiverData.friendRequests.Contains(sender))
                {
                    receiverData.friendRequests.Add(sender);
                    await UpdatePlayerData(receiver, receiverData);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error sending friend request: {e.Message}");
            }
        }

        public async Task AcceptFriendRequest(string accepter, string requester)
        {
            try
            {
                var accepterData = await GetPlayerData(accepter);
                var requesterData = await GetPlayerData(requester);

                if (accepterData == null || requesterData == null)
                {
                    Debug.LogWarning("Cannot accept friend request: One or both players not found");
                    return;
                }

                accepterData.friendRequests.Remove(requester);
                accepterData.friends.Add(requester);
                requesterData.friends.Add(accepter);

                await UpdatePlayerData(accepter, accepterData);
                await UpdatePlayerData(requester, requesterData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error accepting friend request: {e.Message}");
            }
        }

        public async Task RejectFriendRequest(string rejecter, string requester)
        {
            try
            {
                var rejecterData = await GetPlayerData(rejecter);
                if (rejecterData == null)
                {
                    Debug.LogWarning($"Cannot reject friend request: {rejecter} not found");
                    return;
                }

                rejecterData.friendRequests.Remove(requester);
                await UpdatePlayerData(rejecter, rejecterData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rejecting friend request: {e.Message}");
            }
        }
    }
} 