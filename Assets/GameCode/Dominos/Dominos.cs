using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Collections;
using Unity.Netcode;
public class Dominos:CardGame
{
    float[] coordinates = { -0.405f, -0.24f, -0.405f, -0.24f };
    GameObject origin,table;
    public string where="none";
    public Dominos(InputHandler inputHandler):base("Dominos",4,inputHandler)
    {
        gamestate="navigating";
        cardtransformations.centralpileLocalpos= new List<Vector3>();
        cardtransformations.discard_pileSpacing.Add(new Vector3(0.7f/5,0,0));
        cardtransformations.discard_pileSpacing.Add(new Vector3(0.7f/5,0,0));
        cardtransformations.centralpileLocalpos.Add(new Vector3(0,0,0));
        cardtransformations.centralpileLocalpos.Add(new Vector3(0,0,0));
        cardtransformations.discardpileRotation = new Vector3 (-90,0,0);
        cardtransformations.playerRotations = new List<Vector3> 
        {
            new Vector3(0, 0, 0), new Vector3(0, 90, 0), new Vector3(0, 180, 0), new Vector3(0, -90, 0)
        };
        cardtransformations.handspostions= new() 
        {   
        new List<Vector3> {new Vector3(-0.405f, 0, 0.571f) },
        new List<Vector3> {new Vector3(0.5f, 0, -0.24f) },
        new List<Vector3> {new Vector3(-0.405f, 0, -0.571f) },
        new List<Vector3> {new Vector3(-0.5f, 0, -0.24f)},
        };
        cardtransformations.oldscale=new Vector3(1f,1f,1f);
        table = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs_N/DominoBoard_N"));
        table.GetComponent<NetworkObject>().Spawn();
        origin = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs_N/DominoTable_N"));
        origin.GetComponent<NetworkObject>().Spawn();
        GameObjects=spawnobjects("Dominos");
        holder=new List<GameObject>();
        //List<GameObject>notused =prefabtoGamebojects("Prefabs/DominoBoard");
        for (int i = 0; i < GameObjects.Count; i++)
        {
            holder.Add(GameObjects[i]);
            GameObjects[i].transform.SetParent(origin.transform);
        }
        shuffledeck(GameObjects);
        DealCards();
        callsetupposition();
        cardtransformations.MovetoPostion(hands);
    }

    public override void callsetupposition()
    {
    for (int i = 0; i < hands.Count; i++)
        {
            for (int j = 0; j < hands[i].Count; j++)
            {
                
                
                coordinates[i] += 0.1f;
                if (i == 0)
                {
                    cardtransformations.handspostions[i].Add(new Vector3(coordinates[i], 0, 0.571f)); 
                }
                else if (i == 1)
                {
                    cardtransformations.handspostions[i].Add(new Vector3(0.5f, 0, coordinates[i]));
                }
                else if (i == 2)
                {
                    cardtransformations.handspostions[i].Add(new Vector3(coordinates[i], 0, -0.571f));
                }
                else
                {
                    cardtransformations.handspostions[i].Add(new Vector3(-0.5f, 0, coordinates[i]));
                }
            }

        }
    }
    public IEnumerator firstorlast(int player)
    {
        Debug.Log("first or last : "+gamestate);
        int click=0;
        gamestate="choosing";
        Debug.Log("choosing");
        while(click==0)
        {
            if(gamestate=="end")
            {
                yield break;
            }
            if(inputHandler.GetKeyDown(KeyCode.Alpha1,player))
            {
                where="first"; 
                gamestate="navigating";
                yield break;
            }
            else if(inputHandler.GetKeyDown(KeyCode.Alpha2,player))
            {
                where="last";
                gamestate="navigating";
                yield break;
            }
            yield return null;
        }
    }
        public override void throwCard(int player,int cardIndex=0,string place="last")
        {
            cardIndex=navigatedCardindex;
            if(discardpile.Count==0)
            {
                if (hands[player][cardIndex].name!="Domino_6-6")
                {
                    Debug.Log("play 6-6");
                    return;
                }
                else
                { 
                    base.throwCard(player, cardIndex);
                }
            }
            else
            {
                Debug.Log("testing if playable");
                if(isplayable(player,place,cardIndex)) 
                base.throwCard(player, cardIndex,place);
                if(hands[player].Count==0)
                {
                Debug.Log("end: "+hands[player].Count);
                gamestate="end";
                Debug.Log("Game state: " + gamestate+"expected end");
                }
            }
        }
        public bool isplayable(int player,string place,int cardIndex)
        {
            string []dominonumbers=hands[player][cardIndex].name.Split('_');
            GameObject card = hands[player][cardIndex];
            string []CentralPileNumber;
            if (place=="first")
            {
                CentralPileNumber= discardpile.First.Value.name.Split('_');
                if(dominonumbers[1][0]==CentralPileNumber[1][2]||dominonumbers[1][2]==CentralPileNumber[1][2])
                {
                    Debug.Log(dominonumbers[1][0]+"=="+CentralPileNumber[1][2]);
                    Debug.Log(dominonumbers[1][2]+"=="+CentralPileNumber[1][2]);
                    char otherNumber = (dominonumbers[1][0] == CentralPileNumber[1][2]) ? dominonumbers[1][2] : dominonumbers[1][0];
                    Debug.Log("otherNumber: "+otherNumber);
                    card.name= "Domino_" + CentralPileNumber[1][2]+"-"+otherNumber;
                    Debug.Log(card.name);
                    return true;
                }
            }
            else
            {
                CentralPileNumber=discardpile.Last.Value.name.Split('_');
                if(dominonumbers[1][0]==CentralPileNumber[1][2]||dominonumbers[1][2]==CentralPileNumber[1][2])
                {
                    Debug.Log(dominonumbers[1][0]+"=="+CentralPileNumber[1][2]);
                    Debug.Log(dominonumbers[1][2]+"=="+CentralPileNumber[1][2]);
                    char otherNumber = (dominonumbers[1][0] == CentralPileNumber[1][2]) ? dominonumbers[1][2]  : dominonumbers[1][0];
                    card.name= "Domino_" + CentralPileNumber[1][2]+"-"+otherNumber;
                    Debug.Log(card.name);
                    return true;
                }
            }
            Debug.Log("not playable");
            Debug.Log(dominonumbers[1][0]+"!="+CentralPileNumber[1][2]);
            Debug.Log(dominonumbers[1][2]+"!="+CentralPileNumber[1][2]);;
            return false;
    }
}
    

