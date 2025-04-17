using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class Drawcards : MonoBehaviour
{
    public string dominoCards ="";
    List<GameObject> domino = new();
    int couint = 0;

   public List<List<GameObject>> listarr = new()
    {
    new List<GameObject> { null, null, null, null,null,null,null},
    new List<GameObject> { null, null, null, null,null,null,null},
    new List<GameObject> { null, null, null, null,null,null,null},
    new List<GameObject> { null, null, null, null,null,null,null},
    };
    
    int[] angle = { 0, 90, 180, 270 };
    float[] coordinates = { -0.405f, -0.24f, -0.405f, -0.24f };

   public List<List<Vector3>> listpos= new() 
    {
    new List<Vector3> {new Vector3(-0.405f, 0, 0.571f) },
    new List<Vector3> {new Vector3(0.5f, 0, -0.24f) },
    new List<Vector3> {new Vector3(-0.405f, 0, -0.571f) },
    new List<Vector3> {new Vector3(-0.5f, 0, 0.24f) },
    };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void Start()
    {




        ////////////////////////////////////////////////////////

        for (int i = 0; i <= 6; i++)
        {
            for (int j = i; j <= 6; j++)
            {
                // Format each domino card as Domino_"i|j"
                 dominoCards=($"Domino_{i}-{j}");
               // Debug.Log(dominoCards);
                domino.Add(GameObject.Find(dominoCards));

            }
        }
        
        ///////////////////////////////////////////////////////
        
        // Generate a random hand of 7 domino cards
        System.Random random = new System.Random();

        for (int i = 0;i <listarr.Count; i++)
        {
            for(int j =0; j < listarr[i].Count; j++)
            {
                int index = domino.Count; // Get a random index
                int randomindex = random.Next(0, index);
                GameObject iteam = domino[randomindex];
                domino.RemoveAt(randomindex);
                listarr[i][j] = iteam;
            }
        }


        
        for (int i = 0; i < listarr.Count; i++)
        {
            for (int j = 0; j < listarr[i].Count; j++)
            {
                
                listarr[i][j].transform.Rotate(0, angle[i], 0);
                
                coordinates[i] += 0.1f;
                if (i == 0)
                {
                    listpos[i].Add(new Vector3(coordinates[i], 0, 0.571f)); 
                }
                else if (i == 1)
                {
                    listpos[i].Add(new Vector3(0.5f, 0, coordinates[i]));
                }
                else if (i == 2)
                {
                    listpos[i].Add(new Vector3(coordinates[i], 0, -0.571f));
                }
                else
                {
                    listpos[i].Add(new Vector3(-0.5f, 0, coordinates[i]));
                }

            }


        }
        for (int i = 0; i < listarr.Count; i++)
        {
            for (int j = 0; j < listarr[i].Count; j++)
            {
                listarr[i][j].transform.localPosition = listpos[i][j];

            }
        }
       


    }

    // Update is called once per frame
     public void Update()
    {

    }

}
