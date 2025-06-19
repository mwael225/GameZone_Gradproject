//This file defines the core logic for the Old Maid card game in Unity. It handles game setup, card dealing, shuffling, player hand management, card swapping, merging pairs, tracking game state, and determining win/loss conditions, including AI behavior for non-human players.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameSystem;

public class OldMaid : CardGame
{
    string prefabpath = "Prefabs/Card_Deck_Shayeb";
    private List<Vector3> originalCardPositions;
    private List<Quaternion> originalCardRotations;
    public bool areLeftPlayerCardsDisplayed;
    private List<int> finishOrder;

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
        gamestate = "swapping";
        cplayer = 0;
        originalCardPositions = new List<Vector3>();
        originalCardRotations = new List<Quaternion>();
        areLeftPlayerCardsDisplayed = false;
        finishOrder = new List<int>();
    }

    public List<int> GetFinishOrder()
    {
        return new List<int>(finishOrder);
    }

    public override void setupposition()
    {
        cardSpacing = new List<Vector3>()
        {
            new Vector3(-0.034975f, 0f, 0.01f)*6,
            new Vector3(0.001f, -0.034975f, 0)*6,
            new Vector3(-0.034975f, 0.001f, 0)*6,
            new Vector3(0.001f, -0.034975f, 0)*6,
        };
        playerrotations = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(0, -90, -90), 
            new Vector3(-90, 0, 180),
            new Vector3(0, 90, 90)
        };

        handspostions = new List<List<Vector3>>()
        {
            new List<Vector3> { new Vector3(0.25f, -0.32f, 0.3f)*5 },
            new List<Vector3> { new Vector3(-0.612f, 0.231f, 0.3f)*5 },
            new List<Vector3> { new Vector3(0.25f, 0.49f, 0.3f)*5 },
            new List<Vector3> { new Vector3(0.612f, 0.231f, 0.3f)*5},
        };
        for (int i = 0; i < hands.Count; i++)
        {
            for (int j = 0; j < hands[i].Count; j++)
            {
                if (i == 0)
                {
                    Vector3 lastPos = handspostions[i][handspostions[i].Count - 1];
                    handspostions[i].Add(new Vector3(lastPos.x + cardSpacing[i].x, lastPos.y, lastPos.z + 0.01f));
                }
                else
                {
                    handspostions[i].Add(handspostions[i][handspostions[i].Count - 1] + cardSpacing[i]);
                }
            }
        }
    }
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
        ClearSelection(player);
        List<GameObject> shuffledHand = new List<GameObject>(hands[player]);
        System.Random rand = new System.Random();
        for (int i = shuffledHand.Count - 1; i > 0; i--)
        {
            int randomIndex = rand.Next(0, i + 1);
            GameObject temp = shuffledHand[i];
            shuffledHand[i] = shuffledHand[randomIndex];
            shuffledHand[randomIndex] = temp;
        }
        hands[player] = shuffledHand;
        navigatedCardindex = 0;
        UpdateHandPositions(player);

        Debug.Log("Player " + player + " shuffled their hand! Cards have been reorganized.");
    }
    public int GetNextPlayerWithCards(int currentPlayer)
    {
        int nextPlayer = currentPlayer;
        int attempts = 0;

        do
        {
            nextPlayer = (nextPlayer - 1 + numberOfPlayers) % numberOfPlayers;
            attempts++;
            if (attempts >= numberOfPlayers)
            {
                return -1;
            }
        }
        while (hands[nextPlayer].Count == 0 && nextPlayer != currentPlayer);
        if (nextPlayer == currentPlayer)
        {
            return -1;
        }

        return nextPlayer;
    }
    public int GetLeftPlayer(int currentPlayer)
    {
        return (currentPlayer - 1 + numberOfPlayers) % numberOfPlayers;
    }
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
        GameObject swappedCard = hands[targetPlayer][targetCardIndex];
        hands[targetPlayer].RemoveAt(targetCardIndex);
        hands[currentPlayer].Add(swappedCard);
        swappedCard.transform.localRotation = Quaternion.Euler(currentPlayer == 0 ? new Vector3(0, 0, 0) : playerrotations[currentPlayer]);
        Debug.Log("Player " + currentPlayer + " swapped a card from Player " + targetPlayer);
        areLeftPlayerCardsDisplayed = false;
        UpdateHandPositions(targetPlayer);
        UpdateHandPositions(currentPlayer);
        navigatedCardindex = hands[currentPlayer].Count - 1;
        gamestate = "merging";

        Debug.Log("Player " + currentPlayer + " can now merge cards. Press M to merge selected cards, Alt to end turn.");
    }
    public void EndTurn()
    {
        selectedCardsindex.Clear();
        do
        {
            cplayer = (cplayer + 1) % numberOfPlayers;
        } while (hands[cplayer].Count == 0 && !IsGameOver());
        navigatedCardindex = 0;
        if (hands[cplayer].Count > 0)
        {
            navigatedCardindex = hands[cplayer].Count > navigatedCardindex ? navigatedCardindex : hands[cplayer].Count - 1;
        }
        if (IsGameOver())
        {
            gamestate = "end";
        }
        else
        {
            gamestate = "swapping";
            areLeftPlayerCardsDisplayed = false;
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
    private bool IsGameOver()
    {
        int playersWithCards = 0;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (hands[i].Count > 0) playersWithCards++;
        }
        return playersWithCards <= 1;
    }
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
            selectedCardsindex.Remove(cardIndex);
            hands[player][cardIndex].GetComponent<Renderer>().material.color = Color.white;
            Debug.Log("Card deselected at index: " + cardIndex);
        }
        else
        {
            if (selectedCardsindex.Count >= 2)
            {
                Debug.Log("Cannot select more than 2 cards!");
                return;
            }
            selectedCardsindex.Add(cardIndex);
            hands[player][cardIndex].GetComponent<Renderer>().material.color = Color.yellow;
            Debug.Log("Card selected at index: " + cardIndex);
        }
    }
    public void SelectCardFromTargetPlayer(int currentPlayer, int cardIndex)
    {
        int targetPlayer = GetNextPlayerWithCards(currentPlayer);
        if (targetPlayer == -1)
        {
            Debug.Log("No other players have cards! Moving to merging phase.");
            gamestate = "merging";
            areLeftPlayerCardsDisplayed = false;
            return;
        }

        if (cardIndex >= hands[targetPlayer].Count)
        {
            cardIndex = 0;
        }
        SwapCardFromTarget(currentPlayer, targetPlayer, cardIndex);
    }
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
    private void UpdateHandPositions(int player)
    {
        handspostions[player].Clear();
        if (player == 0) 
        {
            handspostions[player].Add(new Vector3(0.25f * 5, -0.32f * 5, 0.3f * 5)); 
            for (int j = 1; j < hands[player].Count; j++)
            {
                Vector3 lastPos = handspostions[player][j - 1];
                handspostions[player].Add(new Vector3(lastPos.x + cardSpacing[player].x, lastPos.y, lastPos.z + 0.01f));
            }
        }
        else
        {
            handspostions[player].Add(GetBasePosition(player));
            for (int j = 1; j < hands[player].Count; j++)
            {
                handspostions[player].Add(handspostions[player][j - 1] + cardSpacing[player]);
            }
        }

        for (int j = 0; j < hands[player].Count; j++)
        {
            hands[player][j].transform.localPosition = handspostions[player][j];
            hands[player][j].transform.localRotation = Quaternion.Euler(player == 0 ? new Vector3(0, 0, 0) : playerrotations[player]);
        }
    }
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
    private void CheckWinCondition(int player)
    {
        if (hands[player].Count == 0 && !finishOrder.Contains(player))
        {
            finishOrder.Add(player);
            Debug.Log("Player " + player + " has no cards left!");
        }
        else if (hands[player].Count == 1 && GetCardValue(hands[player][0]) == "King")
        {
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);
            }
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

        if (playersWithCards == 1 && !finishOrder.Contains(lastPlayerWithCards))
        {
            finishOrder.Add(lastPlayerWithCards);
            Debug.Log("Game Over! Player " + lastPlayerWithCards + " loses with the Old Maid!");
            gamestate = "end";
        }
    }
    public void HighlightTargetPlayerCard(int targetPlayer, int cardIndex)
    {
        for (int i = 0; i < hands[targetPlayer].Count; i++)
        {
            hands[targetPlayer][i].transform.localScale = oldscale;
            hands[targetPlayer][i].GetComponent<Renderer>().material.color = Color.white;
        }
        if (cardIndex >= 0 && cardIndex < hands[targetPlayer].Count)
        {
            GameObject navigatedCard = hands[targetPlayer][cardIndex];
            navigatedCard.transform.localScale = oldscale * 1.2f;
            navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
        }
    }

    public int AISelectRandomCard(int targetPlayer)
    {
        if (hands[targetPlayer].Count == 0)
        {
            return -1;
        }
        System.Random rand = new System.Random();
        int navigationSteps = rand.Next(1, 4);
        int currentIndex = navigatedCardindex;

        for (int i = 0; i < navigationSteps; i++)
        {
            bool forward = rand.Next(0, 2) == 0;
            currentIndex = forward ?
                (currentIndex + 1) % hands[targetPlayer].Count :
                (currentIndex - 1 + hands[targetPlayer].Count) % hands[targetPlayer].Count;
        }

        return currentIndex;
    }

    public List<List<int>> AISelectCardsToMerge(int player)
    {
        List<List<int>> mergeablePairs = new List<List<int>>();
        Dictionary<string, List<int>> valueToIndices = new Dictionary<string, List<int>>();
        for (int i = 0; i < hands[player].Count; i++)
        {
            string value = GetCardValue(hands[player][i]);
            if (value != "King")
            {
                if (!valueToIndices.ContainsKey(value))
                {
                    valueToIndices[value] = new List<int>();
                }
                valueToIndices[value].Add(i);
            }
        }
        foreach (var pair in valueToIndices)
        {
            if (pair.Value.Count >= 2)
            {
                for (int i = 0; i < pair.Value.Count - 1; i += 2)
                {
                    if (i + 1 < pair.Value.Count)
                    {
                        mergeablePairs.Add(new List<int> { pair.Value[i], pair.Value[i + 1] });
                    }
                }
            }
        }

        return mergeablePairs;
    }
    public bool HasKingCard(int player)
    {
        foreach (GameObject card in hands[player])
        {
            if (GetCardValue(card) == "King")
            {
                return true;
            }
        }
        return false;
    }
}