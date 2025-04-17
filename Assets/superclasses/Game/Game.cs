using UnityEngine;
using Firebase.Database;
using Unity.Mathematics;
using System.Collections.Generic;
namespace GameSystem
{
    public class Game
    {
        protected string name;
        public int numberOfPlayers;
        protected NetworkManager networkManager;
        
        protected DB_Manager db_manager;

        protected List<GameObject> GameObjects;
        

        public Game(string name, int numberOfPlayers)
        {
            this.name = name;
            this.numberOfPlayers = numberOfPlayers;
            networkManager = new NetworkManager();
            db_manager = new DB_Manager();

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

        allObjectsInPrefab.Clear();

        foreach (Transform child in prefabInstance.transform)
        {
            allObjectsInPrefab.Add(child.gameObject);  // Add each child GameObject to the list
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




    }
} 