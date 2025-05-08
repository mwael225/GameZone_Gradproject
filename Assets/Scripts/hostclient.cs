using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class hostclient : MonoBehaviour
{
    public Button startHostButton; // Reference to the button in the UI
    public Button startClientButton; // Reference to the button in the UI

    public GameObject spawner; // Reference to the player prefab
    void Start()
    {
        
    }
    void Update()
    {
        //Debug.Log("Number of connected clients: " + NetworkManager.Singleton.ConnectedClients.Count);
    }

    void Awake()
    {
        startHostButton.onClick.AddListener(starthost); // Add listener to the button
        startClientButton.onClick.AddListener(startclient); // Add listener to the button
    }
    void starthost()
    {
        NetworkManager.Singleton.StartHost();  
        Debug.Log("Host started."); 
    }
    void startclient()
    {
        NetworkManager.Singleton.StartClient();  
        Debug.Log("Client started."); 
    }

}
