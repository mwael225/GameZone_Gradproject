//OldMaidGameManager.cs
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

public class OldMaidGameManager : MonoBehaviour
{
    OldMaid oldmaid;
    private int navigatedCardIndexTarget = 0; // Index for target player's hand navigation
    [SerializeField] private Camera[] playerCameras; // Array to hold references to the four player cameras

    public void Start()
    {
        oldmaid = new OldMaid();

        // Initialize cameras
        if (playerCameras == null || playerCameras.Length != 4)
        {
            // Fallback: Find cameras by name
            playerCameras = new Camera[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject camObj = GameObject.Find("Player" + i + "Camera");
                if (camObj != null)
                {
                    playerCameras[i] = camObj.GetComponent<Camera>();
                }
            }

            // Check if all cameras were found
            if (playerCameras[0] == null || playerCameras[1] == null || playerCameras[2] == null || playerCameras[3] == null)
            {
                Debug.LogError("Could not find all player cameras (Player0Camera, Player1Camera, Player2Camera, Player3Camera). Please assign them in the Inspector or ensure they exist in the scene!");
                return;
            }
        }

        SwitchToPlayerCamera(oldmaid.cplayer); // Set initial camera to Player 0
        int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);
        if (targetPlayer != -1)
        {
            Debug.Log("Old Maid game started. Player " + oldmaid.cplayer + "'s turn. Navigate Player " + targetPlayer + "'s cards with Q/W, select with S.");
        }
        else
        {
            Debug.Log("Old Maid game started. Player " + oldmaid.cplayer + "'s turn. No other players have cards - moving to merging phase.");
            oldmaid.gamestate = "merging";
        }
    }

    public void Update()
    {
        if (oldmaid.gamestate != "end")
        {
            if (oldmaid.gamestate == "swapping")
            {
                HandleSwappingNavigation();
            }
            else if (oldmaid.gamestate == "merging" && oldmaid.hands[oldmaid.cplayer].Count > 0)
            {
                StartCoroutine(oldmaid.navigatedCards(oldmaid.cplayer));
            }
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (oldmaid.gamestate == "merging")
        {
            HandleMergingInput();
        }
        else if (oldmaid.gamestate == "swapping")
        {
            HandleSwappingInput();
        }

        // Handle shuffle input - available during both phases but only for current player's turn
        HandleShuffleInput();
    }

    private void HandleMergingInput()
    {
        // Use Space key or Enter key to select/deselect cards for merging
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (oldmaid.hands[oldmaid.cplayer].Count > 0 && oldmaid.navigatedCardindex < oldmaid.hands[oldmaid.cplayer].Count)
            {
                oldmaid.SelectCard(oldmaid.cplayer, oldmaid.navigatedCardindex);
            }
            else
            {
                Debug.Log("No cards to select or invalid navigation index!");
            }
        }

        // Use M key to attempt merging selected cards
        if (Input.GetKeyDown(KeyCode.M))
        {
            oldmaid.AttemptMerge(oldmaid.cplayer);
        }

        // Use Alt key to end turn
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            Debug.Log("Player " + oldmaid.cplayer + " ending turn");
            oldmaid.EndTurn();
            SwitchToPlayerCamera(oldmaid.cplayer); // Switch to the next player's camera
        }

        // Optional: Use Escape key to clear all selections
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearAllSelections();
        }
    }

    private void HandleSwappingInput()
    {
        // Use S key to swap card from target player
        if (Input.GetKeyDown(KeyCode.S))
        {
            SelectCardFromTargetPlayer();
        }

        // Optional: Display game info
        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayGameInfo();
        }
    }

    private void HandleShuffleInput()
    {
        // Use R key to shuffle current player's hand
        if (Input.GetKeyDown(KeyCode.R))
        {
            oldmaid.ShufflePlayerHand(oldmaid.cplayer);
        }
    }

    private void SelectCardFromTargetPlayer()
    {
        int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);

        if (targetPlayer == -1)
        {
            Debug.Log("No other players have cards! Moving to merging phase.");
            oldmaid.gamestate = "merging";
            oldmaid.areLeftPlayerCardsDisplayed = false;
            return;
        }

        if (navigatedCardIndexTarget >= oldmaid.hands[targetPlayer].Count)
        {
            navigatedCardIndexTarget = oldmaid.hands[targetPlayer].Count - 1;
        }

        oldmaid.navigatedCardindex = navigatedCardIndexTarget;
        oldmaid.SelectCardFromTargetPlayer(oldmaid.cplayer);
    }

    private void HandleSwappingNavigation()
    {
        if (oldmaid.gamestate != "swapping") return; // Skip if not in swapping phase

        int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);

        if (targetPlayer == -1)
        {
            Debug.Log("No other players have cards! Moving to merging phase.");
            oldmaid.gamestate = "merging";
            oldmaid.areLeftPlayerCardsDisplayed = false;
            return;
        }

        // Handle navigation with Q/W
        bool navigationChanged = false;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            navigatedCardIndexTarget = (navigatedCardIndexTarget - 1 + oldmaid.hands[targetPlayer].Count) % oldmaid.hands[targetPlayer].Count;
            navigationChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            navigatedCardIndexTarget = (navigatedCardIndexTarget + 1) % oldmaid.hands[targetPlayer].Count;
            navigationChanged = true;
        }

        if (navigationChanged)
        {
            oldmaid.navigatedCardindex = navigatedCardIndexTarget;
            // Only highlight the navigated card, don't move any cards
            oldmaid.HighlightTargetPlayerCard(targetPlayer, navigatedCardIndexTarget);
        }
    }

    private void ClearAllSelections()
    {
        // Clear visual selection indicators
        foreach (int index in oldmaid.selectedCardsindex)
        {
            if (index < oldmaid.hands[oldmaid.cplayer].Count)
            {
                oldmaid.hands[oldmaid.cplayer][index].GetComponent<Renderer>().material.color = Color.white;
            }
        }
        oldmaid.selectedCardsindex.Clear();
        Debug.Log("All selections cleared");
    }

    private void DisplayGameInfo()
    {
        Debug.Log("=== Game Info ===");
        Debug.Log("Current Player: " + oldmaid.cplayer);
        Debug.Log("Game State: " + oldmaid.gamestate);
        if (oldmaid.hands[oldmaid.cplayer].Count > 0)
        {
            Debug.Log("Navigated Card Index: " + oldmaid.navigatedCardindex);
        }

        if (oldmaid.gamestate == "merging")
        {
            Debug.Log("Selected Cards: " + string.Join(", ", oldmaid.selectedCardsindex));
            Debug.Log("Player " + oldmaid.cplayer + " has " + oldmaid.hands[oldmaid.cplayer].Count + " cards");
        }
        else if (oldmaid.gamestate == "swapping")
        {
            int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);
            if (targetPlayer != -1)
            {
                Debug.Log("Swapping card from Player " + targetPlayer + " (target player)");
                Debug.Log("Target player has " + oldmaid.hands[targetPlayer].Count + " cards");
            }
            else
            {
                Debug.Log("No other players have cards to swap from");
            }
        }

        for (int i = 0; i < oldmaid.numberOfPlayers; i++)
        {
            Debug.Log("Player " + i + ": " + oldmaid.hands[i].Count + " cards");
        }

        Debug.Log("Controls:");
        Debug.Log("- Q/W: Navigate cards");
        Debug.Log("- Space: Select/deselect cards (merging phase)");
        Debug.Log("- M: Merge selected cards");
        Debug.Log("- S: Swap card from target player (swapping phase)");
        Debug.Log("- R: Shuffle your hand");
        Debug.Log("- Alt: End turn");
        Debug.Log("- I: Display game info");
        Debug.Log("================");
    }

    // Method to switch to the current player's camera
    private void SwitchToPlayerCamera(int playerIndex)
    {
        if (playerCameras == null || playerCameras.Length != 4)
        {
            Debug.LogError("Player cameras not properly assigned!");
            return;
        }

        // Disable all cameras
        for (int i = 0; i < playerCameras.Length; i++)
        {
            if (playerCameras[i] != null)
            {
                playerCameras[i].enabled = false;
            }
        }

        // Enable the current player's camera
        if (playerCameras[playerIndex] != null)
        {
            playerCameras[playerIndex].enabled = true;
            Debug.Log("Switched to Player " + playerIndex + "'s camera");
        }
        else
        {
            Debug.LogError("Camera for Player " + playerIndex + " is null!");
        }
    }
}