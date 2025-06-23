using GameSystem;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject buttons, menu, passpanel;
    public Toggle toggle1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // This is where you can initialize your game or set up any necessary components
        Debug.Log("Game script initialized.");
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void openmenu()
    {
        buttons.SetActive(false);
        menu.SetActive(true);
    }
    public void closemenu()
    {
        buttons.SetActive(true);
        menu.SetActive(false);
    }
    public void togglepass()
    {
        if (toggle1.isOn)
        {
            passpanel.SetActive(true);
        }
        else
        {
            passpanel.SetActive(false);
        }
    }
    public void exit()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
