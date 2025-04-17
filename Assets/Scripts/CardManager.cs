using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    List<GameObject> cards = new List<GameObject>();
    List<List<GameObject>> playersCards = new List<List<GameObject>>();
    public GameObject cardObject;
    List<Vector3> cardSpacing = new List<Vector3>()
    {
        new Vector3(-0.034975f, 0.001f, 0),
        new Vector3(0.001f, -0.034975f, 0),
        new Vector3(-0.034975f, 0.001f, 0),
        new Vector3(0.001f, -0.034975f, 0),
    };

    List<List<Vector3>> playerPositions = new List<List<Vector3>>()
    {
        new List<Vector3> { new Vector3(0.25f, -0.49f, 0.185f) },
        new List<Vector3> { new Vector3(-0.612f, 0.231f, 0.185f) },
        new List<Vector3> { new Vector3(0.25f, 0.49f, 0.185f) },
        new List<Vector3> { new Vector3(0.612f, 0.231f, 0.185f) }
    };

    List<Quaternion> cardAngles = new List<Quaternion>
    {
        Quaternion.Euler(90f, 0f, 0f),
        Quaternion.Euler(0f, -90f, -90f),
        Quaternion.Euler(-90f, 0f, 0f),
        Quaternion.Euler(180f, -90f, -90f)
    };

    string[] cardValues = new string[] { "Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King" };
    string[] suits = new string[] { "Heart", "Club", "Spade", "Diamond" };

    int currentPlayerIndex = 0;
    int totalPlayers = 4;
    int gemy = 0;
    List<GameObject> centralPile = new List<GameObject>();
    Vector3 pilePosition = new Vector3(0, 0, 0.02f);

    int selectedCardIndex = 0;
    List<GameObject> selectedCards = new List<GameObject>();

    void Start()
    {

        //x.Start();
        InitializeDeck();
        ShuffleCards();
        DealCards();
        SetupCardPositions();
        for (int i = 0; i < playerPositions[0].Count; i++)
        {
            Debug.Log(playerPositions[0][i]);
        }

    }

    void Update()
    {
        //  x.Update();
        HandleCardSelection();
        MoveCardsSmoothly();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowSelectedCards();
            NextTurn();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            doubt();
        }

    }
    void doubt()
    {
        Debug.Log(gemy);
        for (int i = 0; i < gemy; i++)
        {
            String[] tester = centralPile[centralPile.Count - i].name.Split("_");
            Debug.Log(tester);
            /*if(x.selrank==tester[tester.Length-1])
            {
            }*/
        }
    }

    void InitializeDeck()
    {
        string cardName = "";
        for (int i = 0; i < cardValues.Length; i++)
        {
            for (int j = 0; j < suits.Length; j++)
            {
                cardName = "Card_" + suits[j] + cardValues[i];
                GameObject card = GameObject.Find(cardName);
                if (card != null)
                {
                    cards.Add(card);
                }
            }
        }
    }

    void ShuffleCards()
    {
        System.Random rand = new System.Random();
        List<GameObject> shuffledDeck = new List<GameObject>();
        while (cards.Count > 0)
        {
            int randomIndex = rand.Next(cards.Count);
            GameObject selectedCard = cards[randomIndex];
            cards.RemoveAt(randomIndex);
            shuffledDeck.Add(selectedCard);
        }
        cards = shuffledDeck;
    }

    void DealCards()
    {
        playersCards.Clear();
        for (int i = 0; i < totalPlayers; i++)
        {
            playersCards.Add(new List<GameObject>());
        }

        for (int i = 0; i < cards.Count; i++)
        {
            int playerIndex = i % totalPlayers;
            playersCards[playerIndex].Add(cards[i]);
        }
    }

    void SetupCardPositions()
    {
        for (int i = 0; i < playersCards.Count; i++)
        {
            for (int j = 0; j < playersCards[i].Count; j++)
            {
                Vector3 newPos = playerPositions[i][0] + (cardSpacing[i] * (j + 1));
                playerPositions[i].Add(newPos);
                playersCards[i][j].transform.localPosition = newPos;
                playersCards[i][j].transform.localRotation = cardAngles[i];
            }
        }
    }

    void HandleCardSelection()
    {
        foreach (GameObject card in playersCards[currentPlayerIndex])
        {
            if (!selectedCards.Contains(card))
            {
                card.transform.localScale = Vector3.one;
                card.GetComponent<Renderer>().material.color = Color.white;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedCardIndex = (selectedCardIndex - 1 + playersCards[currentPlayerIndex].Count) % playersCards[currentPlayerIndex].Count;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            selectedCardIndex = (selectedCardIndex + 1) % playersCards[currentPlayerIndex].Count;
        }

        GameObject navigatedCard = playersCards[currentPlayerIndex][selectedCardIndex];
        if (!selectedCards.Contains(navigatedCard))
        {
            navigatedCard.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SelectCard(navigatedCard);
        }
    }


    void SelectCard(GameObject card)
    {
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            card.transform.localScale = Vector3.one;
            card.GetComponent<Renderer>().material.color = Color.white;
        }
        else if (selectedCards.Count < 4)
        {
            selectedCards.Add(card);
            card.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            card.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    void ThrowSelectedCards()
    {
        int count = selectedCards.Count;
        foreach (GameObject card in selectedCards)
        {
            centralPile.Add(card);
            card.transform.localPosition = pilePosition;
            card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            playersCards[currentPlayerIndex].Remove(card);
        }
        gemy = selectedCards.Count;
        //   Debug.Log("thrown"+selectedCards.Count+x.selrank());
        selectedCards.Clear();
        selectedCardIndex = 0;
    }

    void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
        Debug.Log("It's Player " + (currentPlayerIndex + 1) + "'s turn!");
    }

    void MoveCardsSmoothly()
    {
        for (int i = 0; i < playersCards.Count; i++)
        {
            for (int j = 0; j < playersCards[i].Count; j++)
            {
                playersCards[i][j].transform.localPosition = playerPositions[i][j];
            }
        }
    }


}