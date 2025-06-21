using UnityEngine;
using Unity.Netcode;

public class BoardSpawner : MonoBehaviour
{
    public GameObject chessBoardPrefab;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnBoard;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= SpawnBoard;
        }
    }

    private void SpawnBoard()
    {
        if (chessBoardPrefab == null)
        {
            Debug.LogError("BoardSpawner ERROR: The 'Chess Board Prefab' is not assigned in the Inspector!");
            return;
        }

        GameObject board = Instantiate(chessBoardPrefab, Vector3.zero, Quaternion.identity);
        NetworkObject networkObject = board.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("BoardSpawner ERROR: The instantiated ChessBoard prefab is MISSING a NetworkObject component!");
        }
    }
}