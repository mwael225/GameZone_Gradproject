using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;  
public class InputHandler : NetworkBehaviour
{
    ulong clientId;
    bool iamserver;
    public List<Dictionary<string,bool>> playerInput = new List<Dictionary<string,bool>>
    {
        new Dictionary<string,bool>{},
        new Dictionary<string,bool>{},
        new Dictionary<string,bool>{},
        new Dictionary<string,bool>{}
    };
    
    public List<KeyCode> keyCodes = new List<KeyCode>();
    public void Start()
    {

        Debug.Log("name " + transform.name + " isserver" + IsServer);
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
            Debug.Log("Game manager started/inputhandler started");
            iamserver = true;
            //Keymap();
        }

    }
    /*public void setupinput(List<KeyCode> keyCodes)
    {
        setupinputcalled = true;
        Debug.Log("setup input called");
        this.keyCodes = keyCodes;
        for(int i=0;i<playerInput.Count;i++)
        {
            for(int j=0;j<keyCodes.Count;j++)
            {
                playerInput[i].Add(keyCodes[j].ToString(),false);
            }
        }
    }*/
    public void Update()
    {
        if (IsOwner && IsClient)
        {
            for (int i = 0; i < keyCodes.Count; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    InputGetkeyDownServerRpc(keyCodes[i],clientId,true);
                }
                else if (Input.GetKeyUp(keyCodes[i]))
                {
                    InputGetkeyDownServerRpc(keyCodes[i],clientId,false);
                }
            }

        }
    }
    public void Keymap(List<KeyCode> keyCodes)
    {
        this.keyCodes = keyCodes;
        for(int i=0;i<playerInput.Count;i++)
        {
            for(int j=0;j<keyCodes.Count;j++)
            {
                playerInput[i].Add(keyCodes[j].ToString(),false);
            }
        }
    }
    

    [ServerRpc]
    public void InputGetkeyDownServerRpc(KeyCode keyCode,ulong clientId,bool isPressed = false)
    {
        GameObject Gamemanger = GameObject.Find("GameManager_N(Clone)");
        InputHandler inputHandler = Gamemanger.GetComponent<InputHandler>();
        inputHandler.playerInput[(int)clientId][keyCode.ToString()] = isPressed;
    }
    public bool GetKeyDown(KeyCode keyCode,int clientId)
    {
        clientId = 0;
        if (!IsServer && transform.name != "GameManager_N(Clone)")
        {
            Debug.Log("imposter");
            return false;
        }
        if(playerInput[clientId][keyCode.ToString()])
        {
        Debug.Log("key pressed"+keyCode.ToString()+" clientId "+clientId);
        playerInput[clientId][keyCode.ToString()]=false;
        return true;
        }
        return false;
        
    }
 
}