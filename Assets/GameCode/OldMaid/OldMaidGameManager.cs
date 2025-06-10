using System.Collections.Generic;
using UnityEngine;
using GameSystem;

public class OldMaidGameManager : MonoBehaviour
{
    OldMaid oldmaid;
    private int navigatedCardIndexLeft = 0; // Separate index for left player's hand navigation

    public void Start()
    {
        oldmaid = new OldMaid();
        Debug.Log("Old Maid game started. Player " + oldmaid.cplayer + "'s turn. Navigate Player " + oldmaid.GetLeftPlayer(oldmaid.cplayer) + "'s cards with Q/W, select with S.");
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
        }

        // Optional: Use Escape key to clear all selections
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearAllSelections();
        }
    }

    private void HandleSwappingInput()
    {
        // Use S key to swap card from left player
        if (Input.GetKeyDown(KeyCode.S))
        {
            SelectCardFromLeftPlayer();
        }

        // Optional: Display game info
        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayGameInfo();
        }
    }

    private void SelectCardFromLeftPlayer()
    {
        int leftPlayer = oldmaid.GetLeftPlayer(oldmaid.cplayer);

        if (oldmaid.hands[leftPlayer].Count == 0)
        {
            Debug.Log("Left player has no cards! Moving to merging phase.");
            oldmaid.gamestate = "merging";
            return;
        }

        if (navigatedCardIndexLeft >= oldmaid.hands[leftPlayer].Count)
        {
            navigatedCardIndexLeft = oldmaid.hands[leftPlayer].Count - 1;
        }

        oldmaid.navigatedCardindex = navigatedCardIndexLeft;
        oldmaid.SwapCardFromLeft(oldmaid.cplayer, navigatedCardIndexLeft);
    }

    private void HandleSwappingNavigation()
    {
        int leftPlayer = oldmaid.GetLeftPlayer(oldmaid.cplayer);

        if (oldmaid.hands[leftPlayer].Count == 0)
        {
            Debug.Log("Left player has no cards! Moving to merging phase.");
            oldmaid.gamestate = "merging";
            return;
        }

        // Reset visual states
        for (int i = 0; i < oldmaid.hands[leftPlayer].Count; i++)
        {
            oldmaid.hands[leftPlayer][i].transform.localScale = oldmaid.oldscale;
            oldmaid.hands[leftPlayer][i].GetComponent<Renderer>().material.color = Color.white;
        }

        // Handle navigation with Q/W
        if (Input.GetKeyDown(KeyCode.Q))
        {
            navigatedCardIndexLeft = (navigatedCardIndexLeft - 1 + oldmaid.hands[leftPlayer].Count) % oldmaid.hands[leftPlayer].Count;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            navigatedCardIndexLeft = (navigatedCardIndexLeft + 1) % oldmaid.hands[leftPlayer].Count;
        }

        // Update navigatedCardindex in OldMaid to reflect the left player's hand
        oldmaid.navigatedCardindex = navigatedCardIndexLeft;

        // Highlight the navigated card
        if (navigatedCardIndexLeft < oldmaid.hands[leftPlayer].Count)
        {
            GameObject navigatedCard = oldmaid.hands[leftPlayer][navigatedCardIndexLeft];
            navigatedCard.transform.localScale = oldmaid.oldscale * 1.2f;
            navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
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
            int leftPlayer = oldmaid.GetLeftPlayer(oldmaid.cplayer);
            Debug.Log("Swapping card from Player " + leftPlayer + " (left player)");
            Debug.Log("Left player has " + oldmaid.hands[leftPlayer].Count + " cards");
        }

        for (int i = 0; i < oldmaid.numberOfPlayers; i++)
        {
            Debug.Log("Player " + i + ": " + oldmaid.hands[i].Count + " cards");
        }

        Debug.Log("================");
    }
}