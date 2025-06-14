//OldMaid_N.cs
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using GameSystem;
using System;

public class OldMaid_N : CardGame_N
{
    string prefabpath = "Prefabs/Card_Deck_Shayeb";
    private List<Vector3> originalCardPositions;
    private List<Quaternion> originalCardRotations;
    public bool areLeftPlayerCardsDisplayed;
    public int targetPlayer;
    GameObject origin; // Non-networked parent for cards
    GameObject cardDeck; // Networked OldMaid_CardDeck_N

    public OldMaid_N(InputHandler inputHandler) : base("Old Maid", 4, inputHandler)
    {
        oldscale = new Vector3(7f, 7f, 7f);
        origin = new GameObject("CardDeckParent"); // Non-networked parent
        cardDeck = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs_N/OldMaid_CardDeck_N"));
        NetworkObject deckNetObj = cardDeck.GetComponent<NetworkObject>();
        if (deckNetObj == null)
        {
            Debug.LogError("OldMaid_CardDeck_N prefab is missing a NetworkObject component!");
            return;
        }
        deckNetObj.Spawn();
        Debug.Log("OldMaid_CardDeck_N spawned with NetworkObject ID: " + deckNetObj.NetworkObjectId);
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartCoroutine(InitializeCards());
        }
        else
        {
            Debug.LogWarning("Card initialization skipped on client; waiting for server.");
        }
        selectedCardsindex = new List<int>();
        centralpileLocalpos = new List<Vector3> { new Vector3(0, 0, 0) };
        discard_pilespcaing = new List<Vector3> { new Vector3(0, 0, 0.005f) };
        gamestate = "swapping";
        cplayer = 0;
        originalCardPositions = new List<Vector3>();
        originalCardRotations = new List<Quaternion>();
        areLeftPlayerCardsDisplayed = false;
        targetPlayer = GetNextPlayerWithCards(cplayer);
    }

    private IEnumerator InitializeCards()
    {
        yield return new WaitForSeconds(1f); // Wait for GameManager to spawn
        Debug.Log("Initializing cards for Old Maid");
        GameObjects = spawnobjects(prefabpath);
        foreach (var card in GameObjects)
        {
            if (card != null)
            {
                card.transform.SetParent(origin.transform);
                Debug.Log($"Card {card.name} parented to {origin.name}");
            }
        }
        shuffledeck(GameObjects);
        DealCards();
        Assemble(deck);
        setupposition();
        MovetoPostion();
        Debug.Log("Card initialization complete");
    }

    public List<GameObject> spawnobjects(string path)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab at {path}");
            return new List<GameObject>();
        }
        List<GameObject> objects = new List<GameObject>();

        // Create cards as regular GameObjects without NetworkObject components
        for (int i = 0; i < 52; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab);

            // Remove NetworkObject component if it exists to avoid nesting issues
            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                Debug.Log($"Removing NetworkObject component from card {obj.name} to avoid nesting issues");
                GameObject.DestroyImmediate(netObj);
            }

            // Remove any child NetworkObjects to prevent nesting issues
            NetworkObject[] childNetObjs = obj.GetComponentsInChildren<NetworkObject>(true);
            foreach (var childNetObj in childNetObjs)
            {
                Debug.LogWarning($"Destroying nested NetworkObject on {childNetObj.gameObject.name} in card prefab {path}");
                GameObject.DestroyImmediate(childNetObj);
            }

            Debug.Log($"Card {obj.name} created as regular GameObject (no NetworkObject)");
            objects.Add(obj);
        }
        return objects;
    }

    public override void setupposition()
    {
        cardSpacing = new List<Vector3>
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
        handspostions = new List<List<Vector3>>
        {
            new List<Vector3> { new Vector3(0.25f, -0.49f, 0.3f)*5 },
            new List<Vector3> { new Vector3(-0.612f, 0.231f, 0.3f)*5 },
            new List<Vector3> { new Vector3(0.25f, 0.49f, 0.3f)*5 },
            new List<Vector3> { new Vector3(0.612f, 0.231f, 0.3f)*5 },
        };
        for (int i = 0; i < hands.Count; i++)
        {
            for (int j = 0; j < hands[i].Count; j++)
                handspostions[i].Add(handspostions[i][handspostions[i].Count - 1] + cardSpacing[i]);
        }
    }

    public void ShufflePlayerHand(int player)
    {
        if (player != cplayer)
        {
            Debug.Log("Can only shuffle during your turn!");
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
            (shuffledHand[i], shuffledHand[randomIndex]) = (shuffledHand[randomIndex], shuffledHand[i]);
        }
        hands[player] = shuffledHand;
        navigatedCardindex = 0;
        UpdateHandPositions(player);
        Debug.Log($"Player {player} shuffled their hand!");
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
                return -1;
        } while (hands[nextPlayer].Count == 0 && nextPlayer != currentPlayer);
        return nextPlayer == currentPlayer ? -1 : nextPlayer;
    }

    public void SwapCardFromTarget(int currentPlayer, int targetPlayer, int targetCardIndex)
    {
        if (hands[targetPlayer].Count == 0)
        {
            Debug.Log("Target has no cards! Moving to merging.");
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
        Debug.Log($"Player {currentPlayer} swapped a card from Player {targetPlayer}");
        areLeftPlayerCardsDisplayed = false;
        UpdateHandPositions(targetPlayer);
        UpdateHandPositions(currentPlayer);
        navigatedCardindex = hands[currentPlayer].Count - 1;
        gamestate = "merging";
    }

    public void EndTurn()
    {
        selectedCardsindex.Clear();
        do
        {
            cplayer = (cplayer + 1) % numberOfPlayers;
        } while (hands[cplayer].Count == 0 && !IsGameOver());
        navigatedCardindex = hands[cplayer].Count > 0 ? System.Math.Min(navigatedCardindex, hands[cplayer].Count - 1) : 0;
        if (IsGameOver())
        {
            gamestate = "end";
        }
        else
        {
            gamestate = "swapping";
            areLeftPlayerCardsDisplayed = false;
            targetPlayer = GetNextPlayerWithCards(cplayer);
            Debug.Log($"=== Player {cplayer}'s turn ===");
            if (targetPlayer != -1)
                Debug.Log($"Navigate Player {targetPlayer}'s cards with Q/W, select with S");
            else
            {
                Debug.Log("No other players have cards! Moving to merging.");
                gamestate = "merging";
            }
        }
    }

    private bool IsGameOver()
    {
        int playersWithCards = 0;
        for (int i = 0; i < numberOfPlayers; i++)
            if (hands[i].Count > 0) playersWithCards++;
        return playersWithCards <= 1;
    }

    public void SelectCard(int player, int cardIndex)
    {
        if (gamestate != "merging")
        {
            Debug.Log("Can only select during merging!");
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
            Debug.Log($"Card deselected at index: {cardIndex}");
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
            Debug.Log($"Card selected at index: {cardIndex}");
        }
    }

    public void SelectCardFromTargetPlayer(int currentPlayer, int cardIndex)
    {
        targetPlayer = GetNextPlayerWithCards(currentPlayer);
        if (targetPlayer == -1)
        {
            Debug.Log("No other players have cards! Moving to merging.");
            gamestate = "merging";
            areLeftPlayerCardsDisplayed = false;
            return;
        }
        if (cardIndex >= hands[targetPlayer].Count)
            cardIndex = 0;
        SwapCardFromTarget(currentPlayer, targetPlayer, cardIndex);
    }

    public void AttemptMerge(int player)
    {
        if (gamestate != "merging")
        {
            Debug.Log("Can only merge during merging!");
            return;
        }
        if (selectedCardsindex.Count != 2)
        {
            Debug.Log("Must select exactly two cards!");
            return;
        }
        int firstCardIndex = selectedCardsindex[0];
        int secondCardIndex = selectedCardsindex[1];
        if (firstCardIndex >= hands[player].Count || secondCardIndex >= hands[player].Count)
        {
            Debug.Log("Invalid card indices!");
            ClearSelection(player);
            return;
        }
        string firstCardValue = GetCardValue(hands[player][firstCardIndex]);
        string secondCardValue = GetCardValue(hands[player][secondCardIndex]);
        Debug.Log($"Comparing cards: '{firstCardValue}' vs '{secondCardValue}'");
        if (firstCardValue == secondCardValue && firstCardValue != "King")
        {
            Debug.Log($"SUCCESS: Merging cards with value: {firstCardValue}");
            MergeCards(player, firstCardIndex, secondCardIndex);
        }
        else
        {
            Debug.Log($"FAILED: Cards cannot be merged. First: '{firstCardValue}', Second: '{secondCardValue}'");
            if (firstCardValue == "King" || secondCardValue == "King")
                Debug.Log("Reason: King cards cannot be merged");
            else if (firstCardValue != secondCardValue)
                Debug.Log("Reason: Different card values");
            ClearSelection(player);
        }
    }

    private void MergeCards(int player, int firstIndex, int secondIndex)
    {
        List<int> indices = new List<int> { firstIndex, secondIndex };
        indices.Sort((a, b) => b.CompareTo(a));
        foreach (int index in indices)
            throwCard(player, index);
        selectedCardsindex.Clear();
        UpdateHandPositions(player);
        navigatedCardindex = hands[player].Count > 0 ? System.Math.Min(navigatedCardindex, hands[player].Count - 1) : 0;
        Debug.Log($"Cards merged successfully. Player {player} now has {hands[player].Count} cards.");
        CheckWinCondition(player);
    }

    private void ClearSelection(int player)
    {
        foreach (int index in selectedCardsindex)
            if (index < hands[player].Count)
                hands[player][index].GetComponent<Renderer>().material.color = Color.white;
        selectedCardsindex.Clear();
    }

    private string GetCardValue(GameObject card)
    {
        string cardName = card.name.Replace("(Clone)", "").Trim();
        Debug.Log($"Extracting value from card: {cardName}");
        if (cardName.StartsWith("Card_"))
        {
            string suitAndValue = cardName.Substring(5);
            string value = "";
            if (suitAndValue.EndsWith("Ace"))
                value = "Ace";
            else if (suitAndValue.EndsWith("King"))
                value = "King";
            else if (suitAndValue.EndsWith("Queen"))
                value = "Queen";
            else if (suitAndValue.EndsWith("Jack"))
                value = "Jack";
            else
            {
                for (int i = 10; i >= 2; i--)
                    if (suitAndValue.EndsWith(i.ToString()))
                    {
                        value = i.ToString();
                        break;
                    }
            }
            if (!string.IsNullOrEmpty(value))
            {
                Debug.Log($"Successfully extracted value: {value} from {cardName}");
                return value;
            }
        }
        Debug.LogError($"Could not extract card value from: {cardName}. Expected format: Card_[Suit][Value]");
        return cardName;
    }

    private void UpdateHandPositions(int player)
    {
        handspostions[player].Clear();
        handspostions[player].Add(GetBasePosition(player));
        for (int j = 1; j < hands[player].Count; j++)
            handspostions[player].Add(handspostions[player][j - 1] + cardSpacing[player]);
        for (int j = 0; j < hands[player].Count; j++)
        {
            hands[player][j].transform.localPosition = handspostions[player][j];
            hands[player][j].transform.localRotation = Quaternion.Euler(playerrotations[player]);
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
        if (hands[player].Count == 0)
            Debug.Log($"Player {player} has no cards left!");
        else if (hands[player].Count == 1 && GetCardValue(hands[player][0]) == "King")
        {
            Debug.Log($"Player {player} loses! They have the Old Maid (King)!");
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
            Debug.Log($"Game Over! Player {lastPlayerWithCards} loses with the Old Maid!");
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
}