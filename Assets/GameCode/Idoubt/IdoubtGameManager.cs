using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.ExceptionServices;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
namespace GameSystem
{
    public class IdoubtGameManager : GameManager
    {
        int clientid;
        Idoubt idoubt; // Instance of the Idoubt_N class : idbout_N class is child of CardGame_N class
        public GameObject dropdownPrefab;// list that is used for players to claim before they actually play
        public bool[] passcheck = { false, false, false, false };//checks if the player has passed or not
        bool doubtcall, islying = false;
        string claimer;
        public Transform parentCanvas;
        private Dropdown dropdownInstance;
        private InputHandler inputHandler;
        private bool isOpen = false;
        private int currentIndex = 0;
        List<KeyCode> keyCodes = new List<KeyCode>
        {
            KeyCode.C,
            KeyCode.X,
            KeyCode.Space,
            KeyCode.Q,
            KeyCode.W,
            KeyCode.A,
            KeyCode.D,
            KeyCode.P,
            KeyCode.Return
        };
        enum IdoubtGameState
        {
            Start,
            Navigating,
            Claiming,
            End
        }
        IdoubtGameState idoubt_gamestate;//implement it later
        public void Start()
        {
            if (IsClient)
            {
                clientid = (int)NetworkManager.Singleton.LocalClientId;
                dropdownInstance = GameObject.Find("idoubt ui").transform.GetChild(1).GetComponent<Dropdown>();
            }
            if (!IsServer) return;
            inputHandler = GetComponent<InputHandler>();
            inputHandler.Keymap(keyCodes);
            idoubt = new Idoubt(inputHandler);
            //dropdownPrefab = Resources.Load<GameObject>("Prefabs_N/Dropdown_Networked");
            //Debug.Log(dropdownPrefab.name);//holds the canvas
            //GameObject instance = Instantiate(dropdownPrefab);
            //instance.GetComponent<NetworkObject>().Spawn();
            //Transform child = dropdownPrefab.transform.GetChild(0);//dropdown gamobject
            //dropdownInstance = child.gameObject.GetComponent<Dropdown>();
            currentTurn = firstplayer();
            idoubt.gamestate = "start";
            //dropdownPrefab.SetActive(false);
            //currentIndex = dropdownInstance.value;
            //HighlightOption(currentIndex);
            idoubt.DebugLog2("current turn: " + currentTurn);
            UpdateoverlayClientRpc(currentTurn, 0, false);
        }
        public void Update()
        {
            if (IsClient)
            {
                Dropdownmanager();
            }

            if (!IsServer) return;
            idoubt.DebugLog2("gamstate: " + idoubt.gamestate);
            // Check if the game is over
            if (idoubt.gamestate == "end")
            {
                idoubt.DebugLog2("Game Over");
                return;
            }

            if (!passcheck[currentTurn])
            {
                idoubt.DebugLog2("claim: " + idoubt.claim);
                StartCoroutine(idoubt.navigatedCards(currentTurn));
                if (idoubt.gamestate == "navigating" || idoubt.gamestate == "start")
                {
                    if (inputHandler.GetKeyDown(KeyCode.Space, currentTurn))
                    {
                        if (idoubt.gamestate == "start")
                        {
                            if (idoubt.hands[currentTurn][idoubt.navigatedCardindex].name.Split('-')[1] == "King")
                            {
                                idoubt.SelectCard(idoubt.navigatedCardindex, currentTurn);
                            }
                            else
                            {
                                idoubt.DebugLog2("you can only select kings");
                            }
                        }
                        else
                        {
                            idoubt.SelectCard(idoubt.navigatedCardindex, currentTurn);
                        }
                    }
                    if (inputHandler.GetKeyDown(KeyCode.Return, currentTurn))
                    {
                        if (idoubt.selectedCardsindex.Count != 0)
                        {
                            if (idoubt.gamestate == "start")
                            {
                                bool kingofhearts = false;
                                for (int i = 0; i < idoubt.selectedCardsindex.Count; i++)
                                {
                                    if (idoubt.hands[currentTurn][idoubt.selectedCardsindex[i]].name == "Card_Heart-King")
                                    {
                                        kingofhearts = true;
                                    }
                                }
                                if (kingofhearts)
                                {
                                    idoubt.throwCards(currentTurn, idoubt.selectedCardsindex);
                                    idoubt.selectedCardsindex.Clear();
                                    currentTurn = NextTurn(idoubt.numberOfPlayers);
                                    idoubt.gamestate = "navigating";
                                }
                                else
                                {
                                    idoubt.DebugLog2("throw the king of hearts");
                                }
                            }
                            else
                            {
                                idoubt.throwCards(currentTurn, idoubt.selectedCardsindex);
                                idoubt.selectedCardsindex.Clear();
                                currentTurn = NextTurn(idoubt.numberOfPlayers);
                            }
                        }
                    }
                    if (inputHandler.GetKeyDown(KeyCode.C, currentTurn) && isclaimopen())
                    {
                        setdropdowntrueClientRpc(currentTurn);
                        CanclaimClientRpc(currentTurn,false);
                        idoubt.gamestate = "claiming";
                    }
                }
                else if (idoubt.gamestate == "claiming")
                {
                    for (int i = 0; i < passcheck.Length; i++)
                    {
                        passcheck[i] = false;
                    }
                    setdropdowntrueClientRpc(currentTurn);
                    idoubt.DebugLog2("claiming");


                }
            }
            else
            {
                idoubt.DebugLog2("since you passed you can only call doubt");
            }
            if (inputHandler.GetKeyDown(KeyCode.X, currentTurn))
            {
                doubtcall = true;
                islying = idoubt.doubt(currentTurn);
                currentTurn = NextTurn(idoubt.numberOfPlayers);
            }
            if (inputHandler.GetKeyDown(KeyCode.P, currentTurn)&!isclaimopen())
            {
                passcheck[currentTurn] = true;
                currentTurn = NextTurn(idoubt.numberOfPlayers);
            }
        }
        public override int firstplayer()
        {
            for (int i = 0; i < idoubt.hands.Count; i++)
            {
                for (int j = 0; j < idoubt.hands[i].Count; j++)
                {
                    if (idoubt.hands[i][j].name == "Card_Heart-King")
                        return i;
                }
            }
            return 0;
        }
        public override int NextTurn(int noOfPlayers)
        {
            idoubt.navigatedCardindex = 0;
            idoubt.cardtransformations.scalecard(idoubt.hands[currentTurn][idoubt.navigatedCardindex]);
            idoubt.hands[currentTurn][idoubt.navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
            if (doubtcall)
            {
                doubtcall = false;
                if (islying)
                {
                    //turnvisual();
                    setdropdowntrueClientRpc(currentTurn);
                    return currentTurn;
                }
                else
                {
                    //turnvisual();
                    UpdateoverlayClientRpc(currentTurn-1, idoubt.prev_play.Count,false);
                    setdropdowntrueClientRpc((currentTurn - 1) % noOfPlayers);
                    return (currentTurn - 1) % noOfPlayers;
                }
            }
            UpdateoverlayClientRpc((currentTurn+1)%noOfPlayers, idoubt.prev_play.Count,true,idoubt.claim,currentTurn);
            //turnvisual();
            return (currentTurn + 1) % noOfPlayers;
        }

        void OpenDropdown()
        {
            isOpen = true;
            dropdownInstance.Show();
            HighlightOption(currentIndex);
        }

        void dropdownMoveDown()
        {
            currentIndex = (currentIndex + 1) % dropdownInstance.options.Count;
            HighlightOption(currentIndex);
        }

        void dropdownMoveUp()
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = dropdownInstance.options.Count - 1;

            HighlightOption(currentIndex);
        }

