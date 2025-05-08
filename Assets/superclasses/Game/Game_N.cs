using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

namespace GameSystem
{
    public class Game_N    
    {
        protected string Gamename;
        public int numberOfPlayers;
        
        //protected DB_Manager db_manager;

        public  InputHandler inputHandler;
        protected List<GameObject> GameObjects;

        public Game_N(string name, int numberOfPlayers,InputHandler inputHandler)
        {
            Gamename = name;
            this.numberOfPlayers = numberOfPlayers;
            this.inputHandler = inputHandler;
            //db_manager = new DB_Manager();
        }
        public virtual List<GameObject> prefabtoGamebojects(string path)
        {
        GameObject prefab;  
        List<GameObject> allObjectsInPrefab = new List<GameObject>();
        prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError("Prefab could not be loaded! Ensure it is inside a 'Resources' folder.");
            return null;
        }

        GameObject prefabInstance = GameObject.Instantiate(prefab);
         // Spawn the prefab instance in the network

        allObjectsInPrefab.Clear();

        foreach (Transform child in prefabInstance.transform)
        {
            allObjectsInPrefab.Add(child.gameObject);
              // Add each child GameObject to the list
        }
        return allObjectsInPrefab;  
        }
        public GameObject GetNthItem(int n ,LinkedList<GameObject> list)
        {
            if (n <= 0 || list.Count == 0)
                return null;

            LinkedListNode<GameObject> currentNode = list.First;
            int index = 1; // 1-based index

            // Traverse the list until the nth node
            while (currentNode != null)
            {
                if (index == n)
                {
                    return currentNode.Value;
                }

                currentNode = currentNode.Next;
                index++;
            }

            return null; // If n exceeds the count of elements in the linked list
        }
        public List<GameObject> spawnobjects(string path)
         {
                List<GameObject> allObjectsInPrefab = new List<GameObject>();
                GameObject[] tilePrefabs = Resources.LoadAll<GameObject>(path);
                foreach (GameObject prefab in tilePrefabs)
                {
                    GameObject instance = NetworkBehaviour.Instantiate(prefab);
                    instance.GetComponent<NetworkObject>().Spawn();
                    string [] name = prefab.name.Split('(');
                    instance.name = name[0];
                    allObjectsInPrefab.Add(instance);
                }
                return allObjectsInPrefab;
        }
        
        
    }
} 