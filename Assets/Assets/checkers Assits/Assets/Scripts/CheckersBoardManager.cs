using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using System.Collections;

public class CheckersBoardManager : NetworkBehaviour
{
    public int boardSize = 8;
    public float squareSize = 1.0f;
    private CheckersPiece selectedPiece = null;
    private Vector3 boardOrigin;
    public NetworkVariable<bool> isPlayerOne = new NetworkVariable<bool>();
    public bool isKing = false;
    private CheckersPiece[,] boardState;
    public GameObject piecePrefab;
    private int rowsPerPlayer = 3;

    // Turn-based system variables
    private NetworkVariable<bool> isPlayerOneTurn = new NetworkVariable<bool>(true);
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI winText;

    private NetworkVariable<int> playerOnePieces = new NetworkVariable<int>(12);
    private NetworkVariable<int> playerTwoPieces = new NetworkVariable<int>(12);
    private NetworkVariable<bool> gameEnded = new NetworkVariable<bool>(false);

    private bool isInMultipleJump = false;
    private CheckersPiece multiJumpPiece = null;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Find UI elements in the scene by name
        if (turnText == null)
        {
            GameObject turnTextObject = GameObject.Find("turn");
            if (turnTextObject != null)
            {
                turnText = turnTextObject.GetComponent<TextMeshProUGUI>();
            }
        }
        if (winText == null)
        {
            // Assuming the win text object is named "WinText" for clarity. 
            // Please rename your "Text (TMP)" object to "WinText" in the hierarchy.
            GameObject winTextObject = GameObject.Find("WinText");
            if (winTextObject != null)
            {
                winText = winTextObject.GetComponent<TextMeshProUGUI>();
            }
        }

        if (IsServer)
        {
            SpawnPieces();
        }

