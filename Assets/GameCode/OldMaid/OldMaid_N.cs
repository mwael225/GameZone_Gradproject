using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameSystem;
using Unity.Netcode;

public class OldMaid_N : CardGame_N
{
    string prefabpath = "Prefabs/Card_Deck_Shayeb";
    private List<Vector3> originalCardPositions;
    private List<Quaternion> originalCardRotations;
    public bool areLeftPlayerCardsDisplayed;
    GameObject origin;

    public OldMaid_N(InputHandler inputHandler) : base("Old Maid", 4, inputHandler)
    {
        Debug.Log("OldMaid_N constructor called");
        oldscale = new Vector3(7f, 7f, 7f);
        var prefab = Resources.Load<GameObject>("Prefabs_N/Card_Deck_Shayeb_N");
        if (prefab == null)
        {
            Debug.LogError("Failed to load prefab: Prefabs_N/Card_Deck_Shayeb_N");
            return;
        }
        Debug.Log("Loaded Card_Deck_Shayeb_N prefab");
        origin = GameObject.Instantiate(prefab);
        var networkObject = origin.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("NetworkObject component missing on Card_Deck_Shayeb_N prefab");
            return;
        }
        networkObject.Spawn();
        Debug.Log("Spawned Card_Deck_Shayeb_N with NetworkObject ID: " + networkObject.NetworkObjectId);

        GameObjects = spawnobjects(prefabpath);
        Debug.Log($"Spawned {GameObjects.Count} card objects from {prefabpath}");
        if (GameObjects.Count == 0)
        {
            Debug.LogError("No card objects were spawned!");
            return;
        }

        for (int i = 0; i < GameObjects.Count; i++)
        {
            var cardNetworkObject = GameObjects[i].GetComponent<NetworkObject>();
            if (cardNetworkObject != null)
            {
                cardNetworkObject.Spawn();
                GameObjects[i].transform.SetParent(origin.transform);
                Debug.Log($"Card {GameObjects[i].name} spawned with NetworkObject ID: {cardNetworkObject.NetworkObjectId}");
            }
            else
            {
                Debug.LogError($"Card {GameObjects[i].name} missing NetworkObject component");
            }
        }

        shuffledeck(GameObjects);
        Debug.Log("Deck shuffled");
        DealCards();
        Debug.Log("Cards dealt to players");
        setupposition();
        Debug.Log("Card positions set up");
        MovetoPostion();
        Debug.Log("Cards moved to positions");
        selectedCardsindex = new List<int>();
        centralpileLocalpos = new List<Vector3> { new Vector3(0, 0, 0) };
        discard_pilespcaing = new List<Vector3> { new Vector3(0, 0, 0.005f) };
        gamestate = "swapping";
        cplayer = 0;
        originalCardPositions = new List<Vector3>();
        originalCardRotations = new List<Quaternion>();
        areLeftPlayerCardsDisplayed = false;
        Debug.Log("OldMaid_N initialization complete. Game state: " + gamestate + ", Current player: " + cplayer);
    }

    public override void setupposition()
    {
        Debug.Log("Setting up card positions");
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
            Debug.Log($"Player {i} hand positions set up with {handspostions[i].Count} positions");
        }
    }

    public void ShufflePlayerHand(int player)
    {
        Debug.Log($"Shuffling hand for player {player}");
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
        Debug.Log($"Getting next player with cards from current player {currentPlayer}");
        int nextPlayer = currentPlayer;
        int attempts = 0;

        do
        {
            nextPlayer = (nextPlayer - 1 + numberOfPlayers) % numberOfPlayers;
            attempts++;
            if (attempts >= numberOfPlayers)
            {
                Debug.Log("No other players with cards found");
                return -1;
            }
        }
        while (hands[nextPlayer].Count == 0 && nextPlayer != currentPlayer);

        if (nextPlayer == currentPlayer)
        {
            Debug.Log("Current player is the only one with cards");
            return -1;
        }

        Debug.Log($"Next player with cards: {nextPlayer}");
        return nextPlayer;
    }

    public int GetLeftPlayer(int currentPlayer)
    {
        int leftPlayer = (currentPlayer - 1 + numberOfPlayers) % numberOfPlayers;
        Debug.Log($"Left player for {currentPlayer} is {leftPlayer}");
        return leftPlayer;
    }

    public void SwapCardFromTarget(int currentPlayer, int targetPlayer, int targetCardIndex)
    {
        Debug.Log($"Swapping card from player {targetPlayer} to player {currentPlayer} at index {targetCardIndex}");
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
        swappedCard.transform.localRotation = Quaternion.Euler(playerrotations[currentPlayer]);

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
        Debug.Log("Ending turn for player " + cplayer);
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
            Debug.Log("Game over");
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
        bool gameOver = playersWithCards <= 1;
        Debug.Log($"IsGameOver: {gameOver}, Players with cards: {playersWithCards}");
        return gameOver;
    }

    public void SelectCard(int player, int cardIndex)
    {
        Debug.Log($"Selecting card for player {player} at index {cardIndex}");
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

    public void SelectCardFromTargetPlayer(int currentPlayer)
    {
        Debug.Log($"Selecting card from target player for player {currentPlayer}");
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
        SwapCardFromTarget(currentPlayer, targetPlayer, navigatedCardindex);
    }

    public void AttemptMerge(int player)
    {
        Debug.Log($"Attempting to merge cards for player {player}");
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
        Debug.Log($"Merging cards for player {player} at indices {firstIndex}, {secondIndex}");
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
        Debug.Log($"Clearing selection for player {player}");
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
        Debug.Log($"Updating hand positions for player {player}");
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
            Debug.Log($"Card {hands[player][j].name} positioned at {handspostions[player][j]} for player {player}");
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
        Debug.Log($"Base position for player {player}: {basePositions[player]}");
        return basePositions[player];
    }

    private void CheckWinCondition(int player)
    {
        Debug.Log($"Checking win condition for player {player}");
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

    public void HighlightTargetPlayerCard(int targetPlayer, int cardIndex)
    {
        Debug.Log($"Highlighting card for player {targetPlayer} at index {cardIndex}");
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
            Debug.Log($"Highlighted card {navigatedCard.name} for player {targetPlayer}");
        }
    }
}