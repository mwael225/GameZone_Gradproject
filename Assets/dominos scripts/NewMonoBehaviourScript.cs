using UnityEngine;
using System.Collections.Generic;
using System;

public class NewMonoBehaviourScript : MonoBehaviour
{
    float x_axis = 0.3f;
    Drawcards x = new Drawcards();
    int nextp = 0;
    float x_axis_l;
    float x_axis_r;
    Vector3 domino_scale = new Vector3(0.7f, 0.7f, 0.7f);
    List<KeyCode> keys = new List<KeyCode>()
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7
    };

    List<List<GameObject>> leftover = new List<List<GameObject>>()
    {
        new List<GameObject>(),
        new List<GameObject>()
    };
    private Vector3 frst_pos;
    public Vector3 leftpos;
    public Vector3 rightpos;
    Boolean flag1 = false;

    // Variable to store the selected domino
    private GameObject selectedDomino = null;
    private Material originalMaterial; // To store the original material of the selected domino
    public Material highlightMaterial; // Assign a highlight material in the Inspector

    // Increased spacing between dominoes
    private float dominoSpacing = 0.12f; // Adjust this value as needed

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        x.Start();
        frst_pos = new Vector3(x_axis, -0.09f, 0.08f);

        leftpos = frst_pos;
        rightpos = frst_pos;
        x_axis_l = x_axis;
        x_axis_r = x_axis;
    }

    // Update is called once per frame
    void Update()
    {
        x.Update();

        // Check if the player presses 'X' to find the player with Domino_6-6
        if (Input.GetKeyDown(KeyCode.X))
        {
            for (int i = 0; i < x.listarr.Count; i++)
            {
                for (int j = 0; j < x.listarr[i].Count; j++)
                {
                    if (x.listarr[i][j].name == "Domino_6-6")
                    {
                        leftover[0].Add(x.listarr[i][j]);
                        leftover[1].Add(x.listarr[i][j]);
                        x.listarr[i].RemoveAt(j);
                        leftover[0][leftover[0].Count - 1].transform.localPosition = frst_pos;
                        leftover[0][leftover[0].Count - 1].transform.localRotation = Quaternion.Euler(-90, 0, 0);
                        leftover[0][leftover[0].Count - 1].transform.localScale = domino_scale;
                        nextp = ++i;
                    }
                }
            }
        }

        if (nextp > x.listarr.Count - 1)
        {
            nextp = 0;
        }
        else
        {
            for (int i = 0; i < x.listarr[nextp].Count; i++)
            {
                // Highlight the selected domino when a key (1-7) is pressed
                if (Input.GetKeyDown(keys[i]))
                {
                    // Remove highlight from the previously selected domino
                    if (selectedDomino != null)
                    {
                        selectedDomino.GetComponent<Renderer>().material = originalMaterial;
                    }

                    // Store the selected domino and its original material
                    selectedDomino = x.listarr[nextp][i];
                    originalMaterial = selectedDomino.GetComponent<Renderer>().material;

                    // Highlight the selected domino
                    selectedDomino.GetComponent<Renderer>().material = highlightMaterial;
                    flag1 = true;
                }

                // Place the selected domino on the left or right side
                if (flag1)
                {
                    if (Input.GetKeyDown(KeyCode.E)) // Left side
                    {
                        if (CanPlaceDomino(selectedDomino, 0)) // Check if the domino can be placed on the left
                        {
                            PlaceDominoOnTable(selectedDomino, ref x_axis_l, ref leftpos, Quaternion.Euler(270, 0, 90), 0);
                            flag1 = false;
                        }
                        else
                        {
                            Debug.Log("Invalid move: Numbers do not match!");
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.Q)) // Right side
                    {
                        if (CanPlaceDomino(selectedDomino, 1)) // Check if the domino can be placed on the right
                        {
                            PlaceDominoOnTable(selectedDomino, ref x_axis_r, ref rightpos, Quaternion.Euler(270, 0, 90), 1);
                            flag1 = false;
                        }
                        else
                        {
                            Debug.Log("Invalid move: Numbers do not match!");
                        }
                    }
                }
            }
        }
    }

    // Helper method to place a domino on the table
    private void PlaceDominoOnTable(GameObject domino, ref float xAxis, ref Vector3 position, Quaternion rotation, int side)
    {
        // Remove highlight
        domino.GetComponent<Renderer>().material = originalMaterial;

        // Place the domino on the table
        leftover[side].Add(domino);
        x.listarr[nextp].Remove(domino);

        // Adjust the position with increased spacing
        xAxis += (side == 0) ? -dominoSpacing : dominoSpacing;
        position = new Vector3(xAxis, -0.09f, 0.08f);
        domino.transform.localPosition = position;
        domino.transform.localRotation = rotation;
        domino.transform.localScale = domino_scale;

        // Move to the next player
        nextp = (nextp + 1) % x.listarr.Count;

        // Clear the selected domino
        selectedDomino = null;
    }

    // Helper method to check if the selected domino can be placed
    private bool CanPlaceDomino(GameObject domino, int side)
    {
        // Get the numbers on the selected domino
        string[] selectedNumbers = domino.name.Split(new char[] { '_', '-' });
        int selectedLeft = int.Parse(selectedNumbers[1]);
        int selectedRight = int.Parse(selectedNumbers[2]);

        // Get the numbers on the last domino placed on the specified side
        if (leftover[side].Count > 0)
        {
            string[] lastNumbers = leftover[side][leftover[side].Count - 1].name.Split(new char[] { '_', '-' });
            int lastLeft = int.Parse(lastNumbers[1]);
            int lastRight = int.Parse(lastNumbers[2]);

            // Check if the selected domino matches the last domino on the specified side
            if (side == 0) // Left side (Q)
            {
                // The selected domino must match the left number of the last domino on the left side
                return selectedRight == lastLeft || selectedLeft == lastLeft;
            }
            else // Right side (E)
            {
                // The selected domino must match the right number of the last domino on the right side
                return selectedLeft == lastRight || selectedRight == lastRight;
            }
        }

        // If no dominoes are placed yet, allow placement
        return true;
    }
}