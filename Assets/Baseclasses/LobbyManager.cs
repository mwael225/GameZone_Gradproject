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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
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
        createButton.onClick.AddListener(async () =>
        {
            entergame();
            string relaycode = await StartHostWithRelay(4, "udp");
            createlobby(relaycode);
        });
        joinbutton.onClick.AddListener(() => lobbiestobuttons());
        StartCoroutine(refresh());
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
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
    void lobbiestobuttons(List<Lobby> lobbies = null)
    {
        DestroyAll(content);
        if (lobbies != null)
        {
            for (int i = 0; i < lobbies.Count; i++)
            {
                Debug.Log(lobbies[i].Name);
                GameObject newButton = Instantiate(buttonPrefab, content);
                Button button = newButton.GetComponent<Button>();
                button.GetComponentInChildren<TMP_Text>().text = lobbies[i].Name; // Set button text to lobby name  
                button.onClick.AddListener(() => OnButtonClick(lobbies[i]));
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
                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(tmpInputField.text, 4);
                createdlobby = true; // Set the flag to true after creating the lobby
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
            lobbiestobuttons(response.Results);
    }
    IEnumerator refresh()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            listlobbies();
        }
    }
    void OnButtonClick(Lobby lobby)
    {
        Debug.Log(lobby.Name);
    }
    void entergame()
    {
        SceneManager.LoadScene("room");
    }

}
