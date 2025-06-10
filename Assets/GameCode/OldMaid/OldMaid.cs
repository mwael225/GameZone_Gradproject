//OldMaid.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameSystem;

public class OldMaid : CardGame
{
    string prefabpath = "Prefabs/Card_Deck_Shayeb";
    private List<Vector3> originalCardPositions; // Store original positions of left player's cards
    private List<Quaternion> originalCardRotations; // Store original rotations of left player's cards
    public bool areLeftPlayerCardsDisplayed; // Flag to track if left player's cards are displayed

    public OldMaid() : base("Old Maid", 4)
    {
        oldscale = new Vector3(7f, 7f, 7f);
        GameObjects = prefabtoGamebojects(prefabpath);
        shuffledeck(GameObjects);
        DealCards();
        setupposition();
        MovetoPostion();
        selectedCardsindex = new List<int>();
        centralpileLocalpos = new List<Vector3> { new Vector3(0, 0, 0) };
        discard_pilespcaing = new List<Vector3> { new Vector3(0, 0, 0.005f) };
        gamestate = "swapping"; // Start with swapping phase
        cplayer = 0; // Start with player 0
        originalCardPositions = new List<Vector3>();
        originalCardRotations = new List<Quaternion>();
        areLeftPlayerCardsDisplayed = false;
    }

    public override void setupposition()
    {
        cardSpacing = new List<Vector3>()
        {
            new Vector3(-0.034975f, 0.001f, 0)*6,
            new Vector3(0.001f, -0.034975f, 0)*6,
            new Vector3(-0.034975f, 0.001f, 0)*6,
            new Vector3(0.001f, -0.034975f, 0)*6,
        };
        playerrotations = new List<Vector3>
        {
            new Vector3(90, 0, 0), new Vector3(0, -90, -90), new Vector3(-90,0 ,180 ), new Vector3(0,90, 90)
        };

        handspostions = new List<List<Vector3>>()
        {
            new List<Vector3> { new Vector3(0.25f, -0.49f, 0.3f)*5 },
            new List<Vector3> { new Vector3(-0.612f, 0.231f, 0.3f)*5 },
            new List<Vector3> { new Vector3(0.25f, 0.49f, 0.3f)*5 },
            new List<Vector3> { new Vector3(0.612f, 0.231f, 0.3f)*5},
        };
        for (int i = 0; i < hands.Count; i++)
        {
            for (int j = 0; j < hands[i].Count; j++)
            {
                handspostions[i].Add(handspostions[i][handspostions[i].Count - 1] + cardSpacing[i]);
            }
        }
    }

    // Method to shuffle a player's hand
    public void ShufflePlayerHand(int player)
    {
        if (player != cplayer)
        {
            Debug.Log("Can only shuffle cards during your own turn!");
            return;
        }

        if (hands[player].Count <= 1)
        {
            Debug.Log("Not enough cards to shuffle!");
            return;
        }

        // Clear any current selections before shuffling
        ClearSelection(player);

        // Create a copy of the current hand to shuffle
        List<GameObject> shuffledHand = new List<GameObject>(hands[player]);

        // Fisher-Yates shuffle algorithm
        System.Random rand = new System.Random();
        for (int i = shuffledHand.Count - 1; i > 0; i--)
        {
            int randomIndex = rand.Next(0, i + 1);
            GameObject temp = shuffledHand[i];
            shuffledHand[i] = shuffledHand[randomIndex];
            shuffledHand[randomIndex] = temp;
        }

        // Replace the player's hand with the shuffled version
        hands[player] = shuffledHand;

        // Reset navigation index to 0 after shuffle
        navigatedCardindex = 0;

        // Update positions for the shuffled cards
        UpdateHandPositions(player);

        Debug.Log("Player " + player + " shuffled their hand! Cards have been reorganized.");
    }

