using UnityEngine;

namespace GameSystem
{
    public class UnoGameManager:GameManager
    {
        Uno UnoGame; 
        public void Start()
        {
            UnoGame = new Uno();
            UnoGame.gamestate = "playing";
             
        }
        public void Update()
        {
            if(UnoGame.gamestate!="end")
            {
                StartCoroutine(UnoGame.navigatedCards(currentTurn));
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    UnoGame.throwCard(currentTurn,UnoGame.navigatedCardindex);
                    Debug.Log(UnoGame.Playable);
                    if(UnoGame.Playable)
                    {
                    UnoGame.navigatedCardindex = 0;
                    currentTurn = NextTurn(UnoGame.numberOfPlayers);
                    if(UnoGame.hands[currentTurn].Count==0)
                    {
                        UnoGame.gamestate = "end";
                    }
                    UnoGame.Playable = false;
                    }
                    
                }
                else if(Input.GetKeyDown(KeyCode.Alpha1))
                {
                    UnoGame.PickCard(currentTurn);
                }
            }
           
        }
        public override int NextTurn(int noOfPlayers)
        {
            if (UnoGame.reverse)
            {
                currentTurn = (currentTurn - 1 + noOfPlayers) % noOfPlayers;
                return currentTurn;
            }
            else if (UnoGame.skip)
            {
                currentTurn = (currentTurn + 2) % noOfPlayers;
                UnoGame.skip = false;
                return currentTurn;
            }
            else
            {
                return (currentTurn + 1) % noOfPlayers;
            }
        }
    }
}