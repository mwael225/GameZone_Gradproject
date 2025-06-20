using System;
using System.Linq;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem
{
    public class UnoGameManager : GameManager
    {
        Uno UnoGame;
        private bool hasDrawn = false; // Tracks if the current player has drawn a card this turn

        public void Start()
        {
            UnoGame = new Uno();
            UnoGame.gamestate = "playing";
        }

        public void Update()
        {
            if (UnoGame.gamestate != "end")
            {
                StartCoroutine(UnoGame.navigatedCards(currentTurn));

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    UnoGame.throwCard(currentTurn, UnoGame.navigatedCardindex);
                    Debug.Log(UnoGame.Playable);
                    if (UnoGame.Playable)
                    {
                        // Check if current player has won (no cards left)
                        if (UnoGame.hands[currentTurn].Count == 0)
                        {
                            Debug.Log("Player " + (currentTurn + 1) + " wins the game!");
                            UnoGame.gamestate = "end";
                        }
                        else
                        {
                            // If not, proceed to next player
                            UnoGame.navigatedCardindex = 0;
                            currentTurn = NextTurn(UnoGame.numberOfPlayers);
                            hasDrawn = false; // Reset draw status for next player
                        }
                        UnoGame.Playable = false;
                    }
                }
                // Allow drawing only once per turn
                else if (Input.GetKeyDown(KeyCode.Alpha1) && !hasDrawn)
                {
                    GameObject drawnCard = UnoGame.PickCard(currentTurn);
                    if (drawnCard != null)
                    {
                        hasDrawn = true; // Mark that player has drawn a card this turn
                        Debug.Log("Player drew a card. Press Enter to play it if valid, or press 2 to skip your turn.");
                    }
                    else
                    {
                        Debug.LogWarning("No cards available to draw. Continue with your turn.");
                    }
                }
                // Add option to skip turn after drawing
                else if (Input.GetKeyDown(KeyCode.Alpha2) && hasDrawn)
                {
                    Debug.Log("Player skipped their turn after drawing.");
                    UnoGame.navigatedCardindex = 0;
                    currentTurn = NextTurn(UnoGame.numberOfPlayers);
                    hasDrawn = false; // Reset draw status for the next player
                }
            }

        }
        public override int NextTurn(int noOfPlayers)
        {
            int nextPlayer = currentTurn;

            // Determine direction based on reverse status
            if (UnoGame.reverse)
            {
                // Move one player in reverse direction
                nextPlayer = (nextPlayer - 1 + noOfPlayers) % noOfPlayers;
            }
            else
            {
                // Move one player in normal direction
                nextPlayer = (nextPlayer + 1) % noOfPlayers;
            }

            // Apply skip if needed - moves one more player in current direction
            if (UnoGame.skip)
            {
                if (UnoGame.reverse)
                {
                    nextPlayer = (nextPlayer - 1 + noOfPlayers) % noOfPlayers;
                }
                else
                {
                    nextPlayer = (nextPlayer + 1) % noOfPlayers;
                }
                UnoGame.skip = false; // Reset skip flag
            }

            return nextPlayer;
        }
    }
}