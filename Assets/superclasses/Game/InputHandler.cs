using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;

public class InputHandler : NetworkBehaviour
{
    private ulong clientId;
    private bool iamserver;

    public bool setupinputcalled = false;
    public List<Dictionary<string, bool>> playerInput = new List<Dictionary<string, bool>>
    {
        new Dictionary<string, bool>{},
        new Dictionary<string, bool>{},
        new Dictionary<string, bool>{},
        new Dictionary<string, bool>{}
    };

    public void Start()
    {
        Debug.Log("name " + transform.name + " isserver " + IsServer);
        clientId = NetworkManager.Singleton.LocalClientId;
        if (IsClient)
        {
            Debug.Log("Client started");
        }
        if (IsServer)
        {
            Debug.Log("Server started");
        }
        if (transform.name == "GameManager_N(Clone)" && IsServer)
        {
            Debug.Log("Game manager started/inputhandler");
            iamserver = true;
            Keymap();
        }
    }
    /*public void setupinput(List<KeyCode> keyCodes)
{
    setupinputcalled = true;
    Debug.Log("setup input called");
    this.keyCodes = keyCodes;
    for(int i=0;i<playerInput.Count;i++)
    {
        for(int j=0;j<keyCodes.Count;j++) // Fixed syntax error in commented code
        {
            playerInput[i].Add(keyCodes[j].ToString(),false);
        }
    }
}*/
    public void Update()
    {
        if (IsOwner && IsClient)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                InputGetkeyDownServerRpc(KeyCode.Return, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Return))
            {
                InputGetkeyDownServerRpc(KeyCode.Return, clientId);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                InputGetkeyDownServerRpc(KeyCode.Space, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                InputGetkeyDownServerRpc(KeyCode.Space, clientId);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                InputGetkeyDownServerRpc(KeyCode.Q, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                InputGetkeyDownServerRpc(KeyCode.Q, clientId);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                InputGetkeyDownServerRpc(KeyCode.W, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.W))
            {
                InputGetkeyDownServerRpc(KeyCode.W, clientId);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha1, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha1, clientId);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha2, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha2, clientId);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha3, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha3, clientId);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha4, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha4, clientId);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                InputGetkeyDownServerRpc(KeyCode.M, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.M))
            {
                InputGetkeyDownServerRpc(KeyCode.M, clientId);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                InputGetkeyDownServerRpc(KeyCode.R, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                InputGetkeyDownServerRpc(KeyCode.R, clientId);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                InputGetkeyDownServerRpc(KeyCode.S, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                InputGetkeyDownServerRpc(KeyCode.S, clientId);
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                InputGetkeyDownServerRpc(KeyCode.LeftAlt, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                InputGetkeyDownServerRpc(KeyCode.LeftAlt, clientId);
            }
            if (Input.GetKeyDown(KeyCode.RightAlt))
            {
                InputGetkeyDownServerRpc(KeyCode.RightAlt, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.RightAlt))
            {
                InputGetkeyDownServerRpc(KeyCode.RightAlt, clientId);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                InputGetkeyDownServerRpc(KeyCode.Escape, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                InputGetkeyDownServerRpc(KeyCode.Escape, clientId);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                InputGetkeyDownServerRpc(KeyCode.I, clientId, true);
            }
            else if (Input.GetKeyUp(KeyCode.I))
            {
                InputGetkeyDownServerRpc(KeyCode.I, clientId);
            }
        }
    }

    public void Keymap()
    {
        if (transform.name == "GameManager_N(Clone)" && IsServer)
        {
            Debug.Log("real Keymap started");
        }
        else
        {
            Debug.Log("Keymap started");
        }

        for (int i = 0; i < playerInput.Count; i++)
        {
            playerInput[i].Add(KeyCode.Return.ToString(), false);
            playerInput[i].Add(KeyCode.Space.ToString(), false);
            playerInput[i].Add(KeyCode.Q.ToString(), false);
            playerInput[i].Add(KeyCode.W.ToString(), false);
            playerInput[i].Add(KeyCode.Alpha1.ToString(), false);
            playerInput[i].Add(KeyCode.Alpha2.ToString(), false);
            playerInput[i].Add(KeyCode.Alpha3.ToString(), false);
            playerInput[i].Add(KeyCode.Alpha4.ToString(), false);
            playerInput[i].Add(KeyCode.M.ToString(), false);
            playerInput[i].Add(KeyCode.R.ToString(), false);
            playerInput[i].Add(KeyCode.S.ToString(), false);
            playerInput[i].Add(KeyCode.LeftAlt.ToString(), false);
            playerInput[i].Add(KeyCode.RightAlt.ToString(), false); // Added RightAlt
            playerInput[i].Add(KeyCode.Escape.ToString(), false);
            playerInput[i].Add(KeyCode.I.ToString(), false);
        }
    }

    [ServerRpc]
    public void InputGetkeyDownServerRpc(KeyCode keyCode, ulong clientId, bool isPressed = false)
    {
        Debug.Log($"InputGetkeyDownServerRpc called for key {keyCode} by client {clientId}, isPressed: {isPressed}");
        this.playerInput[(int)clientId][keyCode.ToString()] = isPressed;
    }

    public bool GetKeyDown(KeyCode keyCode, int clientId)
    {
        if (!IsServer && transform.name != "GameManager_N(Clone)")
        {
            Debug.Log("imposter");
            return false;
        }
        if (playerInput[clientId][keyCode.ToString()])
        {
            Debug.Log("key pressed " + keyCode.ToString() + " clientId " + clientId);
            playerInput[clientId][keyCode.ToString()] = false;
            return true;
        }
        return false;
    }
}