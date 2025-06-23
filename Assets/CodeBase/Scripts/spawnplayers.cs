using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class spawnplayers : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<GameObject> playerPrefabs = new List<GameObject>();
    public GameObject GameMenu;

    void Start()
    {
        
    }   

    // Update is called once per frame
    void Update()
    {

    }
    void OnEnable()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.ConnectionApprovalCallback += OnConnectionApproval;
                Debug.Log("dont need to update every time");
                
            }
            else
            {
                Debug.LogError("NetworkManager.Singleton is null in OnEnable(). Is it in the scene?");
            }
        }

    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    void OnClientConnected(ulong clientId)
    {   if(NetworkManager.Singleton.IsHost)
        {
        Debug.Log($"Client connected with ID: {clientId}");
        GameObject instance = Instantiate(playerPrefabs[int.Parse(clientId.ToString())]);
        instance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        Debug.Log("Player"+clientId+" prefab instantiated and spawned.");
        Debug.Log("Total connected clients: " + NetworkManager.Singleton.ConnectedClients.Count);
        }
        if (NetworkManager.Singleton.ConnectedClients.Count ==1&& NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Spawning game manager as two players are connected.");
            GameObject instance = Instantiate(GameMenu);
            instance.GetComponent<NetworkObject>().Spawn();
        }

    }
    void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(NetworkManager.Singleton.ConnectedClients.Count >=4)
        {
            Debug.Log("Connection approval denied. Maximum number of players reached.");
            response.Approved = false; 
            return;
        }
        else
        {
        Debug.Log("Connection approved");
        response.Approved = true;
        }    
    }     
}
