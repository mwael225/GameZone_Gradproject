using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;

public class InputHandler : NetworkBehaviour
{
    ulong clientId;
    bool iamserver;
    /*public InputHandler(string x="someone kill me")
    {
        Debug.Log(x);
    }*/
    public List<Dictionary<string,bool>> playerInput = new List<Dictionary<string,bool>>
    {
        new Dictionary<string,bool>{},
        new Dictionary<string,bool>{},
        new Dictionary<string,bool>{},
        new Dictionary<string,bool>{}
    };
    public void Start()
    {
        Debug.Log("name "+transform.name+" isserver"+IsServer);
        clientId = NetworkManager.Singleton.LocalClientId;
        if(IsClient)
        {
            Debug.Log("Client started");
        }
        if(IsServer)
        {
            Debug.Log("Server started");
        }
        if(transform.name=="GameManager_N(Clone)"&&IsServer)
        {
            Debug.Log("Game manager started/inputhandler started");
            iamserver=true;
            Keymap();
        }

    }
    int count = 0;
    public void Update()
    {
        if(IsOwner&&IsClient)
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                InputGetkeyDownServerRpc(KeyCode.Return,clientId,true);
            }
            else if(Input.GetKeyUp(KeyCode.Return))
            {
                InputGetkeyDownServerRpc(KeyCode.Return,clientId);
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                InputGetkeyDownServerRpc(KeyCode.Space,clientId,true);
            }
            else if(Input.GetKeyUp(KeyCode.Space))
            {
                InputGetkeyDownServerRpc(KeyCode.Space,clientId);
            }
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha1,clientId,true);
            }
            else if(Input.GetKeyUp(KeyCode.Alpha1))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha1,clientId);
            }

            if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha2,clientId,true);
            }
            else if(Input.GetKeyUp(KeyCode.Alpha2))
            {
                InputGetkeyDownServerRpc(KeyCode.Alpha2,clientId);
            }
            if(Input.GetKeyDown(KeyCode.Q))
            {
                InputGetkeyDownServerRpc(KeyCode.Q,clientId,true);
            }
            else if(Input.GetKeyUp(KeyCode.Q))
            {
                InputGetkeyDownServerRpc(KeyCode.Q,clientId);
            }
            if(Input.GetKeyDown(KeyCode.W))
            {
                InputGetkeyDownServerRpc(KeyCode.W,clientId,true);
            }
            else if(Input.GetKeyUp(KeyCode.W))
            {
                InputGetkeyDownServerRpc(KeyCode.W,clientId);
            }
        }

    }
    public void Keymap()
    {
        string key= "";
        if(transform.name=="GameManager_N(Clone)"&&IsServer)
        {
            Debug.Log("real Keymap started");
        }
        else
        Debug.Log("Keymap started");

        for(int i=0;i<playerInput.Count;i++)
            {
                key= KeyCode.Return.ToString();
                Debug.Log(key);
                playerInput[i].Add(KeyCode.Return.ToString(),false);
                playerInput[i].Add(KeyCode.Space.ToString(),false);
                playerInput[i].Add(KeyCode.Alpha1.ToString(),false);
                playerInput[i].Add(KeyCode.Alpha2.ToString(),false);
                playerInput[i].Add(KeyCode.Q.ToString(),false);
                playerInput[i].Add(KeyCode.W.ToString(),false);
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

        if(!IsServer&&transform.name!="GameManager_N(Clone)")
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