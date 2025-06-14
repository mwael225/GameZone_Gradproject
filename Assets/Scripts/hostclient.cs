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

public class hostclient : MonoBehaviour
{
    [Header("UI References")]
    public Button startHostButton;
    public Button startClientButton;
    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeDisplay;

    private async void Awake()
    {
        // Initialize Unity Services
        await UnityServices.InitializeAsync();

        // Sign in anonymously
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }

        // Add button listeners
        startHostButton.onClick.AddListener(() =>
        {
            StartHostWithRelay(4, "udp").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    joinCodeDisplay.text = "Join Code: " + task.Result;
                }
                else
                {
                    Debug.LogError("Failed to start host: " + task.Exception);
                }
            });
        });
        startClientButton.onClick.AddListener(() =>
        {
            StartClientWithRelay(joinCodeInput.text, "udp").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully && task.Result)
                {
                    Debug.Log("Client started successfully.");
                }
                else
                {
                    Debug.LogError("Failed to start client: " + task.Exception);
                }
            });
        });
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
        Debug.Log("Join Code: " + joinCode);
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
}
