using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem
{
    public class DominosGameManager:GameManager
    {
        Dominos dominos;
        //enum Gamestate gamestate ={navitgaing,choosing}
        int[] scores={0,0,0,0};
        public void Start() 
        {
            dominos = new Dominos();
            currentTurn=firstplayer();
            Debug.Log("Current turn: " + currentTurn);
        }
        public override int firstplayer()
        {
            for(int i=0;i<dominos.hands.Count;i++)
            {
                for(int j=0;j<dominos.hands[i].Count;j++)
                {
                    if(dominos.hands[i][j].name=="Domino_6-6")
                    {
                        Debug.Log("Player " + i + " has 6-6");
                        return i;
                    }
                }
            }
            Debug.Log("No player has 6-6");
            return -1;
        }
        public void Update()
        {
            Debug.Log("Game state: " + dominos.gamestate);
            if(dominos.gamestate!="end")
            {
                Debug.Log("convert to navigating");
                if(dominos.gamestate=="navigating")
                {
                StartCoroutine(dominos.navigatedCards(currentTurn));
                }
                if (Input.GetKeyDown(KeyCode.Return)||dominos.gamestate=="choosing")
                {
                    if(dominos.centralPile.Count!=0)
                    StartCoroutine(dominos.firstorlast());

                    if(dominos.gamestate=="navigating")
                    {
                    int x =dominos.centralPile.Count;
                    Debug.Log("Central pile count: " + x);
                    dominos.throwCard(currentTurn,0,dominos.where);
                    Debug.Log("gamestate2:"+dominos.gamestate);
                        if(x<dominos.centralPile.Count)
                        {
                            if(dominos.gamestate!="end")
                            {
                            Debug.Log("final:"+dominos.gamestate);
                            currentTurn=NextTurn(dominos.numberOfPlayers);
                            dominos.navigatedCardindex=0;
                            dominos.gamestate="navigating";
                            }
                            Debug.Log("a:"+dominos.gamestate);
                        }  
                        Debug.Log("b:"+dominos.gamestate);
                    }
                    Debug.Log("c:"+dominos.gamestate);
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("pass");
                    currentTurn=NextTurn(dominos.numberOfPlayers);
                }
                Debug.Log("d:"+dominos.gamestate);
            }
            else
            {
                Debug.Log("End game");
                EndGame();
            }
            Debug.Log("e:"+dominos.gamestate);
        }
        public override void EndGame()
        {
            Debug.Log("Game Over");
            for(int i=0;i<dominos.hands.Count;i++)
            {
                while(dominos.hands[i].Count!=0)
                {
                    string[] max=dominos.hands[i][0].name.Split('-');
                    int sum = int.Parse(max[0][max[0].Length-1].ToString())+int.Parse(max[1]);
                    scores[i]+=sum;
                    dominos.hands[i].RemoveAt(0);
                }
            }
            Debug.Log("Scores: " + scores[0] + " " + scores[1] + " " + scores[2] + " " + scores[3]);
        }
    }
}