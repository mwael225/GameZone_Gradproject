using UnityEngine;
using Unity.Netcode;

public class CheckersPieceSpawner : NetworkBehaviour
{
    public GameObject piecePrefab; // Assign your checker piece prefab in the Inspector
    public int boardSize = 8;
    public int rowsPerPlayer = 3;

    // Board size and starting rows for each player
    private Vector3 boardOrigin = new Vector3(0f, 0.18f, 0f); // Height above board

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the host/server spawns the pieces
        {
            // Get the board's position
            GameObject board = GameObject.Find("ChessBoard");
            if (board != null)
            {
                boardOrigin = new Vector3(
                    board.transform.position.x - 4f, // Center of board minus half width
                    0.18f, // Fixed height above board
                    board.transform.position.z - 4f  // Center of board minus half depth
                );
            }
            SpawnPieces();
        }
    }

    void SpawnPieces()
    {
        if (!IsServer) return;

        // Spawn black pieces (top)
        for (int row = 0; row < rowsPerPlayer; row++)
        {
            bool evenRow = row % 2 == 0;
            for (int col = evenRow ? 1 : 0; col < boardSize; col += 2)
            {
                Vector3 pos = boardOrigin + new Vector3(col + 0.5f, 0f, (boardSize - 1 - row) + 0.5f);
                SpawnPiece(pos, false); // Black pieces
            }
        }

        // Spawn white pieces (bottom)
        for (int row = 0; row < rowsPerPlayer; row++)
        {
            bool evenRow = row % 2 == 0;
            for (int col = evenRow ? 0 : 1; col < boardSize; col += 2)
            {
                Vector3 pos = boardOrigin + new Vector3(col + 0.5f, 0f, row + 0.5f);
                SpawnPiece(pos, true); // White pieces
            }
        }
    }

    void SpawnPiece(Vector3 position, bool isPlayerOne)
    {
        if (!IsServer) return;

        GameObject piece = Instantiate(piecePrefab, position, Quaternion.identity);
        NetworkObject networkObject = piece.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            var pieceScript = piece.GetComponent<CheckersPiece>();
            if (pieceScript != null)
            {
                pieceScript.isPlayerOne.Value = isPlayerOne;
            }
        }
    }

    private bool evenRow(int row)
    {
        return row % 2 == 0;
    }
}