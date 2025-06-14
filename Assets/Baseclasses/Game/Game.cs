using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

namespace GameSystem
{
    public class Game
    {
        protected string Gamename;
        public int numberOfPlayers,frame;

        //protected DB_Manager db_manager;

        public InputHandler inputHandler;
        protected List<GameObject> GameObjects;

        public Game(string name, int numberOfPlayers, InputHandler inputHandler)
        {
            Gamename = name;
            this.numberOfPlayers = numberOfPlayers;
            this.inputHandler = inputHandler;
            //db_manager = new DB_Manager();
        }
        public List<GameObject> spawnobjects(string path)
        {
            List<GameObject> allObjectsInPrefab = new List<GameObject>();
            GameObject[] tilePrefabs = Resources.LoadAll<GameObject>(path);
            foreach (GameObject prefab in tilePrefabs)
            {
                GameObject instance = NetworkBehaviour.Instantiate(prefab);
                instance.GetComponent<NetworkObject>().Spawn();
                string[] name = prefab.name.Split('(');
                instance.name = name[0];
                allObjectsInPrefab.Add(instance);
            }
            return allObjectsInPrefab;
        }
        public void DebugLog2(string message)
        {
            frame++;
            if (frame % 120 == 0)
            {
                Debug.Log(message);
            }
        }
         
    }
} 