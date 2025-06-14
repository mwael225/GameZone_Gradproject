using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using GameSystem;

public class OldMaid_N : CardGame
{
    private string prefabpath = "Prefabs/Card_Deck_Shayeb";
    private List<Vector3> originalCardPositions;
    private List<Quaternion> originalCardRotations;
    public NetworkVariable<bool> areLeftPlayerCardsDisplayed = new NetworkVariable<bool>(false);
    public NetworkVariable<int> navigatedCardIndexTarget = new NetworkVariable<int>(0);

    public OldMaid_N(InputHandler inputHandler) : base("Old Maid", 4, inputHandler)
    {
        oldscale = new Vector3(7f, 7f, 7f);
        GameObjects = spawnobjects(prefabpath);
        foreach (var obj in GameObjects)
        {
            obj.GetComponent<NetworkObject>().Spawn();
        }
        shuffledeck(GameObjects);
        DealCards();
        setupposition();
        MovetoPostion();
        selectedCardsindex = new List<int>();
        centralpileLocalpos = new List<Vector3> { new Vector3(0, 0, 0) };
        discard_pileSpacing = new List<Vector3> { new Vector3(0, 0, 0.005f) };
        gamestate = "swapping";
        cplayer = 0;
        originalCardPositions = new List<Vector3>();
        originalCardRotations = new List<Quaternion>();
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
        playerRotations = new List<Vector3>
        {
            new Vector3(90, 0, 0), new Vector3(0, -90, -90), new Vector3(-90,0 ,180), new Vector3(0,90, 90)
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
            for (int j = 1; j < hands[i].Count; j++)
            {
                handspostions[i].Add(handspostions[i][j - 1] + cardSpacing[i]);
            }
        }
    }