    // Method to get the next player to the left who has cards
    public int GetNextPlayerWithCards(int currentPlayer)
    {
        int nextPlayer = currentPlayer;
        int attempts = 0;

        do
        {
            nextPlayer = (nextPlayer - 1 + numberOfPlayers) % numberOfPlayers;
            attempts++;

            // If we've checked all players and none have cards, return -1
            if (attempts >= numberOfPlayers)
            {
                return -1;
            }
        }
        while (hands[nextPlayer].Count == 0 && nextPlayer != currentPlayer);

        // If we looped back to current player, no one else has cards
        if (nextPlayer == currentPlayer)
        {
            return -1;
        }

        return nextPlayer;
    }

    // Method to get the player to the left (previous player in turn order) - kept for compatibility
    public int GetLeftPlayer(int currentPlayer)
    {
        return (currentPlayer - 1 + numberOfPlayers) % numberOfPlayers;
    }

    // Method to swap a card from the target player
    public void SwapCardFromTarget(int currentPlayer, int targetPlayer, int targetCardIndex)
    {
        if (hands[targetPlayer].Count == 0)
        {
            Debug.Log("Target player has no cards to swap! Moving to merging phase.");
            gamestate = "merging";
            areLeftPlayerCardsDisplayed = false;
            return;
        }

        if (targetCardIndex < 0 || targetCardIndex >= hands[targetPlayer].Count)
        {
            Debug.Log("Invalid target card index!");
            areLeftPlayerCardsDisplayed = false;
            return;
        }

        // Take the card from target player
        GameObject swappedCard = hands[targetPlayer][targetCardIndex];
        hands[targetPlayer].RemoveAt(targetCardIndex);

        // Add to current player's hand
        hands[currentPlayer].Add(swappedCard);

        // Rotate the swapped card to match current player's hand orientation
        swappedCard.transform.localRotation = Quaternion.Euler(playerrotations[currentPlayer]);

        Debug.Log("Player " + currentPlayer + " swapped a card from Player " + targetPlayer);

        areLeftPlayerCardsDisplayed = false;

        // Update positions for both players
        UpdateHandPositions(targetPlayer);
        UpdateHandPositions(currentPlayer);

        // Reset navigation to the new card
        navigatedCardindex = hands[currentPlayer].Count - 1;

        // Change game state to merging after swap
        gamestate = "merging";

        Debug.Log("Player " + currentPlayer + " can now merge cards. Press M to merge selected cards, Alt to end turn.");
    }

    // Method to end current player's turn
    public void EndTurn()
    {
        // Clear any selections
        selectedCardsindex.Clear();

        // Move to next player
        do
        {
            cplayer = (cplayer + 1) % numberOfPlayers;
        } while (hands[cplayer].Count == 0 && !IsGameOver());

        // Reset navigation
        navigatedCardindex = 0;
        if (hands[cplayer].Count > 0)
        {
            navigatedCardindex = hands[cplayer].Count > navigatedCardindex ? navigatedCardindex : hands[cplayer].Count - 1;
        }

        // Set game state for next player
        if (IsGameOver())
        {
            gamestate = "end";
        }
        else
        {
            gamestate = "swapping"; // Start with swapping phase
            areLeftPlayerCardsDisplayed = false; // Reset display flag for new turn
            Debug.Log("=== Player " + cplayer + "'s turn ===");

            int targetPlayer = GetNextPlayerWithCards(cplayer);
            if (targetPlayer != -1)
            {
                Debug.Log("Navigate Player " + targetPlayer + "'s cards with Q/W, select with S");
            }
            else
            {
                Debug.Log("No other players have cards! Moving to merging phase.");
                gamestate = "merging";
            }
        }
    }

