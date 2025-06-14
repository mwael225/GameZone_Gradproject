using UnityEngine;
using Unity.Netcode;
using System.Collections;
using GameSystem;

public class OldMaidGameManager : NetworkBehaviour
{
    private OldMaid_N oldmaid;
    private InputHandler inputHandler;
    private Camera[] playerCameras;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        inputHandler = GetComponent<InputHandler>();
        oldmaid = new OldMaid_N(inputHandler);
        oldmaid.gamestate = "swapping";
        oldmaid.cplayer = 0;
        InitializeCameras();
        UpdateCurrentPlayerClientRpc(oldmaid.cplayer);
    }

    private void InitializeCameras()
    {
        playerCameras = new Camera[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject camObj = GameObject.Find($"Player{i}Camera");
            if (camObj != null)
            {
                playerCameras[i] = camObj.GetComponent<Camera>();
            }
        }
        if (playerCameras[0] == null || playerCameras[1] == null || playerCameras[2] == null || playerCameras[3] == null)
        {
            Debug.LogError("Could not find all player cameras!");
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (oldmaid.gamestate == "end") return;

        StartCoroutine(oldmaid.navigatedCards(oldmaid.cplayer));
        if (oldmaid.gamestate == "swapping")
        {
            HandleSwappingNavigation();
        }
        else if (oldmaid.gamestate == "merging" && oldmaid.hands[oldmaid.cplayer].Count > 0)
        {
            // Merging logic handled via input
        }
    }

    private void HandleSwappingNavigation()
    {
        int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);
        if (targetPlayer == -1)
        {
            oldmaid.gamestate = "merging";
            oldmaid.areLeftPlayerCardsDisplayed.Value = false;
            oldmaid.UpdateGameStateClientRpc(oldmaid.gamestate);
            return;
        }
        if (oldmaid.navigatedCardIndexTarget.Value >= oldmaid.hands[targetPlayer].Count)
        {
            oldmaid.navigatedCardIndexTarget.Value = oldmaid.hands[targetPlayer].Count - 1;
        }
        oldmaid.HighlightTargetPlayerCardServerRpc(targetPlayer, oldmaid.navigatedCardIndexTarget.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleInputServerRpc(KeyCode keyCode, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if ((int)clientId != oldmaid.cplayer) return;

        if (oldmaid.gamestate == "merging")
        {
            if (keyCode == KeyCode.Space || keyCode == KeyCode.Return)
            {
                oldmaid.SelectCardServerRpc(oldmaid.cplayer, oldmaid.navigatedCardindex);
            }
            else if (keyCode == KeyCode.M)
            {
                oldmaid.AttemptMergeServerRpc(oldmaid.cplayer);
            }
            else if (keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt)
            {
                oldmaid.EndTurnServerRpc();
                UpdateCurrentPlayerClientRpc(oldmaid.cplayer);
            }
            else if (keyCode == KeyCode.Escape)
            {
                oldmaid.ClearSelectionServerRpc(oldmaid.cplayer);
            }
        }
        else if (oldmaid.gamestate == "swapping")
        {
            int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);
            if (targetPlayer == -1)
            {
                oldmaid.gamestate = "merging";
                oldmaid.areLeftPlayerCardsDisplayed.Value = false;
                oldmaid.UpdateGameStateClientRpc(oldmaid.gamestate);
                return;
            }
            bool navigationChanged = false;
            if (keyCode == KeyCode.Q)
            {
                oldmaid.navigatedCardIndexTarget.Value = (oldmaid.navigatedCardIndexTarget.Value - 1 + oldmaid.hands[targetPlayer].Count) % oldmaid.hands[targetPlayer].Count;
                navigationChanged = true;
            }
            else if (keyCode == KeyCode.W)
            {
                oldmaid.navigatedCardIndexTarget.Value = (oldmaid.navigatedCardIndexTarget.Value + 1) % oldmaid.hands[targetPlayer].Count;
                navigationChanged = true;
            }
            else if (keyCode == KeyCode.S)
            {
                oldmaid.navigatedCardindex = oldmaid.navigatedCardIndexTarget.Value;
                oldmaid.SwapCardFromTargetServerRpc(oldmaid.cplayer, targetPlayer, oldmaid.navigatedCardIndexTarget.Value);
            }
            if (navigationChanged)
            {
                oldmaid.HighlightTargetPlayerCardServerRpc(targetPlayer, oldmaid.navigatedCardIndexTarget.Value);
            }
        }
        if (keyCode == KeyCode.R)
        {
            oldmaid.ShufflePlayerHandServerRpc(oldmaid.cplayer);
        }
        else if (keyCode == KeyCode.I)
        {
            DisplayGameInfoClientRpc();
        }
    }

    [ClientRpc]
    private void UpdateCurrentPlayerClientRpc(int newPlayer)
    {
        oldmaid.cplayer = newPlayer;
        SwitchToPlayerCamera(newPlayer);
    }

    [ClientRpc]
    private void DisplayGameInfoClientRpc()
    {
        Debug.Log("=== Game Info ===");
        Debug.Log($"Current Player: {oldmaid.cplayer}");
        Debug.Log($"Game State: {oldmaid.gamestate}");
        if (oldmaid.hands[oldmaid.cplayer].Count > 0)
        {
            Debug.Log($"Navigated Card Index: {oldmaid.navigatedCardindex}");
        }
        if (oldmaid.gamestate == "merging")
        {
            Debug.Log($"Selected Cards: {string.Join(", ", oldmaid.selectedCardsindex)}");
            Debug.Log($"Player {oldmaid.cplayer} has {oldmaid.hands[oldmaid.cplayer].Count} cards");
        }
        else if (oldmaid.gamestate == "swapping")
        {
            int targetPlayer = oldmaid.GetNextPlayerWithCards(oldmaid.cplayer);
            if (targetPlayer != -1)
            {
                Debug.Log($"Swapping card from Player {targetPlayer}");
                Debug.Log($"Target player has {oldmaid.hands[targetPlayer].Count} cards");
            }
        }
        for (int i = 0; i < oldmaid.numberOfPlayers; i++)
        {
            Debug.Log($"Player {i}: {oldmaid.hands[i].Count} cards");
        }
        Debug.Log("Controls:");
        Debug.Log("- Q/W: Navigate cards");
        Debug.Log("- Space/Return: Select/deselect cards (merging)");
        Debug.Log("- M: Merge selected cards");
        Debug.Log("- S: Swap card (swapping)");
        Debug.Log("- R: Shuffle hand");
        Debug.Log("- Alt: End turn");
        Debug.Log("- I: Display game info");
        Debug.Log("================");
    }

    private void SwitchToPlayerCamera(int playerIndex)
    {
        if (NetworkManager.Singleton.LocalClientId != (ulong)playerIndex) return;

        for (int i = 0; i < playerCameras.Length; i++)
        {
            if (playerCameras[i] != null)
            {
                playerCameras[i].enabled = i == playerIndex;
            }
        }
        if (playerCameras[playerIndex] != null)
        {
            Debug.Log($"Switched to Player {playerIndex}'s camera");
        }
        else
        {
            Debug.LogError($"Camera for Player {playerIndex} is null!");
        }
    }
}