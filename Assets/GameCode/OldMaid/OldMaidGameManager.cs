//This is the Monobehaviour file of the OldMaid, it is used for initializing the game, handles player turns (human and AI), processes input for card navigation, selection, swapping, and merging, updates the game state display, and coordinates game flow using the OldMaid class.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

public class OldMaidGameManager : MonoBehaviour
{
    OldMaid oldmaid;
    private int navigatedCardIndexTarget = 0; 
    [SerializeField] private Camera player0Camera;
    private string gameStateText;

    public void Start()
    {
        oldmaid = new OldMaid();
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
        UpdateGameStateText();
        StartPlayerTurn(0);
    }

    private void UpdateGameStateText()
    {
        if (oldmaid.gamestate == "end")
        {
            List<int> finishOrder = oldmaid.GetFinishOrder();
            string rankingText = "Game Over!\n";
            for (int i = 0; i < finishOrder.Count; i++)
            {
                int place = i + 1;
                string suffix = place == 1 ? "st" : place == 2 ? "nd" : place == 3 ? "rd" : "th";
                rankingText += $"{place}{suffix} Place: Player {finishOrder[i] + 1}\n";
            }
            int lastPlayer = finishOrder[finishOrder.Count - 1];
            if (oldmaid.hands[lastPlayer].Count == 1 && oldmaid.HasKingCard(lastPlayer))
            {
                rankingText += $"Player {lastPlayer + 1} Lost with the Old Maid!";
            }
            gameStateText = rankingText;
        }
        else
        {
            string stateText = $"Player {oldmaid.cplayer + 1}'s Turn\n";
            stateText += oldmaid.gamestate == "swapping" ? "Swapping Phase" : "Merging Phase";
            gameStateText = stateText;
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperRight;
        float x = Screen.width - 10;
        float y = 10;
        Rect labelRect = new Rect(0, y, Screen.width - 20, 100);
        GUI.Label(labelRect, gameStateText, style);
    }

    private void StartPlayerTurn(int player)
    {
        if (oldmaid.gamestate == "end") return;

        Debug.Log("=== Player " + player + "'s turn ===");
        UpdateGameStateText(); // Update text for new turn
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
                UpdateGameStateText(); // Update text for phase change
            }
        }
        else
        {
            StartCoroutine(AIPlayTurn(player));
        }
    }

    private IEnumerator AIPlayTurn(int player)
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));
        if (Random.Range(0f, 1f) < (oldmaid.HasKingCard(player) ? 0.7f : 0.3f))
        {
            Debug.Log("AI Player " + player + " is shuffling their cards...");
            oldmaid.ShufflePlayerHand(player);
            UpdateGameStateText();
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
        if (oldmaid.gamestate == "swapping")
        {
            int targetPlayer = oldmaid.GetNextPlayerWithCards(player);
            if (targetPlayer != -1)
            {
                int hesitationSteps = Random.Range(1, 4);
                for (int i = 0; i < hesitationSteps; i++)
                {
                    int tempIndex = Random.Range(0, oldmaid.hands[targetPlayer].Count);
                    oldmaid.HighlightTargetPlayerCard(targetPlayer, tempIndex);
                    yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
                }

                int cardIndex = oldmaid.AISelectRandomCard(targetPlayer);
                if (cardIndex != -1)
                {
                    Debug.Log("AI Player " + player + " is swapping a card from Player " + targetPlayer);
                    oldmaid.HighlightTargetPlayerCard(targetPlayer, cardIndex);
                    yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
                    oldmaid.SelectCardFromTargetPlayer(player, cardIndex);
                    UpdateGameStateText();
                }
                else
                {
                    Debug.Log("AI Player " + player + ": No cards to swap from target. Moving to merging.");
                    oldmaid.gamestate = "merging";
                    UpdateGameStateText();
                }
            }
            else
            {
                Debug.Log("AI Player " + player + ": No other players have cards. Moving to merging.");
                oldmaid.gamestate = "merging";
                UpdateGameStateText();
            }
        }

        // Merging phase
        if (oldmaid.gamestate == "merging")
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            int maxMerges = oldmaid.hands[player].Count / 2;
            int mergeCount = 0;

            while (mergeCount < maxMerges)
            {
                List<List<int>> mergeablePairs = oldmaid.AISelectCardsToMerge(player);
                if (mergeablePairs.Count > 0)
                {
                    int pairIndex = Random.Range(0, mergeablePairs.Count);
                    var pair = mergeablePairs[pairIndex];

                    Debug.Log("AI Player " + player + " is merging cards at indices: " + pair[0] + ", " + pair[1]);
                    oldmaid.selectedCardsindex = new List<int>(pair);
                    foreach (int index in pair)
                    {
                        if (index < oldmaid.hands[player].Count)
                        {
                            oldmaid.hands[player][index].GetComponent<Renderer>().material.color = Color.yellow;
                            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
                        }
                    }

                    yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
                    oldmaid.AttemptMerge(player);
                    UpdateGameStateText();
                    yield return new WaitForSeconds(Random.Range(0.5f, 1.0f)); // Brief pause between merges
                    mergeCount++;
                }
                else
                {
                    Debug.Log("AI Player " + player + ": No more cards to merge.");
                    break;
                }
            }
        }

        // End AI turn with some delay
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
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
        }
    }

    private void HandleMergingInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (oldmaid.hands[0].Count > 0 && oldmaid.navigatedCardindex < oldmaid.hands[0].Count)
            {
                oldmaid.SelectCard(0, oldmaid.navigatedCardindex);
                UpdateGameStateText();
            }
            else
            {
                Debug.Log("No cards to select or invalid navigation index!");
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            oldmaid.AttemptMerge(0);
            UpdateGameStateText();
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            Debug.Log("Player 0 ending turn");
            oldmaid.EndTurn();
            StartPlayerTurn(oldmaid.cplayer);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearAllSelections();
            UpdateGameStateText();
        }
    }

    private void HandleSwappingInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SelectCardFromTargetPlayer();
            UpdateGameStateText(); 
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayGameInfo();
        }
    }

    private void HandleShuffleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            oldmaid.ShufflePlayerHand(0);
            UpdateGameStateText();
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
            UpdateGameStateText();
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
        if (oldmaid.gamestate != "swapping") return;

        int targetPlayer = oldmaid.GetNextPlayerWithCards(0);

        if (targetPlayer == -1)
        {
            Debug.Log("No other players have cards! Moving to merging phase.");
            oldmaid.gamestate = "merging";
            oldmaid.areLeftPlayerCardsDisplayed = false;
            UpdateGameStateText();
            return;
        }
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
            UpdateGameStateText();
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