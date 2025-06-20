using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using GLTFast.Schema;
public class opensessions : MonoBehaviour
{
    bool createdlobby, pressedjoin = false; // Flag to check if the lobby has been created
    public GameObject buttonPrefab;  // Prefab of the button
    public Transform content;
    public Button createButton, joinbutton; // Reference to the create button

    public TMP_InputField tmpInputField; // Reference to TMP_InputField

    async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () => Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        createButton.onClick.AddListener(() =>
        {
            createlobby();
            //entergame();
        }
        );
        joinbutton.onClick.AddListener(() => lobbiestobuttons());
        StartCoroutine(refresh());
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
    async void createlobby()
    {

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
        
    }
    void entergame()
    {
        SceneManager.LoadScene("room");
    }
    
}
