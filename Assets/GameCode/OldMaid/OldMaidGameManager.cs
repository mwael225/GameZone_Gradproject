using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

public class OldMaidGameManager : MonoBehaviour
{
    OldMaid oldmaid;
    private int navigatedCardIndexTarget = 0; // Index for target player's hand navigation
    [SerializeField] private Camera player0Camera; // Reference to Player 0's camera

    public void Start()
    {
        oldmaid = new OldMaid();

        // Explicitly find and assign Player0Camera
        GameObject camObj = GameObject.Find("Player0Camera");
        if (camObj != null)
        {
            player0Camera = camObj.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("Could not find Player0Camera in the scene! Please ensure it exists.");
            return;
        }

        // Disable any other cameras to prevent conflicts
        for (int i = 1; i < 4; i++)
        {
            GameObject otherCamObj = GameObject.Find("Player" + i + "Camera");
            if (otherCamObj != null)
            {
                Camera otherCam = otherCamObj.GetComponent<Camera>();
                if (otherCam != null)
                {
                    otherCam.enabled = false;
                }
            }
        }

        // Enable Player0Camera and confirm
        player0Camera.enabled = true;
        Debug.Log("Using camera: " + player0Camera.gameObject.name + " for Player 0");

        Debug.Log("Old Maid game started. Player 0's turn. Navigate target player's cards with Q/W, select with S.");
        StartPlayerTurn(0);
    }

    private void StartPlayerTurn(int player)
    {
        if (oldmaid.gamestate == "end") return;

        Debug.Log("=== Player " + player + "'s turn ===");
        if (player == 0)
        {
            int targetPlayer = oldmaid.GetNextPlayerWithCards(player);
            if (targetPlayer != -1)
            {
                Debug.Log("Navigate Player " + targetPlayer + "'s cards with Q/W, select with S");
            }
            else
            {
                Debug.Log("No other players have cards! Moving to merging phase.");
                oldmaid.gamestate = "merging";
            }
        }
        else
        {
            // AI player's turn
            StartCoroutine(AIPlayTurn(player));
        }
    }

