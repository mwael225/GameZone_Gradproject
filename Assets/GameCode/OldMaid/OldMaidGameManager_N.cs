using UnityEngine;
using System.Collections;
using GameSystem;
using Unity.Netcode;

namespace GameSystem
{
    public class OldMaidGameManager_N : GameManager_N
    {
        OldMaid_N oldmaid;
        private int navigatedCardIndexTarget = 0;
        InputHandler inputHandler;
        [SerializeField] private Camera[] playerCameras;

        public void Start()
        {
            if (!IsServer) return;
            Debug.Log("OldMaidGameManager_N Start called on server");
            inputHandler = GetComponent<InputHandler>();
            if (inputHandler == null)
            {
                Debug.LogError("InputHandler component not found on GameManager_N!");
                return;
            }
            oldmaid = new OldMaid_N(inputHandler);
            currentTurn = 0;
            oldmaid.gamestate = "swapping";
            if (oldmaid.targetPlayer != -1)
                Debug.Log($"Old Maid game started. Player {oldmaid.cplayer}'s turn. Navigate Player {oldmaid.targetPlayer}'s cards with Q/W, select with S.");
            else
            {
                Debug.Log($"Old Maid game started. Player {oldmaid.cplayer}'s turn. No other players have cards - moving to merging.");
                oldmaid.gamestate = "merging";
            }
        }

        public void Update()
        {
            if (!IsServer) return;
            if (oldmaid.gamestate == "end")
            {
                Debug.Log("Game ended");
                return;
            }
            if (oldmaid.gamestate == "swapping")
                HandleSwappingNavigation();
            else if (oldmaid.gamestate == "merging" && oldmaid.hands[oldmaid.cplayer].Count > 0)
                StartCoroutine(oldmaid.navigatedCards(oldmaid.cplayer));
            HandleInput();
        }

        private void HandleInput()
        {
            if (oldmaid.gamestate == "merging")
                HandleMergingInput();
            else if (oldmaid.gamestate == "swapping")
                HandleSwappingInput();
            HandleShuffleInput();
            if (inputHandler.GetKeyDown(KeyCode.I, oldmaid.cplayer))
                DisplayGameInfo();
        }

        private void HandleMergingInput()
        {
            if (inputHandler.GetKeyDown(KeyCode.Space, oldmaid.cplayer) || inputHandler.GetKeyDown(KeyCode.Return, oldmaid.cplayer))
            {
                if (oldmaid.hands[oldmaid.cplayer].Count > 0 && oldmaid.navigatedCardindex < oldmaid.hands[oldmaid.cplayer].Count)
                    oldmaid.SelectCard(oldmaid.cplayer, oldmaid.navigatedCardindex);
                else
                    Debug.Log("No cards to select or invalid index!");
            }
            if (inputHandler.GetKeyDown(KeyCode.M, oldmaid.cplayer))
                oldmaid.AttemptMerge(oldmaid.cplayer);
            if (inputHandler.GetKeyDown(KeyCode.LeftAlt, oldmaid.cplayer))
            {
                Debug.Log($"Player {oldmaid.cplayer} ending turn");
                oldmaid.EndTurn();
                currentTurn = oldmaid.cplayer; // Sync with game state
            }
            if (inputHandler.GetKeyDown(KeyCode.Escape, oldmaid.cplayer))
                ClearAllSelections();
        }

        private void HandleSwappingInput()
        {
            if (inputHandler.GetKeyDown(KeyCode.S, oldmaid.cplayer))
                SelectCardFromTargetPlayer();
        }

        private void HandleShuffleInput()
        {
            if (inputHandler.GetKeyDown(KeyCode.R, oldmaid.cplayer))
                oldmaid.ShufflePlayerHand(oldmaid.cplayer);
        }

        private void SelectCardFromTargetPlayer()
        {
            if (oldmaid.targetPlayer == -1)
            {
                Debug.Log("No other players have cards! Moving to merging.");
                oldmaid.gamestate = "merging";
                oldmaid.areLeftPlayerCardsDisplayed = false;
                return;
            }
            if (navigatedCardIndexTarget >= oldmaid.hands[oldmaid.targetPlayer].Count)
                navigatedCardIndexTarget = oldmaid.hands[oldmaid.targetPlayer].Count - 1;
            oldmaid.navigatedCardindex = navigatedCardIndexTarget;
            oldmaid.SelectCardFromTargetPlayer(oldmaid.cplayer, navigatedCardIndexTarget);
        }

