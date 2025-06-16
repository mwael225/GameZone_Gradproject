using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class MenuManager1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<TMP_Text> menuItems = new List<TMP_Text>();
    public GameObject menuPanel;
    Color oldcolor;
    int navindex = 0;
    void Start()
    {
        oldcolor = menuItems[0].color;
        updateMenu(); // Initialize the menu with the first item highlighted
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            navindex = (navindex + 1) % menuItems.Count; // Increment navindex and wrap around using modulo
            updateMenu();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            navindex = (navindex - 1 + menuItems.Count) % menuItems.Count; // Decrement navindex
            updateMenu();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            choosegame();
        }
    }
    void updateMenu()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == navindex)
            {
                menuItems[i].color = Color.red; // Highlight the selected item
            }
            else
            {
                menuItems[i].color = oldcolor; // Reset color for unselected items
            }
        }
    }
    void choosegame()
    {
        
    }
}
