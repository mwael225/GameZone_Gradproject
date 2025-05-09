using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Collections;
using UnityEngine.Playables;
public class Uno: CardGame
{
    string prefabpath = "Prefabs/Card_Deck-Matchup";
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
    }
    public override void throwCard(int player, int cardIndex, string place = "last")
    {
        if(hands[player][navigatedCardindex].name.Split("_")[1] == centralPile.Last.Value.name.Split("_")[1]||hands[player][navigatedCardindex].name.Split("_")[2] == centralPile.Last.Value.name.Split("_")[2])
        {
        Playable=true;
        base.throwCard(player, cardIndex, place);
        }
        else
        {
            Debug.Log("that card is not playable");
        }
    }
    public override GameObject PickCard(int player)
    {
            if (deck.Count == 0)
            {
                Debug.LogWarning("Deck is empty!");
            }

            GameObject card = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            hands[player].Add(card);
            card.transform.localPosition = handspostions[player][hands[player].Count-1];
            card.transform.localRotation = Quaternion.Euler(playerrotations[player]);
            return card;
    }
    public void plusTwo(int player)
    {
        player +=1 % numberOfPlayers;
        for (int i = 0; i < 2; i++)
        {
            PickCard(player);
        }
    }
    public void plus4(int player)
    {
        player +=1 % numberOfPlayers;
        for (int i = 0; i < 4; i++)
        {
            PickCard(player);
        }
    }

    


    


}