    [ServerRpc]
    public void ShufflePlayerHandServerRpc(int player, ServerRpcParams rpcParams = default)
    {
        if (player != cplayer) return;
        if (hands[player].Count <= 1) return;

        ClearSelectionServerRpc(player);
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
        UpdateHandPositionsServerRpc(player);
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
            if (attempts >= numberOfPlayers) return -1;
        }
        while (hands[nextPlayer].Count == 0 && nextPlayer != currentPlayer);
        return nextPlayer == currentPlayer ? -1 : nextPlayer;
    }

    [ServerRpc]
    public void SwapCardFromTargetServerRpc(int currentPlayer, int targetPlayer, int targetCardIndex, ServerRpcParams rpcParams = default)
    {
        if (hands[targetPlayer].Count == 0)
        {
            gamestate = "merging";
            areLeftPlayerCardsDisplayed.Value = false;
            UpdateGameStateClientRpc(gamestate);
            return;
        }
        if (targetCardIndex < 0 || targetCardIndex >= hands[targetPlayer].Count) return;

        GameObject swappedCard = hands[targetPlayer][targetCardIndex];
        hands[targetPlayer].RemoveAt(targetCardIndex);
        hands[currentPlayer].Add(swappedCard);
        swappedCard.transform.localRotation = Quaternion.Euler(playerRotations[currentPlayer]);

        UpdateHandPositionsServerRpc(targetPlayer);
        UpdateHandPositionsServerRpc(currentPlayer);
        navigatedCardindex = hands[currentPlayer].Count - 1;
        gamestate = "merging";
        UpdateGameStateClientRpc(gamestate);
        Debug.Log($"Player {currentPlayer} swapped a card from Player {targetPlayer}");
    }

    [ServerRpc]
    public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        selectedCardsindex.Clear();
        do
        {
            cplayer = (cplayer + 1) % numberOfPlayers;
        } while (hands[cplayer].Count == 0 && !IsGameOver());

        navigatedCardindex = hands[cplayer].Count > 0 ? Mathf.Min(navigatedCardindex, hands[cplayer].Count - 1) : 0;
        gamestate = IsGameOver() ? "end" : "swapping";
        areLeftPlayerCardsDisplayed.Value = false;
        UpdateGameStateClientRpc(gamestate);
        UpdateCurrentPlayerClientRpc(cplayer);
        Debug.Log($"Player {cplayer}'s turn");
    }

    [ServerRpc]
    public void SelectCardServerRpc(int player, int cardIndex, ServerRpcParams rpcParams = default)
    {
        if (gamestate != "merging") return;
        if (cardIndex < 0 || cardIndex >= hands[player].Count) return;

        if (selectedCardsindex.Contains(cardIndex))
        {
            selectedCardsindex.Remove(cardIndex);
            UpdateCardVisualClientRpc(player, cardIndex, Color.white, oldscale);
        }
        else
        {
            if (selectedCardsindex.Count >= 2) return;
            selectedCardsindex.Add(cardIndex);
            UpdateCardVisualClientRpc(player, cardIndex, Color.yellow, oldscale * 1.2f);
        }
    }

    [ServerRpc]
    public void AttemptMergeServerRpc(int player, ServerRpcParams rpcParams = default)
    {
        if (gamestate != "merging" || selectedCardsindex.Count != 2) return;

        int firstCardIndex = selectedCardsindex[0];
        int secondCardIndex = selectedCardsindex[1];
        if (firstCardIndex >= hands[player].Count || secondCardIndex >= hands[player].Count) return;

        string firstCardValue = GetCardValue(hands[player][firstCardIndex]);
        string secondCardValue = GetCardValue(hands[player][secondCardIndex]);

        if (firstCardValue == secondCardValue && firstCardValue != "King")
        {
            MergeCardsServerRpc(player, firstCardIndex, secondCardIndex);
            CheckWinConditionServerRpc(player);
        }
        else
        {
            ClearSelectionServerRpc(player);
        }
    }

    [ServerRpc]
    public void ClearSelectionServerRpc(int player)
    {
        foreach (int index in selectedCardsindex)
        {
            if (index < hands[player].Count)
            {
                UpdateCardVisualClientRpc(player, index, Color.white, oldscale);
            }
        }
        selectedCardsindex.Clear();
    }

    [ServerRpc]
    public void UpdateHandPositionsServerRpc(int player)
    {
        handspostions[player].Clear();
        handspostions[player].Add(GetBasePosition(player));
        for (int j = 1; j < hands[player].Count; j++)
        {
            handspostions[player].Add(handspostions[player][j - 1] + cardSpacing[player]);
        }
        for (int j = 0; j < hands[player].Count; j++)
        {
            UpdateCardPositionClientRpc(player, j, handspostions[player][j], playerRotations[player]);
        }
    }

    [ClientRpc]
    public void UpdateCardPositionClientRpc(int player, int cardIndex, Vector3 position, Vector3 rotation)
    {
        if (cardIndex < hands[player].Count)
        {
            hands[player][cardIndex].transform.localPosition = position;
            hands[player][cardIndex].transform.localRotation = Quaternion.Euler(rotation);
        }
    }

    [ClientRpc]
    public void UpdateCardVisualClientRpc(int player, int cardIndex, Color color, Vector3 scale)
    {
        if (cardIndex < hands[player].Count)
        {
            hands[player][cardIndex].GetComponent<Renderer>().material.color = color;
            hands[player][cardIndex].transform.localScale = scale;
        }
    }

    [ClientRpc]
    public void UpdateGameStateClientRpc(string newState)
    {
        gamestate = newState;
    }

    [ClientRpc]
    public void UpdateCurrentPlayerClientRpc(int newPlayer)
    {
        cplayer = newPlayer;
    }

    [ServerRpc]
    public void CheckWinConditionServerRpc(int player)
    {
        if (hands[player].Count == 0)
        {
            Debug.Log($"Player {player} has no cards left!");
        }
        else if (hands[player].Count == 1 && GetCardValue(hands[player][0]) == "King")
        {
            gamestate = "end";
            UpdateGameStateClientRpc(gamestate);
            Debug.Log($"Player {player} loses! They have the Old Maid (King)!");
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
        if (playersWithCards <= 1)
        {
            gamestate = "end";
            UpdateGameStateClientRpc(gamestate);
            Debug.Log($"Game Over! Player {lastPlayerWithCards} loses with the Old Maid!");
        }
    }

    private string GetCardValue(GameObject card)
    {
        string cardName = card.name.Replace("(Clone)", "").Trim();
        if (cardName.StartsWith("Card_"))
        {
            string suitAndValue = cardName.Substring(5);
            string value = "";
            if (suitAndValue.EndsWith("Ace")) return "Ace";
            if (suitAndValue.EndsWith("King")) return "King";
            if (suitAndValue.EndsWith("Queen")) return "Queen";
            if (suitAndValue.EndsWith("Jack")) return "Jack";
            for (int i = 10; i >= 2; i--)
            {
                if (suitAndValue.EndsWith(i.ToString()))
                {
                    return i.ToString();
                }
            }
        }
        Debug.LogError($"Could not extract card value from: {cardName}");
        return cardName;
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

    [ServerRpc]
    public void HighlightTargetPlayerCardServerRpc(int targetPlayer, int cardIndex, ServerRpcParams rpcParams = default)
    {
        for (int i = 0; i < hands[targetPlayer].Count; i++)
        {
            UpdateCardVisualClientRpc(targetPlayer, i, Color.white, oldscale);
        }
        if (cardIndex >= 0 && cardIndex < hands[targetPlayer].Count)
        {
            UpdateCardVisualClientRpc(targetPlayer, cardIndex, Color.cyan, oldscale * 1.2f);
        }
    }

    // Added IsGameOver method
    private bool IsGameOver()
    {
        int playersWithCards = 0;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (hands[i].Count > 0) playersWithCards++;
        }
        return playersWithCards <= 1;
    }

    [ServerRpc]
    private void MergeCardsServerRpc(int player, int firstIndex, int secondIndex)
    {
        List<int> indicesToRemove = new List<int> { firstIndex, secondIndex };
        indicesToRemove.Sort((a, b) => b.CompareTo(a));
        foreach (int index in indicesToRemove)
        {
            throwCard(player, index);
        }
        selectedCardsindex.Clear();
        UpdateHandPositionsServerRpc(player);
        navigatedCardindex = hands[player].Count > 0 ? Mathf.Min(navigatedCardindex, hands[player].Count - 1) : 0;
        Debug.Log($"Player {player} merged cards. Cards left: {hands[player].Count}");
    }
}