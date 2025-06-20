using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Collections;
using UnityEngine.Playables;
public class Uno: CardGame
{
    string prefabpath = "Prefabs/#Card_Deck";
    public bool Playable,reverse,skip = false;
    
    public Uno():base("Uno",4)
    {
        centralpileLocalpos = new List<Vector3>();
        discardpileRotation = new Vector3(180,180,0);
        oldscale = new Vector3(7f, 7f, 7f);
        GameObjects=prefabtoGamebojects(prefabpath);
        shuffledeck(GameObjects);
        DealCards(7);
        Assemble(deck);
        setupposition();
        centralpileLocalpos = new List<Vector3> {new Vector3(-1, 0, 0)};
        discard_pilespcaing= new List<Vector3> {new Vector3(0, 0, 0.005f)};
        centralpileLocalpos[0]+=discard_pilespcaing[0];
        MovetoPostion();
    }
    public override void setupposition()
    {
        cardSpacing = new List<Vector3>()
            {
                new Vector3(-0.034975f, -0.001f, 0)*6,
                new Vector3(0.001f, -0.034975f, 0)*6,
                new Vector3(-0.034975f, -0.001f, 0)*6,
                new Vector3(0.001f, -0.034975f, 0)*6,
            };
        playerrotations = new List<Vector3>
                {
                    new Vector3(90, 0, 0), new Vector3(0, -90, -90), new Vector3(-90,0 ,180), new Vector3(0,90, 90)
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
                for (int j = 0; j < hands[i].Count+5; j++)
                {
                    handspostions[i].Add(handspostions[i][handspostions[i].Count-1]+ cardSpacing[i]);
                }
            }
    }
    public override void DealCards(int numberofcards)
    {
        base.DealCards(numberofcards);
        centralPile.AddLast(deck[deck.Count - 1]);
        deck.RemoveAt(deck.Count - 1);
    }
    protected override void MovetoPostion() 
    {
        base.MovetoPostion();
        centralPile.First.Value.transform.localPosition = centralpileLocalpos[0];
        centralPile.First.Value.transform.Rotate(0, 180, 0);
    }    public override void throwCard(int player, int cardIndex, string place = "last")
    {
        // Get the card name parts for both player's card and top card
        string[] playerCardParts = hands[player][navigatedCardindex].name.Split("_");
        string[] topCardParts = centralPile.Last.Value.name.Split("_");
        
        // Check if the suit (Hearts, Clubs, etc.) or value (Ace, 2-10, Jack, etc.) matches
        if(playerCardParts[1] == topCardParts[1] || playerCardParts[2] == topCardParts[2])
        {
            Playable = true;
            
            // Store the card value before playing it
            string cardValue = playerCardParts[2];
            
            // Play the card (adds to central pile and removes from hand)
            base.throwCard(player, cardIndex, place);
            
            // Apply special effects based on card value
            if (cardValue == "Ace")
            {
                // Reverse direction - toggle the reverse flag
                reverse = !reverse;
                Debug.Log("Direction reversed!");
            }
            else if (cardValue == "Queen")
            {
                // Skip next player
                skip = true;
                Debug.Log("Next player will be skipped!");
            }
            else if (cardValue == "Jack")
            {
                // Next player draws 2 cards
                plusTwo(player);
                Debug.Log("Next player must draw 2 cards!");
            }
            else if (cardValue == "King")
            {
                // Next player draws 4 cards
                plus4(player);
                Debug.Log("Next player must draw 4 cards!");
            }
        }
        else
        {
            Debug.Log("That card is not playable");
        }
    }    public override GameObject PickCard(int player)
    {
        // If the deck is empty, recycle cards from discard pile
        if (deck.Count == 0)
        {
            // Attempt to recycle
            if (!RecycleDeck())
            {
                // If recycling failed, return null to indicate no card could be drawn
                Debug.LogWarning("Deck is empty and cannot be recycled. No more cards can be drawn.");
                return null;
            }
        }

        GameObject card = deck[deck.Count - 1];
        deck.RemoveAt(deck.Count - 1);
        hands[player].Add(card);
        card.transform.localPosition = handspostions[player][hands[player].Count-1];
        card.transform.localRotation = Quaternion.Euler(playerrotations[player]);
        return card;
    }public void plusTwo(int player)
    {
        // Get the next player considering the direction
        int nextPlayer;
        if (reverse) {
            nextPlayer = (player - 1 + numberOfPlayers) % numberOfPlayers;
        } else {
            nextPlayer = (player + 1) % numberOfPlayers;
        }
        
        for (int i = 0; i < 2; i++)
        {
            PickCard(nextPlayer);
        }
    }
    public void plus4(int player)
    {
        // Get the next player considering the direction
        int nextPlayer;
        if (reverse) {
            nextPlayer = (player - 1 + numberOfPlayers) % numberOfPlayers;
        } else {
            nextPlayer = (player + 1) % numberOfPlayers;
        }
        
        for (int i = 0; i < 4; i++)
        {
            PickCard(nextPlayer);
        }
    }    // New method to recycle cards from the discard pile and shuffle them
    // Returns true if recycling succeeded, false otherwise
    private bool RecycleDeck()
    {
        Debug.Log("Attempting to recycle discard pile...");
        
        // We need at least 2 cards in the central pile (one to keep as the top card, at least one to recycle)
        if (centralPile.Count <= 1)
        {
            Debug.LogWarning("Not enough cards in discard pile to recycle!");
            return false;
        }
        
        // Keep track of the top card
        GameObject topCard = centralPile.Last.Value;
        
        // Create a list to hold all cards except the top one
        List<GameObject> cardsToRecycle = new List<GameObject>();
        
        // Remove the top card first
        centralPile.RemoveLast();
        
        // Get all remaining cards from the central pile
        while (centralPile.Count > 0)
        {
            // Take the first card each time (more efficient for LinkedList)
            GameObject card = centralPile.First.Value;
            centralPile.RemoveFirst();
            cardsToRecycle.Add(card);
        }
        
        // At this point centralPile should be empty
        Debug.Log("Recycling " + cardsToRecycle.Count + " cards from discard pile.");
        
        // Add the top card back to the central pile
        centralPile.AddLast(topCard);
        
        // Reset the position tracker for discard pile to initial value
        centralpileLocalpos[0] = new Vector3(-1, 0, 0);
        
        // Reset position of the top card to original table position
        topCard.transform.localPosition = centralpileLocalpos[0];
        topCard.transform.localRotation = Quaternion.Euler(discardpileRotation);
        
        // Update the position tracker
        centralpileLocalpos[0] += discard_pilespcaing[0];
          // Shuffle the cards into the deck
        shuffledeck(cardsToRecycle);
        
        // Make sure cards are stacked properly in the deck (face down)
        // This will arrange them with proper spacing and rotation using our overridden Assemble method
        Assemble(deck);
        
        Debug.Log("Successfully recycled " + deck.Count + " cards back into the deck.");
        return true;
    }
    protected override void Assemble(List<GameObject> cards)
    {
        // Position deck cards properly on the table
        float stackOffset = 0f;
        for (int i = 0; i < cards.Count; i++)
        {
            // Position cards horizontally on the table with slight offset for the stack effect
            cards[i].transform.localPosition = new Vector3(0.5f, 0, stackOffset);
            
            // Rotate cards to face down position (laying flat on table, back facing up)
            cards[i].transform.localRotation = Quaternion.Euler(new Vector3(180, 0, 0));
            
            // Add a small vertical offset for each card
            stackOffset += 0.005f;
        }
    }
}