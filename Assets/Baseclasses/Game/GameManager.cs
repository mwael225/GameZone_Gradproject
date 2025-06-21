using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;
using System.Collections;
using System.IO;
using TMPro;
namespace GameSystem
{
    public class GameManager : NetworkBehaviour
    {

        public string playername;
        public List<string> playernames=new List<string>();
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
        public virtual void killGame()
        {
            // Despawn and destroy
        }
        public void getnames()
        {
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string gameZonePath = Path.Combine(documentsPath, "GameZone");
            string playerDataPath = Path.Combine(gameZonePath, "playerdata");
            string content = File.ReadAllText(playerDataPath);
            playername = content;
            Debug.Log(playername+" " + NetworkManager.Singleton.LocalClientId);

        }
        [ServerRpc]
        public void sendnamesServerRpc(string playername)
        {
            putname(playername);
        }
        public void putname(string name)
        {
            Debug.Log("IsServer: " + IsServer);
            playernames.Add(name);    
        }
        
        [ClientRpc]
        public void setnamesClientRpc()
        {
            Debug.Log("Client hello world "+playernames.Count);
            Debug.Log(playernames.Count);
            for (int i = 0; i < playernames.Count; i++)
            {
                Debug.Log("Client hello world "+playernames.Count);
                GameObject.Find("astraplayable" + (i + 1) + "(Clone)").transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = playernames[i];
            }
        }


    }
} 