        // Subscribe to network variable changes
        isPlayerOneTurn.OnValueChanged += OnTurnChanged;
        gameEnded.OnValueChanged += OnGameEndedChanged;
        
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }
        
        UpdateTurnText();
    }

    private void OnTurnChanged(bool previousValue, bool newValue)
    {
        UpdateTurnText();
    }

    private void OnGameEndedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            string winner = playerOnePieces.Value == 0 ? "Black" : "White";
            UpdateWinText(winner);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMovePieceServerRpc(ulong clientId, Vector3 piecePosition, Vector3 targetPosition)
    {
        if (!IsServer) return;

        // Convert world positions to board coordinates
        Vector3 relativePiecePos = piecePosition - boardOrigin;
        Vector3 relativeTargetPos = targetPosition - boardOrigin;
        
        int fromX = Mathf.FloorToInt(relativePiecePos.x);
        int fromZ = Mathf.FloorToInt(relativePiecePos.z);
        int toX = Mathf.FloorToInt(relativeTargetPos.x);
        int toZ = Mathf.FloorToInt(relativeTargetPos.z);

        CheckersPiece piece = boardState[fromX, fromZ];
        
        // Validate the move
        if (piece != null && IsValidMove(piece, fromX, fromZ, toX, toZ))
        {
            bool wasCapture = Mathf.Abs(toX - fromX) == 2;

            MovePiece(piece, fromX, fromZ, toX, toZ);
            
            // After a capture, check if the same piece can make another capture.
            if (wasCapture && HasCaptureMove(piece, toX, toZ))
            {
                isInMultipleJump = true;
                multiJumpPiece = piece;
                // Do NOT switch turns, let the current player make another move.
            }
            else
            {
                // If it wasn't a capture or no more captures are available, end the turn.
                isInMultipleJump = false;
                multiJumpPiece = null;
                isPlayerOneTurn.Value = !isPlayerOneTurn.Value;
            }
        }
    }

    private void MovePiece(CheckersPiece piece, int fromX, int fromZ, int toX, int toZ)
    {
        if (!IsServer) return;

        // Update board state
        boardState[fromX, fromZ] = null;
        boardState[toX, toZ] = piece;
        
        // Move the piece instantly
        Vector3 newPos = boardOrigin + new Vector3(toX + 0.5f, 0f, toZ + 0.5f);
        MovePieceClientRpc(piece.NetworkObjectId, new Vector3(newPos.x, 0.18f, newPos.z));

        // Handle captures
        int dx = toX - fromX;
        int dz = toZ - fromZ;
        if (Mathf.Abs(dx) == 2) // Capture move
        {
            int capturedX = fromX + dx / 2;
            int capturedZ = fromZ + dz / 2;
            CheckersPiece capturedPiece = boardState[capturedX, capturedZ];
            if (capturedPiece != null)
            {
                // Update piece count
                if (capturedPiece.isPlayerOne.Value)
                {
                    playerOnePieces.Value--;
                }
                else
                {
                    playerTwoPieces.Value--;
                }

                // Remove the captured piece
                boardState[capturedX, capturedZ] = null;
                RemovePieceClientRpc(capturedPiece.NetworkObjectId);

                // Play capture sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySoundClientRpc(SoundType.Capture);
                }

                // Check for win condition after capture
                CheckWinCondition();
            }
        }
        else // It was a regular move
        {
            // Play move sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundClientRpc(SoundType.Move);
            }
        }

        // Check for king promotion
        if (!piece.isKing.Value)
        {
            // Player One (White) kings at z=0. Player Two (Black) kings at z=7.
            if ((piece.isPlayerOne.Value && toZ == 0) || (!piece.isPlayerOne.Value && toZ == boardSize - 1))
            {
                piece.MakeKing();
                
                // Play promotion sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySoundClientRpc(SoundType.KingPromotion);
                }
            }
        }
    }

    [ClientRpc]
    private void MovePieceClientRpc(ulong pieceId, Vector3 newPosition)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[pieceId];
        if (networkObject != null)
        {
            networkObject.transform.position = newPosition;
        }
    }

    [ClientRpc]
    private void RemovePieceClientRpc(ulong pieceId)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[pieceId];
        if (networkObject != null)
        {
            CheckersPiece piece = networkObject.GetComponent<CheckersPiece>();
            if (piece != null)
            {
                StartCoroutine(RemovePieceWithAnimation(piece));
            }
        }
    }

    private void UpdateWinText(string winner)
    {
        if (winText != null)
        {
            winText.text = winner + " Wins!";
            winText.color = winner == "White" ? Color.white : Color.black;
            winText.gameObject.SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRestartGameServerRpc()
    {
        if (!IsServer) return;

        RestartGame();
    }

    private void RestartGame()
    {
        if (!IsServer) return;

        // Clear the board
        foreach (var piece in FindObjectsByType<CheckersPiece>(FindObjectsSortMode.None))
        {
            if (piece.NetworkObject != null)
            {
                piece.NetworkObject.Despawn();
            }
        }

        // Reset variables
        playerOnePieces.Value = 12;
        playerTwoPieces.Value = 12;
        gameEnded.Value = false;
        isPlayerOneTurn.Value = true;

        // Spawn new pieces
        SpawnPieces();
    }

    void UpdateTurnText()
    {
        if (turnText != null)
        {
            turnText.text = isPlayerOneTurn.Value ? "White's Turn" : "Black's Turn";
            turnText.color = isPlayerOneTurn.Value ? Color.white : Color.black;
        }
    }

    void Update()
    {
        if (gameEnded.Value)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RequestRestartGameServerRpc();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // First, try to get a piece directly from what we hit
                CheckersPiece piece = hit.collider.GetComponent<CheckersPiece>();

                // If we hit a piece
                if (piece != null)
                {
                    if (isInMultipleJump && piece != multiJumpPiece)
                    {
                        Debug.Log("You must complete the multi-jump with the correct piece!");
                        return;
                    }

                    // Check if the current player is allowed to select this piece
                    bool canSelectWhite = isPlayerOneTurn.Value && IsHost && piece.isPlayerOne.Value;
                    bool canSelectBlack = !isPlayerOneTurn.Value && !IsHost && !piece.isPlayerOne.Value;

                    if (canSelectWhite || canSelectBlack)
                    {
                        if (!isInMultipleJump || piece == multiJumpPiece)
                        {
                            // If the piece is already selected, deselect it
                            if (piece == selectedPiece)
                            {
                                selectedPiece.Deselect();
                                selectedPiece = null;
                            }
                            // Otherwise, select the new piece
                            else
                            {
                                if (selectedPiece != null)
                                {
                                    selectedPiece.Deselect();
                                }
                                selectedPiece = piece;
                                selectedPiece.Select();
                            }
                        }
                    }
                }
                // If we hit something else (like an empty board square)
                else
                {
                    if (selectedPiece != null)
                    {
                        // Calculate target position and request move
                        Vector3 targetPosition = hit.point;
                        RequestMovePieceServerRpc(NetworkManager.Singleton.LocalClientId, selectedPiece.transform.position, targetPosition);
                        selectedPiece.Deselect();
                        selectedPiece = null;
                    }
                }
            }
        }
    }

    private void EndTurn()
    {
        if (selectedPiece != null)
        {
            selectedPiece.Deselect();
            selectedPiece = null;
        }
        isInMultipleJump = false;
        multiJumpPiece = null;
        isPlayerOneTurn.Value = !isPlayerOneTurn.Value;
        UpdateTurnText();
    }

    private IEnumerator RemovePieceWithAnimation(CheckersPiece piece)
    {
        // Add any animations here, e.g., piece fading out
        yield return new WaitForSeconds(0.5f); // Example delay

        if (NetworkManager.Singleton.IsServer)
        {
            piece.NetworkObject.Despawn();
        }
    }

    private bool HasCaptureMove(CheckersPiece piece, int x, int z)
    {
        // Check all 4 diagonal directions for a valid capture
        int[] dx = { 2, 2, -2, -2 };
        int[] dz = { 2, -2, 2, -2 };

        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newZ = z + dz[i];

            if (IsValidCaptureMove(piece, x, z, newX, newZ))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsValidCaptureMove(CheckersPiece piece, int fromX, int fromZ, int toX, int toZ)
    {
        // Check if the target square is on the board
        if (toX < 0 || toX >= boardSize || toZ < 0 || toZ >= boardSize)
        {
            return false;
        }

        // Check if the target square is empty
        if (boardState[toX, toZ] != null)
        {
            return false;
        }

        // Non-king pieces must capture forward
        if (!piece.isKing.Value)
        {
            int dz = toZ - fromZ;
            // Player One (White) is at the top, moves DOWN (z decreases)
            if (piece.isPlayerOne.Value && dz > 0)
            {
                Debug.Log("Invalid capture: White pieces can only capture forward (down).");
                return false;
            }
            // Player Two (Black) is at the bottom, moves UP (z increases)
            else if (!piece.isPlayerOne.Value && dz < 0)
            {
                Debug.Log("Invalid capture: Black pieces can only capture forward (up).");
                return false;
            }
        }

        // Check for an enemy piece to jump over
        int capturedX = fromX + (toX - fromX) / 2;
        int capturedZ = fromZ + (toZ - fromZ) / 2;
        CheckersPiece capturedPiece = boardState[capturedX, capturedZ];

        if (capturedPiece == null || capturedPiece.isPlayerOne.Value == piece.isPlayerOne.Value)
        {
            Debug.Log("Invalid capture: Must jump over an opponent's piece.");
            return false;
        }

        return true;
    }

    private bool IsValidMove(CheckersPiece piece, int fromX, int fromZ, int toX, int toZ)
    {
        if (toX < 0 || toX >= boardSize || toZ < 0 || toZ >= boardSize)
        {
            Debug.Log("Invalid move: outside board boundaries.");
            return false;
        }

        if (boardState[toX, toZ] != null)
        {
            Debug.Log("Invalid move: target square is occupied.");
            return false;
        }

        int dx = toX - fromX;
        int dz = toZ - fromZ;

        // --- Standard Move Validation (Distance of 1) ---
        if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 1)
        {
            if (isInMultipleJump)
            {
                Debug.Log("Invalid move: Must complete all available jumps.");
                return false;
            }

            // Non-king pieces must move forward
            if (!piece.isKing.Value)
            {
                // Player One (White) is at the top, moves DOWN (z decreases)
                if (piece.isPlayerOne.Value && dz > 0)
                {
                    Debug.Log("Invalid move: White pieces can only move forward (down).");
                    return false;
                }
                // Player Two (Black) is at the bottom, moves UP (z increases)
                else if (!piece.isPlayerOne.Value && dz < 0)
                {
                    Debug.Log("Invalid move: Black pieces can only move forward (up).");
                    return false;
                }
            }
            return true;
        }
        // --- Capture Move Validation (Distance of 2) ---
        else if (Mathf.Abs(dx) == 2 && Mathf.Abs(dz) == 2)
        {
            return IsValidCaptureMove(piece, fromX, fromZ, toX, toZ);
        }

        Debug.Log("Invalid move: Must be a diagonal move of 1 or 2 squares.");
        return false;
    }

    private void SpawnPieces()
    {
        if (piecePrefab == null)
        {
            Debug.LogError("CheckersBoardManager ERROR: The 'Piece Prefab' is not assigned on the ChessBoard Prefab's Inspector!");
            return;
        }

        boardState = new CheckersPiece[boardSize, boardSize];
        boardOrigin = transform.position - new Vector3(boardSize * squareSize / 2, 0, boardSize * squareSize / 2);

        for (int z = 0; z < boardSize; z++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                // Only spawn on dark squares
                if ((x + z) % 2 == 1)
                {
                    if (z < rowsPerPlayer)
                    {
                        SpawnPiece(x, z, false); // Player Two
                    }
                    else if (z >= boardSize - rowsPerPlayer)
                    {
                        SpawnPiece(x, z, true); // Player One
                    }
                }
            }
        }
    }

    private void SpawnPiece(int x, int z, bool isPlayerOne)
    {
        if (!IsServer) return;

        Vector3 position = boardOrigin + new Vector3(x + 0.5f, 0.18f, z + 0.5f);
        GameObject pieceObj = Instantiate(piecePrefab, position, Quaternion.identity);
        NetworkObject networkObject = pieceObj.GetComponent<NetworkObject>();
        
        if (networkObject != null)
        {
            networkObject.Spawn();
            CheckersPiece piece = pieceObj.GetComponent<CheckersPiece>();
            if (piece != null)
            {
                piece.isPlayerOne.Value = isPlayerOne;
                boardState[x, z] = piece;
            }
        }
    }

    private void CheckWinCondition()
    {
        if (!IsServer) return;

        if (playerOnePieces.Value == 0 || playerTwoPieces.Value == 0)
        {
            gameEnded.Value = true;
            string winner = playerOnePieces.Value == 0 ? "Black" : "White";
            if (winText != null)
            {
                winText.text = winner + " Wins!";
                winText.color = winner == "White" ? Color.white : Color.black;
                winText.gameObject.SetActive(true);
            }

            // Play win sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundClientRpc(SoundType.Win);
            }
        }
    }
}