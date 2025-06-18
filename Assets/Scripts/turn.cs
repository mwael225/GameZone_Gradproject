using System.Collections.Generic;
using UnityEngine;

public class turn : MonoBehaviour
{
    public Material on;
    public Material off;
    public List<GameObject>turns;
    int currentTurn=0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            turns[currentTurn].GetComponent<MeshRenderer>().material = off;
            currentTurn = (currentTurn + 1) % turns.Count;
            turns[currentTurn].GetComponent<MeshRenderer>().material = on;
        }
    }
}
