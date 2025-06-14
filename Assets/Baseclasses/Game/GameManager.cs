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
        Vector3 [] arrowpositions = {new Vector3(2.46f,6.94f,-0.05f),new Vector3(2.18f,6.94f,-0.05f),new Vector3(1.98f,6.94f,-0.15f),new Vector3(2.43f,6.94f,-0.64f)};
        Vector3 [] arrowrotation = {new Vector3(0,230,0),new Vector3(0,138,0),new Vector3(0,43,0),new Vector3(0,-39,0)};

        public GameObject arrow;
        bool firsttime = true;

        public virtual void EndGame()
        {

        }

        public virtual int NextTurn(int noOfPlayers)
        {
            return (currentTurn + 1) % noOfPlayers;
        }
        public virtual int firstplayer() { return 0; }


        public void arrowdirection()
        {
            if(firsttime)
            {
                arrow = Instantiate(arrow);
                firsttime = false;
            }
            arrow.transform.position = arrowpositions[currentTurn];
            arrow.transform.rotation = Quaternion.Euler(arrowrotation[currentTurn]);
        }

    }
} 