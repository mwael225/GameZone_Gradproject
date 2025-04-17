using UnityEngine;

namespace GameSystem
{
    public class GameManager:MonoBehaviour
    {
        protected int currentTurn;

        public virtual void StartGame()
        {

        }

        public virtual void EndGame()
        {
            
        }
        public virtual void playerturn(int player)
        {

        }

        public virtual int NextTurn(int noOfPlayers)
        {
            return (currentTurn + 1) % noOfPlayers;
        }
        public virtual int firstplayer(){ return 0;}

    }
} 