        private void HandleSwappingNavigation()
        {
            if (oldmaid.gamestate != "swapping") return;
            if (oldmaid.targetPlayer == -1)
            {
                Debug.Log("No other players have cards! Moving to merging.");
                oldmaid.gamestate = "merging";
                oldmaid.areLeftPlayerCardsDisplayed = false;
                return;
            }
            bool navigationChanged = false;
            if (inputHandler.GetKeyDown(KeyCode.Q, oldmaid.cplayer))
            {
                navigatedCardIndexTarget = (navigatedCardIndexTarget - 1 + oldmaid.hands[oldmaid.targetPlayer].Count) % oldmaid.hands[oldmaid.targetPlayer].Count;
                navigationChanged = true;
            }
            else if (inputHandler.GetKeyDown(KeyCode.W, oldmaid.cplayer))
            {
                navigatedCardIndexTarget = (navigatedCardIndexTarget + 1) % oldmaid.hands[oldmaid.targetPlayer].Count;
                navigationChanged = true;
            }
            if (navigationChanged)
            {
                oldmaid.navigatedCardindex = navigatedCardIndexTarget;
                oldmaid.HighlightTargetPlayerCard(oldmaid.targetPlayer, navigatedCardIndexTarget);
            }
        }

        private void ClearAllSelections()
        {
            foreach (int index in oldmaid.selectedCardsindex)
                if (index < oldmaid.hands[oldmaid.cplayer].Count)
                    oldmaid.hands[oldmaid.cplayer][index].GetComponent<Renderer>().material.color = Color.white;
            oldmaid.selectedCardsindex.Clear();
            Debug.Log("All selections cleared");
        }

        private void DisplayGameInfo()
        {
            Debug.Log("=== Game Info ===");
            Debug.Log($"Current Player: {oldmaid.cplayer}");
            Debug.Log($"Game State: {oldmaid.gamestate}");
            if (oldmaid.hands[oldmaid.cplayer].Count > 0)
                Debug.Log($"Navigated Card Index: {oldmaid.navigatedCardindex}");
            if (oldmaid.gamestate == "merging")
            {
                Debug.Log($"Selected Cards: {string.Join(", ", oldmaid.selectedCardsindex)}");
                Debug.Log($"Player {oldmaid.cplayer} has {oldmaid.hands[oldmaid.cplayer].Count} cards");
            }
            else if (oldmaid.gamestate == "swapping")
            {
                if (oldmaid.targetPlayer != -1)
                {
                    Debug.Log($"Swapping card from Player {oldmaid.targetPlayer}");
                    Debug.Log($"Target player has {oldmaid.hands[oldmaid.targetPlayer].Count} cards");
                }
                else
                    Debug.Log("No other players have cards to swap from");
            }
            for (int i = 0; i < oldmaid.numberOfPlayers; i++)
                Debug.Log($"Player {i}: {oldmaid.hands[i].Count} cards");
            Debug.Log("Controls:");
            Debug.Log("- Q/W: Navigate cards");
            Debug.Log("- Space: Select/deselect cards (merging)");
            Debug.Log("- M: Merge selected cards");
            Debug.Log("- S: Swap card (swapping)");
            Debug.Log("- R: Shuffle hand");
            Debug.Log("- Alt: End turn");
            Debug.Log("- I: Display game info");
            Debug.Log("================");
        }

        // Client-side camera management
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                int playerIndex = (int)NetworkManager.Singleton.LocalClientId;
                if (playerCameras != null && playerIndex < playerCameras.Length && playerCameras[playerIndex] != null)
                {
                    playerCameras[playerIndex].enabled = true;
                    Debug.Log($"Client {playerIndex} activated camera");
                }
                else
                    Debug.LogError($"Camera for Player {playerIndex} not assigned!");
            }
        }
    }
}