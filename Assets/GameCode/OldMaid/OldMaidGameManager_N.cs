using System.Collections.Generic;
using UnityEngine;
using GameSystem;
using Unity.Netcode;

public class OldMaidGameManager_N : GameManager_N
{
    OldMaid_N oldmaid;
    private int navigatedCardIndexTarget = 0;
    InputHandler inputHandler;
    List<KeyCode> keyCodes = new List<KeyCode>
    {
        KeyCode.Space,
        KeyCode.Return,
        KeyCode.M,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
        KeyCode.Escape,
        KeyCode.S,
        KeyCode.I,
        KeyCode.R,
        KeyCode.Q,
        KeyCode.W
    };
    int framecount = 0;

    public void Start()
    {
        Debug.Log("OldMaidGameManager_N Start called. IsServer: " + IsServer);
        if (!IsServer)
        {
            Debug.Log("Not server, exiting Start");
            return;
        }
        inputHandler = GetComponent<InputHandler>();
        if (inputHandler == null)
        {
            Debug.LogError("InputHandler component is missing on GameObject: " + gameObject.name);
            return;
        }
        Debug.Log("InputHandler found, initializing OldMaid_N");
        oldmaid = new OldMaid_N(inputHandler);
        if (oldmaid == null)
        {
            Debug.LogError("Failed to initialize OldMaid_N");
            return;
        }
        Debug.Log("OldMaid_N initialized, setting up game state");
        currentTurn = firstplayer();
        oldmaid.gamestate = "swapping";
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
        if (!IsServer || oldmaid == null)
        {
            Debug.Log("Update skipped: Not server or oldmaid is null");
            return;
        }
        if (oldmaid.gamestate == "end")
        {
            Debug.Log("Game ended");
            return;
        }

        framecount++;
        if (framecount % 120 == 0)
        {
            framecount = 0;
            Debug.Log("Game state: " + oldmaid.gamestate + ", Current player: " + oldmaid.cplayer);
            for (int i = 0; i < oldmaid.numberOfPlayers; i++)
            {
                Debug.Log($"Player {i} has {oldmaid.hands[i].Count} cards");
            }
        }

        currentTurn = oldmaid.cplayer;
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

    private void HandleInput()
    {
        Debug.Log($"Handling input for player {currentTurn}, game state: {oldmaid.gamestate}");
        if (oldmaid.gamestate == "merging")
        {
            HandleMergingInput();
        }
        else if (oldmaid.gamestate == "swapping")
        {
            HandleSwappingInput();
        }
        HandleShuffleInput();
    }

    private void HandleMergingInput()
    {
        Debug.Log("Handling merging input");
        if (inputHandler.GetKeyDown(KeyCode.Space, currentTurn) || inputHandler.GetKeyDown(KeyCode.Return, currentTurn))
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

        if (inputHandler.GetKeyDown(KeyCode.M, currentTurn))
        {
            oldmaid.AttemptMerge(oldmaid.cplayer);
        }

        if (inputHandler.GetKeyDown(KeyCode.LeftAlt, currentTurn) || inputHandler.GetKeyDown(KeyCode.RightAlt, currentTurn))
        {
            Debug.Log("Player " + oldmaid.cplayer + " ending turn");
            oldmaid.EndTurn();
            currentTurn = oldmaid.cplayer;
        }

        if (inputHandler.GetKeyDown(KeyCode.Escape, currentTurn))
        {
            ClearAllSelections();
        }
    }

    private void HandleSwappingInput()
    {
        Debug.Log("Handling swapping input");
        if (inputHandler.GetKeyDown(KeyCode.S, currentTurn))
        {
            SelectCardFromTargetPlayer();
        }

        if (inputHandler.GetKeyDown(KeyCode.I, currentTurn))
        {
            DisplayGameInfo();
        }
    }

    private void HandleShuffleInput()
    {
        Debug.Log("Handling shuffle input");
        if (inputHandler.GetKeyDown(KeyCode.R, currentTurn))
        {
            oldmaid.ShufflePlayerHand(oldmaid.cplayer);
        }
    }

    private void SelectCardFromTargetPlayer()
    {
        Debug.Log("Selecting card from target player");
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
        Debug.Log("Handling swapping navigation");
        if (oldmaid.gamestate != "swapping") return;

        int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);
        if (targetPlayer == -1)
        {
            Debug.Log("No other players have cards! Moving to merging phase.");
            oldmaid.gamestate = "merging";
            oldmaid.areLeftPlayerCardsDisplayed = false;
            return;
        }

        bool navigationChanged = false;
        if (inputHandler.GetKeyDown(KeyCode.Q, currentTurn))
        {
            navigatedCardIndexTarget = (navigatedCardIndexTarget - 1 + oldmaid.hands[targetPlayer].Count) % oldmaid.hands[targetPlayer].Count;
            navigationChanged = true;
        }
        else if (inputHandler.GetKeyDown(KeyCode.W, currentTurn))
        {
            navigatedCardIndexTarget = (navigatedCardIndexTarget + 1) % oldmaid.hands[targetPlayer].Count;
            navigationChanged = true;
        }

        if (navigationChanged)
        {
            oldmaid.navigatedCardindex = navigatedCardIndexTarget;
            oldmaid.HighlightTargetPlayerCard(targetPlayer, navigatedCardIndexTarget);
            Debug.Log($"Navigated to card index {navigatedCardIndexTarget} for player {targetPlayer}");
        }
    }

    private void ClearAllSelections()
    {
        Debug.Log("Clearing all selections");
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

    public override int NextTurn(int noOfPlayers)
    {
        Debug.Log($"Next turn: {oldmaid.cplayer}");
        return oldmaid.cplayer;
    }
}