    private IEnumerator AIPlayTurn(int player)
    {
        // Simulate thinking time
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        // Swapping phase
        if (oldmaid.gamestate == "swapping")
        {
            int targetPlayer = oldmaid.GetNextPlayerWithCards(player);
            if (targetPlayer != -1)
            {
                int cardIndex = oldmaid.AISelectRandomCard(targetPlayer);
                if (cardIndex != -1)
                {
                    Debug.Log("AI Player " + player + " is swapping a card from Player " + targetPlayer);
                    oldmaid.HighlightTargetPlayerCard(targetPlayer, cardIndex);
                    yield return new WaitForSeconds(0.5f); // Brief highlight
                    oldmaid.SelectCardFromTargetPlayer(player, cardIndex);
                }
                else
                {
                    Debug.Log("AI Player " + player + ": No cards to swap from target. Moving to merging.");
                    oldmaid.gamestate = "merging";
                }
            }
            else
            {
                Debug.Log("AI Player " + player + ": No other players have cards. Moving to merging.");
                oldmaid.gamestate = "merging";
            }
        }

        // Merging phase
        if (oldmaid.gamestate == "merging")
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f)); // Simulate thinking
            int maxMerges = oldmaid.hands[player].Count / 2; // Limit merges to avoid infinite loops
            int mergeCount = 0;

            while (mergeCount < maxMerges)
            {
                List<List<int>> mergeablePairs = oldmaid.AISelectCardsToMerge(player);
                if (mergeablePairs.Count > 0)
                {
                    var pair = mergeablePairs[0]; // Take the first pair
                    Debug.Log("AI Player " + player + " is merging cards at indices: " + pair[0] + ", " + pair[1]);
                    oldmaid.selectedCardsindex = new List<int>(pair);
                    foreach (int index in pair)
                    {
                        if (index < oldmaid.hands[player].Count) // Safety check
                        {
                            oldmaid.hands[player][index].GetComponent<Renderer>().material.color = Color.yellow;
                        }
                    }
                    yield return new WaitForSeconds(0.5f); // Show selection
                    oldmaid.AttemptMerge(player);
                    yield return new WaitForSeconds(0.5f); // Brief pause between merges
                    mergeCount++;
                }
                else
                {
                    Debug.Log("AI Player " + player + ": No more cards to merge.");
                    break;
                }
            }
        }

        // End AI turn
        yield return new WaitForSeconds(0.5f);
        Debug.Log("AI Player " + player + " ending turn");
        oldmaid.EndTurn();
        StartPlayerTurn(oldmaid.cplayer);
    }

    private void Update()
    {
        if (oldmaid.gamestate != "end")
        {
            if (oldmaid.cplayer == 0)
            {
                // Human player input
                if (oldmaid.gamestate == "swapping")
                {
                    HandleSwappingNavigation();
                    HandleSwappingInput();
                }
                else if (oldmaid.gamestate == "merging" && oldmaid.hands[0].Count > 0)
                {
                    StartCoroutine(oldmaid.navigatedCards(0));
                    HandleMergingInput();
                }
                HandleShuffleInput();
            }
            // AI players are handled via coroutines
        }
    }

    private void HandleMergingInput()
    {
        // Use Space key or Enter key to select/deselect cards for merging
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (oldmaid.hands[0].Count > 0 && oldmaid.navigatedCardindex < oldmaid.hands[0].Count)
            {
                oldmaid.SelectCard(0, oldmaid.navigatedCardindex);
            }
            else
            {
                Debug.Log("No cards to select or invalid navigation index!");
            }
        }

        // Use M key to attempt merging selected cards
        if (Input.GetKeyDown(KeyCode.M))
        {
            oldmaid.AttemptMerge(0);
        }

        // Use Alt key to end turn
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            Debug.Log("Player 0 ending turn");
            oldmaid.EndTurn();
            StartPlayerTurn(oldmaid.cplayer);
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
            oldmaid.ShufflePlayerHand(0);
        }
    }

    private void SelectCardFromTargetPlayer()
    {
        int targetPlayer = oldmaid.GetNextPlayerWithCards(0);

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
        oldmaid.SelectCardFromTargetPlayer(0, navigatedCardIndexTarget);
    }

    private void HandleSwappingNavigation()
    {
        if (oldmaid.gamestate != "swapping") return; // Skip if not in swapping phase

        int targetPlayer = oldmaid.GetNextPlayerWithCards(0);

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
            oldmaid.HighlightTargetPlayerCard(targetPlayer, navigatedCardIndexTarget);
        }
    }

    private void ClearAllSelections()
    {
        foreach (int index in oldmaid.selectedCardsindex)
        {
            if (index < oldmaid.hands[0].Count)
            {
                oldmaid.hands[0][index].GetComponent<Renderer>().material.color = Color.white;
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
        if (oldmaid.hands[0].Count > 0)
        {
            Debug.Log("Navigated Card Index: " + oldmaid.navigatedCardindex);
        }

        if (oldmaid.gamestate == "merging")
        {
            Debug.Log("Selected Cards: " + string.Join(", ", oldmaid.selectedCardsindex));
            Debug.Log("Player 0 has " + oldmaid.hands[0].Count + " cards");
        }
        else if (oldmaid.gamestate == "swapping")
        {
            int targetPlayer = oldmaid.GetNextPlayerWithCards(0);
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

        Debug.Log("Controls for Player 0:");
        Debug.Log("- Q/W: Navigate cards");
        Debug.Log("- Space: Select/deselect cards (merging phase)");
        Debug.Log("- M: Merge selected cards");
        Debug.Log("- S: Swap card from target player (swapping phase)");
        Debug.Log("- R: Shuffle your hand");
        Debug.Log("- Alt: End turn");
        Debug.Log("- I: Display game info");
        Debug.Log("================");
    }
}