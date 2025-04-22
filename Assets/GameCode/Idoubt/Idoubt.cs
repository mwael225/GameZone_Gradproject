using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using NUnit.Framework;
using System;
using Unity.Mathematics;
public class Idoubt :CardGame
{

    string prefabpath = "Prefabs/Card_Deck";
    public string claim ="King";
    List<GameObject> cards ;
    
    public Idoubt() : base("Idoubt", 4)
    {
        cards = new List<GameObject>();
        discardpileRotation = new Vector3(180,0,0);
        oldscale = new Vector3(7f, 7f, 7f);
        GameObjects=prefabtoGamebojects(prefabpath);
        shuffledeck(GameObjects);
        DealCards();
        setupposition();
        MovetoPostion();
        selectedCardsindex = new List<int>();
        centralpileLocalpos = new List<Vector3> {new Vector3(0, 0, 0)};
        discard_pilespcaing= new List<Vector3> {new Vector3(0, 0, 0.005f)};

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
                    new Vector3(-90, 0, 0), new Vector3(0, -90, -90), new Vector3(90,0 ,180), new Vector3(0,90, 90)
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
                    handspostions[i].Add(handspostions[i][handspostions[i].Count-1]+ cardSpacing[i]);
                }
            }
    }
    public void SelectCard(int index,int player)
    {
        if (selectedCardsindex.Contains(index))
        {
                selectedCardsindex.Remove(index);
                hands[player][index].transform.localScale=oldscale;
                hands[player][index].GetComponent<Renderer>().material.color=Color.white;
        }
        else    
        {       if(selectedCardsindex.Count<4)
                {
                selectedCardsindex.Add(index);
                hands[player][index].transform.localScale = oldscale*1.2f;
                hands[player][index].GetComponent<Renderer>().material.color = Color.yellow;
                }
                else
                {
                    Debug.Log("You can't select more than 4 cards");
                }
        }
    }
        
    public void throwCards(int player,List<int> cardsindex)
    {
        cards = new List<GameObject>();
        for(int i=0;i<cardsindex.Count;i++)
        {
            cards.Add(hands[player][cardsindex[i]]);
        }
        for(int i=0;i<cards.Count;i++)
        {
            cards[i].transform.localScale = oldscale;
            cards[i].GetComponent<Renderer>().material.color=Color.white;
            discardpileRotation += new Vector3(0,0,Zangles());
            throwCard(player,hands[player].IndexOf(cards[i]));
        }
        if(hands[player].Count==0)
        {
            gamestate = "end";
        }
        cardPositionReset();
    }
    public bool doubt(int player)
    {
        bool isdoubt = false;
        for(int i=0;i<cards.Count;i++)
        {
            if(cards[i].name.Split('-')[1]!=claim)
            {
                isdoubt = true;
                piletohand(player-1%numberOfPlayers);
                Debug.Log("passed");

            }
            else
            {
                isdoubt = false;
                piletohand(player);
                Debug.Log("failed");  
            }
        }
        gamestate="claiming";
        return isdoubt;
    }
    public void cardPositionReset()
    {
        for(int i=0;i<hands.Count;i++)
        {
            for(int j=0;j<hands[i].Count;j++)
            {
                hands[i][j].transform.localPosition = handspostions[i][j];
            }
        }
    }
    public void piletohand(int player)
    {
        while(centralPile.Count>0)
        {
            try
            {
                hands[player].Add(centralPile.Last.Value);
                centralPile.RemoveLast();
                hands[player][hands[player].Count-1].transform.localPosition = handspostions[player][hands[player].Count-1];
                hands[player][hands[player].Count-1].transform.localRotation = hands[player][0].transform.localRotation;
            }
            catch(ArgumentOutOfRangeException e)
            {
                Debug.Log("no place for cards ..... creating place"+e.Message);
                for(int i=0;i<5;i++)
                {
                    handspostions[player].Add(handspostions[player][handspostions[player].Count-1]+cardSpacing[player]);
                }
                hands[player][hands[player].Count-1].transform.localPosition = handspostions[player][hands[player].Count-1];
                hands[player][hands[player].Count-1].transform.localRotation = hands[player][0].transform.localRotation;
            }
        }
    }
}