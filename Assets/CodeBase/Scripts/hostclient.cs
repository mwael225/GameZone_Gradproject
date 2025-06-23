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
using Unity.Networking.Transport.Relay;
using Unity.VisualScripting;
using System;
using GameSystem;

public class hostclient : MonoBehaviour
{
    [Header("UI References")]
    public GameObject startHostButton;
    public GameObject startClientButton;
    public GameObject joinCodeInput;
    public GameObject canvas;
    public TMP_Text joinCodeDisplay;

    private async void Awake()
    {
        await authencateandsign();
        // Add button listeners
        startHostButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            StartHostWithRelay(3, "udp").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    joinCodeDisplay.text = "Join Code:" + task.Result;

                }
                else
                {
                    Debug.LogError("Failed to start host: " + task.Exception);
                }
            });
            hideui();
        });
        startClientButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            StartClientWithRelay(joinCodeInput.GetComponent<TMP_InputField>().text, "udp").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully && task.Result)
                {
                    Debug.Log("Client started successfully.");  // Hide the canvas after starting the clien                    
                }
                else
                {
                    Debug.LogError("Failed to start client: " + task.Exception);
                }
            });
            canvas.SetActive(false);
        });
    }
    public async Task<string> authencateandsign()
    {
        await UnityServices.InitializeAsync();
        // Sign in anonymously
        if (!AuthenticationService.Instance.IsSignedIn)
        { 
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        return AuthenticationService.Instance.PlayerId;
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
        joinCodeDisplay.text = "Join Code: " + joinCode;
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
    void hideui()
    {
        startHostButton.SetActive(false);
        startClientButton.SetActive(false);
        joinCodeInput.SetActive(false);
    }
}