    // Method to check if game is over
    private bool IsGameOver()
    {
        int playersWithCards = 0;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (hands[i].Count > 0) playersWithCards++;
        }
        return playersWithCards <= 1;
    }

    // Method to select/deselect a card
    public void SelectCard(int player, int cardIndex)
    {
        if (gamestate != "merging")
        {
            Debug.Log("Can only select cards during merging phase!");
            return;
        }

        if (cardIndex < 0 || cardIndex >= hands[player].Count)
        {
            Debug.Log("Invalid card index!");
            return;
        }

        if (selectedCardsindex.Contains(cardIndex))
        {
            // Deselect the card
            selectedCardsindex.Remove(cardIndex);
            hands[player][cardIndex].GetComponent<Renderer>().material.color = Color.white;
            Debug.Log("Card deselected at index: " + cardIndex);
        }
        else
        {
            // Only allow selecting up to 2 cards
            if (selectedCardsindex.Count >= 2)
            {
                Debug.Log("Cannot select more than 2 cards!");
                return;
            }
            // Select the card
            selectedCardsindex.Add(cardIndex);
            hands[player][cardIndex].GetComponent<Renderer>().material.color = Color.yellow;
            Debug.Log("Card selected at index: " + cardIndex);
        }
    }

    // Method to select card from target player for swapping
    public void SelectCardFromTargetPlayer(int currentPlayer)
    {
        int targetPlayer = GetNextPlayerWithCards(currentPlayer);

        if (targetPlayer == -1)
        {
            Debug.Log("No other players have cards! Moving to merging phase.");
            gamestate = "merging";
            areLeftPlayerCardsDisplayed = false;
            return;
        }

        if (navigatedCardindex >= hands[targetPlayer].Count)
        {
            navigatedCardindex = 0;
        }

        // Perform swap
        SwapCardFromTarget(currentPlayer, targetPlayer, navigatedCardindex);
    }

    // Method to attempt merging selected cards
    public void AttemptMerge(int player)
    {
        if (gamestate != "merging")
        {
            Debug.Log("Can only merge cards during merging phase!");
            return;
        }

        if (selectedCardsindex.Count != 2)
        {
            Debug.Log("Must select exactly two cards to merge!");
            return;
        }

        int firstCardIndex = selectedCardsindex[0];
        int secondCardIndex = selectedCardsindex[1];

        if (firstCardIndex >= hands[player].Count || secondCardIndex >= hands[player].Count)
        {
            Debug.Log("Invalid card indices selected!");
            ClearSelection(player);
            return;
        }

        string firstCardValue = GetCardValue(hands[player][firstCardIndex]);
        string secondCardValue = GetCardValue(hands[player][secondCardIndex]);

        Debug.Log("Comparing cards: '" + firstCardValue + "' vs '" + secondCardValue + "'");

        if (firstCardValue == secondCardValue && firstCardValue != "King")
        {
            Debug.Log("SUCCESS: Merging cards with value: " + firstCardValue);
            MergeCards(player, firstCardIndex, secondCardIndex);
        }
        else
        {
            Debug.Log("FAILED: Cards cannot be merged. First: '" + firstCardValue + "', Second: '" + secondCardValue + "'");
            if (firstCardValue == "King" || secondCardValue == "King")
            {
                Debug.Log("Reason: King cards cannot be merged");
            }
            else if (firstCardValue != secondCardValue)
            {
                Debug.Log("Reason: Different card values");
            }
            ClearSelection(player);
        }
    }

    // Method to merge two selected cards
    private void MergeCards(int player, int firstIndex, int secondIndex)
    {
        List<int> indicesToRemove = new List<int> { firstIndex, secondIndex };
        indicesToRemove.Sort((a, b) => b.CompareTo(a));

        foreach (int index in indicesToRemove)
        {
            throwCard(player, index);
        }

        selectedCardsindex.Clear();
        UpdateHandPositions(player);

        if (navigatedCardindex >= hands[player].Count)
        {
            navigatedCardindex = hands[player].Count > 0 ? hands[player].Count - 1 : 0;
        }

        Debug.Log("Cards merged successfully. Player " + player + " now has " + hands[player].Count + " cards.");
        CheckWinCondition(player);
    }

    // Method to clear current selection
    private void ClearSelection(int player)
    {
        foreach (int index in selectedCardsindex)
        {
            if (index < hands[player].Count)
            {
                hands[player][index].GetComponent<Renderer>().material.color = Color.white;
            }
        }
        selectedCardsindex.Clear();
    }

    // Method to extract card value from GameObject name
    private string GetCardValue(GameObject card)
    {
        string cardName = card.name;
        Debug.Log("Extracting value from card: " + cardName);

        cardName = cardName.Replace("(Clone)", "").Trim();

        if (cardName.StartsWith("Card_"))
        {
            string suitAndValue = cardName.Substring(5);
            string value = "";

            if (suitAndValue.EndsWith("Ace"))
            {
                value = "Ace";
            }
            else if (suitAndValue.EndsWith("King"))
            {
                value = "King";
            }
            else if (suitAndValue.EndsWith("Queen"))
            {
                value = "Queen";
            }
            else if (suitAndValue.EndsWith("Jack"))
            {
                value = "Jack";
            }
            else
            {
                for (int i = 10; i >= 2; i--)
                {
                    if (suitAndValue.EndsWith(i.ToString()))
                    {
                        value = i.ToString();
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(value))
            {
                Debug.Log("Successfully extracted value: " + value + " from " + cardName);
                return value;
            }
        }

        Debug.LogError("Could not extract card value from: " + cardName + ". Expected format: Card_[Suit][Value]");
        return cardName;
    }

    // Method to update hand positions after card removal
    private void UpdateHandPositions(int player)
    {
        handspostions[player].Clear();
        handspostions[player].Add(GetBasePosition(player));

        for (int j = 1; j < hands[player].Count; j++)
        {
            handspostions[player].Add(handspostions[player][j - 1] + cardSpacing[player]);
        }

        for (int j = 0; j < hands[player].Count; j++)
        {
            hands[player][j].transform.localPosition = handspostions[player][j];
            hands[player][j].transform.localRotation = Quaternion.Euler(playerrotations[player]);
        }
    }

    // Helper method to get base position for each player
    private Vector3 GetBasePosition(int player)
    {
        Vector3[] basePositions = {
            new Vector3(0.25f, -0.49f, 0.3f)*5,
            new Vector3(-0.612f, 0.231f, 0.3f)*5,
            new Vector3(0.25f, 0.49f, 0.3f)*5,
            new Vector3(0.612f, 0.231f, 0.3f)*5
        };
        return basePositions[player];
    }

    // Method to check win condition
    private void CheckWinCondition(int player)
    {
        if (hands[player].Count == 0)
        {
            Debug.Log("Player " + player + " has no cards left!");
        }
        else if (hands[player].Count == 1 && GetCardValue(hands[player][0]) == "King")
        {
            Debug.Log("Player " + player + " loses! They have the Old Maid (King)!");
            gamestate = "end";
        }

        int playersWithCards = 0;
        int lastPlayerWithCards = -1;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (hands[i].Count > 0)
            {
                playersWithCards++;
                lastPlayerWithCards = i;
            }
        }

        if (playersWithCards == 1)
        {
            Debug.Log("Game Over! Player " + lastPlayerWithCards + " loses with the Old Maid!");
            gamestate = "end";
        }
    }

    // Method to highlight a specific card in target player's hand during navigation
    public void HighlightTargetPlayerCard(int targetPlayer, int cardIndex)
    {
        // Reset all target player's cards to normal appearance
        for (int i = 0; i < hands[targetPlayer].Count; i++)
        {
            hands[targetPlayer][i].transform.localScale = oldscale;
            hands[targetPlayer][i].GetComponent<Renderer>().material.color = Color.white;
        }

        // Highlight the navigated card
        if (cardIndex >= 0 && cardIndex < hands[targetPlayer].Count)
        {
            GameObject navigatedCard = hands[targetPlayer][cardIndex];
            navigatedCard.transform.localScale = oldscale * 1.2f;
            navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
        }
    }
}