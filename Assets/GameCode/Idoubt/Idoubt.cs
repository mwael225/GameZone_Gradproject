using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System;
using Unity.Netcode;
public class Idoubt :CardGame
{

    public string claim ="King";
    public List<GameObject> prev_play ;
    GameObject origin;//it is an origin point for local position of cards/ local rotation of cards
    
    public Idoubt(InputHandler inputHandler) : base("Idoubt", 4, inputHandler)
    {
        cardtransformations.discardpileRotation = new Vector3(180, 0, 0);
        cardtransformations.oldscale = new Vector3(7f, 7f, 7f);
        origin = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs_N/Card_Deck_N"));
        origin.GetComponent<NetworkObject>().Spawn();
        GameObjects=spawnobjects("Card_Deck");
        holder = new List<GameObject>();
        for (int i = 0; i < GameObjects.Count; i++)
        {
            holder.Add(GameObjects[i]);
            GameObjects[i].transform.SetParent(origin.transform);
        }
        shuffledeck(GameObjects);
        DealCards();
        callsetupposition();
        cardtransformations.MovetoPostion(hands);
        selectedCardsindex = new List<int>();
        cardtransformations.centralpileLocalpos = new List<Vector3> { new Vector3(0, 0, 0) };
        cardtransformations.discard_pileSpacing = new List<Vector3> { new Vector3(0, 0, 0.005f) };

    }
    public override void callsetupposition()
    {
        cardtransformations.cardSpacing = new List<Vector3>()
            {
                new Vector3(-0.034975f, -0.001f, 0)*6,
                new Vector3(0.001f, -0.034975f, 0)*6,
                new Vector3(-0.034975f, -0.001f, 0)*6,
                new Vector3(0.001f, -0.034975f, 0)*6,
            };
        cardtransformations.playerRotations = new List<Vector3>
                {
                    new Vector3(90, 0, 0), new Vector3(0, -90, -90), new Vector3(-90,0 ,180), new Vector3(0,90, 90)
                };

        cardtransformations.handspostions = new List<List<Vector3>>()
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
                    cardtransformations.handspostions[i].Add(cardtransformations.handspostions[i][cardtransformations.handspostions[i].Count-1]+ cardtransformations.cardSpacing[i]);
                }
            }
    }
    public void SelectCard(int index,int player)
    {
        if (selectedCardsindex.Contains(index))
        {
                selectedCardsindex.Remove(index);
                cardtransformations.scalecard(hands[player][index],false);
                hands[player][index].GetComponent<Renderer>().material.color=Color.white;
        }
        else    
        {       if(selectedCardsindex.Count<4)
                {
                selectedCardsindex.Add(index);
                cardtransformations.scalecard(hands[player][index],true);
                hands[player][index].GetComponent<Renderer>().material.color = Color.yellow;
                }
                else
                {
                    DebugLog2("You can't select more than 4 cards");
                }
        }
    }
        
    public void throwCards(int player,List<int> cardsindex)
    {
        prev_play = new List<GameObject>();
        for(int i=0;i<cardsindex.Count;i++)
        {
            prev_play.Add(hands[player][cardsindex[i]]);
        }
        for(int i=0;i<prev_play.Count;i++)
        {
            cardtransformations.scalecard(prev_play[i],false);
            prev_play[i].GetComponent<Renderer>().material.color=Color.white;
            cardtransformations.discardpileRotation += new Vector3(0,0,cardtransformations.Zangles());
            throwCard(player,hands[player].IndexOf(prev_play[i]));
        }
        if(hands[player].Count==0)
        {
            gamestate = "end";
        }
        cardPositionReset();
    }
    public bool doubt(int player)
    {
        bool islying = false;
        for(int i=0;i<prev_play.Count;i++)
        {
            if(prev_play[i].name.Split('-')[1]!=claim)
            {
                islying = true;
                piletohand((player-1+numberOfPlayers)%numberOfPlayers);

            }
            else
            {
                islying = false;
                piletohand(player);
                DebugLog2("failed");  
            }
        }
        gamestate="claiming";
        return islying;
    }
    public void cardPositionReset()
    {
        cardtransformations.MovetoPostion(hands);
        /*for(int i=0;i<hands.Count;i++)
        {
            for(int j=0;j<hands[i].Count;j++)
            {
                hands[i][j].transform.localPosition = handspostions[i][j];
            }
        }*/
    }
    public void piletohand(int player)
    {
        while(discardpile.Count>0)
        {
            try
            {
                hands[player].Add(discardpile.Last.Value);
                discardpile.RemoveLast();
                cardtransformations.moveandrotate(hands,player,hands[player].Count-1,cardtransformations.handspostions,cardtransformations.playerRotations);
            }
            catch(ArgumentOutOfRangeException e)
            {
                DebugLog2("no place for cards ..... creating place"+e.Message);
                for(int i=0;i<5;i++)
                {
                    cardtransformations.handspostions[player].Add(cardtransformations.handspostions[player][cardtransformations.handspostions[player].Count-1]+cardtransformations.cardSpacing[player]);
                }
                cardtransformations.moveandrotate(hands,player,hands[player].Count-1,cardtransformations.handspostions,cardtransformations.playerRotations);
            }
        }
    }
}