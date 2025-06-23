using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections;
using Unity.Networking.Transport.Relay;
using Unity.VisualScripting;
using System;
using GameSystem;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    bool createdlobby, pressedjoin = false; // Flag to check if the lobby has been created
    public GameObject buttonPrefab;
    public static LobbyManager Instance;  // Prefab of the button
    public Transform content;
    public Button createButton, joinbutton; // Reference to the create button

    public TMP_InputField tmpInputField; // Reference to TMP_InputField

    public Player player;
    public Lobby thislobby;

    List<Lobby> lobbies;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //player = new Player();
        if (Instance == null)
        {
            Instance = this; // Set this as the instance
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () => Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        createButton.onClick.AddListener(() =>
        {
            entergame("true");
        });
        joinbutton.onClick.AddListener(() =>
        {
            listlobbies();
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task<string> StartHostWithRelay(int maxConnections, string connectionType)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        //joinCodeDisplay.text = "Join Code: " + joinCode;
        GUIUtility.systemCopyBuffer = joinCode;
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }
    public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
    {
        Debug.Log("Joining relay with code: " + joinCode+"in scene "+SceneManager.GetActiveScene().name+" connection type "+connectionType);
        /*await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }*/
        Debug.Log("why + "+joinCode);
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        await WaitForNetworkSingletonAsync();
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
    public async Task WaitForNetworkSingletonAsync(float timeout = 10f)
    {
        float elapsedTime = 0f;
        while (NetworkManager.Singleton == null && elapsedTime < timeout)
        {
            await Task.Delay(100);  // Wait for 100ms before checking again
            elapsedTime += 0.1f;
        }
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Timed out waiting for NetworkManager.Singleton.");
        }
        else
        {
            Debug.Log("NetworkManager.Singleton is initialized.");
        }
    }
    void listbuttons()
    {
        DestroyAll(content);
        if (lobbies != null)
        {
            for (int i = 0; i < lobbies.Count; i++)
            {
            int index = i; // Capture index, not i
            GameObject newButton = Instantiate(buttonPrefab, content);
            Button button = newButton.GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = lobbies[index].Name + " ";
            button.onClick.AddListener(() =>
            {
                Debug.Log("index: " + index);
                Debug.Log("lobby name: " + lobbies[index].Name);
                string relaycode = lobbies[index].Data["code"].Value;
                Debug.Log("client : " + relaycode);
                entergame("false", relaycode);
            });
            }
        }
    }
    void DestroyAll(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
    async void createlobby(string relaycode)
    {
        var lobbyOptions = new CreateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {
                    "code", new DataObject(DataObject.VisibilityOptions.Public, relaycode)
                }
            }
        };
        if (string.IsNullOrEmpty(tmpInputField.text))
        {
            Debug.LogError("Lobby name cannot be empty.");
        }
        else
        {
            try
            {
                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(tmpInputField.text, 4, lobbyOptions);
                createdlobby = true; // Set the flag to true after creating the lobby
                thislobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to create lobby: " + e.Message);
            }
        }
        

    }
    public async void listlobbies()
    {
        QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
        if (response.Results.Count > 0)
        {
            lobbies = response.Results;
            listbuttons();
        }
    }
   async void entergame(string host="false", string relaycode="")
    {
        SceneManager.LoadScene("room");
        if (host == "false")
        {
            Debug.Log("relay code is : "+relaycode);
            bool x = await StartClientWithRelay(relaycode, "udp");
            Debug.Log(x);
        }
        else
        {
            string code = await StartHostWithRelay(4, "udp");
            Debug.Log("host code: " + code);
            createlobby(code);
        }

    }
    
    private IEnumerator LobbyHeartbeatCoroutine(Lobby lobby)
    {
    const float heartbeatInterval = 15f;

    while (true)
    {
        if (lobby != null)
        {
            yield return LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
        }

        yield return new WaitForSeconds(heartbeatInterval);
    }
    }


}
