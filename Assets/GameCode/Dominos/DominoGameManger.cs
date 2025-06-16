using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
namespace GameSystem
{
    public class DominosGameManager:GameManager
    {
        Dominos dominos;
        int framecount=0;
        //enum Gamestate gamestate ={navitgaing,choosing}
        InputHandler inputHandler;
        int[] scores={0,0,0,0};
        List<KeyCode> keyCodes = new List<KeyCode>
        {
            KeyCode.P,
            KeyCode.Return,
            KeyCode.Q,
            KeyCode.W,
            KeyCode.Alpha1,
            KeyCode.Alpha2
        };
        public void Start()
        {
            if (IsServer)
            {
                inputHandler = GetComponent<InputHandler>();
                inputHandler.Keymap(keyCodes);
                dominos = new Dominos(inputHandler);
                currentTurn = firstplayer();
                Debug.Log("Current turn: " + currentTurn);
            }

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

            if(!IsServer)return;
            framecount++;
            if(framecount==60)
            {
                framecount=0;
                Debug.Log("Game state: " + dominos.gamestate);
            }
            if(dominos.gamestate!="end")
            {
                if(dominos.gamestate=="navigating")
                {
                    StartCoroutine(dominos.navigatedCards(currentTurn));
                }
                if (inputHandler.GetKeyDown(KeyCode.Return,currentTurn)||dominos.gamestate=="choosing")
                {
                    if(dominos.discardpile.Count!=0)
                    StartCoroutine(dominos.firstorlast(currentTurn));
                    if(dominos.gamestate=="navigating")
                    {
                    int x =dominos.discardpile.Count;
                    Debug.Log("Central pile count: " + x);
                    dominos.throwCard(currentTurn,0,dominos.where);
                        if(x<dominos.discardpile.Count)
                        {
                            if(dominos.gamestate!="end")
                            {
                            Debug.Log("final:"+dominos.gamestate);
                            currentTurn=NextTurn(dominos.numberOfPlayers);
                            dominos.navigatedCardindex=0;
                            dominos.gamestate="navigating";
                            }

                        }  

                    }

                }
                else if (inputHandler.GetKeyDown(KeyCode.P,currentTurn))
                {
                    Debug.Log("pass");
                    currentTurn=NextTurn(dominos.numberOfPlayers);
                }

            }
            else
            {
                Debug.Log("End game");
                endGame();
            }

        }
        public override void endGame()
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