        void HighlightOption(int index)
        {
            dropdownInstance.captionText.text = dropdownInstance.options[index].text;
        }

        void SelectOption()
        {
            dropdownInstance.value = currentIndex;
            dropdownInstance.Hide();
            dropdownInstance.onValueChanged.Invoke(currentIndex);
            isOpen = false;
            string claim = dropdownInstance.options[currentIndex].text;
            GameObject temp = GameObject.Find("idoubt ui");
            temp.gameObject.transform.GetChild(1).gameObject.SetActive(false);
            SendclaimServerRpc(claim);
        }

        public bool isclaimopen()
        {
            for (int i = 0; i < passcheck.Length; i++)
            {
                if (i != currentTurn)
                {
                    if (!passcheck[i])
                    {
                        return false;
                    }
                }
            }
            CanclaimClientRpc(currentTurn,true);
            return true;
        }
        [ClientRpc]
        public void setdropdowntrueClientRpc(int currentTurn)
        {
            if (currentTurn == clientid)
            {
                if (!GameObject.Find("idoubt ui").transform.GetChild(0).gameObject.activeInHierarchy)
                {
                    GameObject.Find("idoubt ui").transform.GetChild(0).gameObject.SetActive(false);
                }
                Debug.Log("UpdateDropdownClientRpc" + currentTurn);
                GameObject temp = GameObject.Find("idoubt ui");
                Transform x = temp.transform.GetChild(1);
                x.gameObject.SetActive(true);
            }

        }
        [ClientRpc]
        public void UpdateoverlayClientRpc(int player, int noofcards, bool isclaim, string claim = "", int claimer = 0)
        {
            GameObject temp = GameObject.Find("idoubt ui");
            Transform x = temp.transform.GetChild(2);
            //x.gameObject.SetActive(true);
            x.GetChild(0).GetComponent<TMP_Text>().text = claim + " Table ";
            x.GetChild(1).GetComponent<TMP_Text>().text = "Turn: " + (player+1);
            if (isclaim)
            {
                x.GetChild(2).GetComponent<TMP_Text>().text = "player" + (claimer+1) + ": " + noofcards + " " + claim;
            }
            else
            {
                x.GetChild(2).GetComponent<TMP_Text>().text ="";
            }

        }
        [ServerRpc]
        public void SendclaimServerRpc(string claim)
        {
            managesentclaim(claim);
        }

        public void Dropdownmanager()
        {
            if (GameObject.Find("idoubt ui").transform.GetChild(1).gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (!isOpen)
                        OpenDropdown();
                    else
                        dropdownMoveDown();
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) && isOpen)
                {
                    dropdownMoveUp();
                }
                if (Input.GetKeyDown(KeyCode.Return) && isOpen)
                {
                    SelectOption();
                }

            }
        }
        [ClientRpc]
        void CanclaimClientRpc(int clientid,bool canclaim)
        {
            if (clientid == currentTurn)
            {
                GameObject temp = GameObject.Find("idoubt ui");
                temp.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                GameObject temp = GameObject.Find("idoubt ui");
                temp.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        void managesentclaim(string claim)
        {
            idoubt.claim = claim;
            idoubt.gamestate = "navigating";
        }
}
}

