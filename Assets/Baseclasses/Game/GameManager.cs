using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;
using System.Collections;

namespace GameSystem
{
    public class GameManager : NetworkBehaviour
    {
        protected int currentTurn;
        public Material on;
        public Material off;
        public List<GameObject> turns;

        public virtual void endGame()
        {

        }

        public virtual int NextTurn(int noOfPlayers)
        {
            return (currentTurn + 1) % noOfPlayers;
        }
        public virtual int firstplayer() { return 0; }


        public void turnvisual()
        {
            turns[currentTurn].GetComponent<MeshRenderer>().material = on;
            for (int i = 0; i < turns.Count; i++)
            {
                if (i != currentTurn)
                {
                    turns[i].GetComponent<MeshRenderer>().material = off;
                }
            }
        }
        public void killGame()
        {
        }

    }
} 