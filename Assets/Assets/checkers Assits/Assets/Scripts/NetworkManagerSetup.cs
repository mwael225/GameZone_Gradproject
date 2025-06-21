using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkManagerSetup : MonoBehaviour
{
    public GameObject checkersPiecePrefab;
    public GameObject chessBoardPrefab;

    void Awake()
    {
        var networkManager = GetComponent<Unity.Netcode.NetworkManager>();
        if (networkManager != null)
        {
            if (checkersPiecePrefab != null)
            {
                networkManager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = checkersPiecePrefab });
            }
            
            if (chessBoardPrefab != null)
            {
                networkManager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = chessBoardPrefab });
            }
        }
    }
} 