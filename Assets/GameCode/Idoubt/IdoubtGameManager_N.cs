using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.ExceptionServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
namespace GameSystem
{   
    public class IdoubtGameManager_N:GameManager_N
    {
    Idoubt_N idoubt;
    public List<GameObject> dropdownPrefab;
    
    public bool[] passcheck = { false, false, false, false };
    bool doubtcall,isdoubt = false;
    public Transform parentCanvas;
    private Dropdown dropdownInstance;
    private InputHandler inputHandler;
    private bool isOpen = false;
    private int currentIndex = 0;
    public void Start()
    {
        if (!IsServer) return;
        inputHandler = GetComponent<InputHandler>();
        idoubt = new Idoubt_N(inputHandler);
        dropdownPrefab = idoubt.prefabtoGamebojects("Prefabs/dropdown");
        Transform child = dropdownPrefab[0].transform.GetChild(0);
        dropdownInstance = child.gameObject.GetComponent<Dropdown>();
        currentTurn=firstplayer();
        idoubt.gamestate="start";
        dropdownPrefab[0].SetActive(false);
        if (dropdownPrefab == null)
        {
            Debug.LogError("Dropdown prefab or parentCanvas is not assigned!");
            return;
        }

        // Instantiate the dropdown
        if (dropdownInstance == null)
        {
            Debug.LogError("The prefab does not have a Dropdown component.");
            return;
        }

        currentIndex = dropdownInstance.value;
        HighlightOption(currentIndex);
        Debug.Log("current turn: " + currentTurn);
        
    }
    public void Update()
    {
        if (!IsServer) return;

        if (idoubt.gamestate == "end")
            {
                Debug.Log("Game Over");
                return;
            }
        if(!passcheck[currentTurn])
        {
            Debug.Log("claim: " + idoubt.claim);
            StartCoroutine(idoubt.navigatedCards(currentTurn));
            if(idoubt.gamestate=="navigating" || idoubt.gamestate=="start")
            {
                if(isclaimopen())
                {
                    Debug.Log("press c to claim");
                }
                if(dropdownPrefab[0].activeSelf)
                {
                    dropdownPrefab[0].SetActive(false);
                }
                if (inputHandler.GetKeyDown(KeyCode.Space,currentTurn))
                    {
                        if(idoubt.gamestate=="start")
                        {
                            if(idoubt.hands[currentTurn][idoubt.navigatedCardindex].name.Split('-')[1]=="King")
                            {
                                idoubt.SelectCard(idoubt.navigatedCardindex,currentTurn);
                            }
                            else
                            {
                                Debug.Log("you can only select kings");
                            }
                        }
                        else
                        {
                            idoubt.SelectCard(idoubt.navigatedCardindex,currentTurn);
                        }
                    }
                if(inputHandler.GetKeyDown(KeyCode.Return,currentTurn))
                    {
                        if(idoubt.selectedCardsindex.Count!=0)
                        {
                        if(idoubt.gamestate=="start")
                        {
                            bool kingofhearts=false;
                            for(int i=0;i<idoubt.selectedCardsindex.Count;i++)
                            {
                                if(idoubt.hands[currentTurn][idoubt.selectedCardsindex[i]].name=="Card_Heart-King")
                                {
                                    kingofhearts=true;
                                }
                            }
                            if(kingofhearts)
                            {
                                idoubt.throwCards(currentTurn,idoubt.selectedCardsindex);
                                idoubt.selectedCardsindex.Clear();
                                currentTurn=NextTurn(idoubt.numberOfPlayers);
                                idoubt.gamestate="navigating";
                            } 
                            else
                            {
                                Debug.Log("throw the king of hearts");
                            }
                        }
                        else
                        {
                            idoubt.throwCards(currentTurn,idoubt.selectedCardsindex);
                            idoubt.selectedCardsindex.Clear();
                            currentTurn=NextTurn(idoubt.numberOfPlayers);
                        }
                    }
                    }
                if(inputHandler.GetKeyDown(KeyCode.C,currentTurn) && isclaimopen())
                    {
                        idoubt.gamestate="claiming";
                    }
            }
            else if(idoubt.gamestate=="claiming")
            {
                for(int i=0;i<passcheck.Length;i++)
                {
                    passcheck[i]=false;
                }
                Debug.Log("claiming");
                dropdownPrefab[0].SetActive(true);
                if (dropdownInstance == null)
                    return;
                if (inputHandler.GetKeyDown(KeyCode.DownArrow,currentTurn))
                {
                    if (!isOpen)
                        OpenDropdown();
                    else
                        dropdownMoveDown();
                }
                if (inputHandler.GetKeyDown(KeyCode.UpArrow,currentTurn) && isOpen)
                {
                    dropdownMoveUp();
                }
                if ((inputHandler.GetKeyDown(KeyCode.LeftAlt,currentIndex) || inputHandler.GetKeyDown(KeyCode.RightAlt,currentTurn)) && isOpen)
                {
                    SelectOption();
                }
                if (inputHandler.GetKeyDown(KeyCode.Escape,currentTurn) && isOpen)
                {
                    CloseDropdown();
                }
            }
        }
        else
        {
            Debug.Log("since you passed you can only call doubt");
        }
        if(inputHandler.GetKeyDown(KeyCode.X,currentTurn))
            { 
                doubtcall=true;
                isdoubt=idoubt.doubt(currentTurn);
                currentTurn=NextTurn(idoubt.numberOfPlayers);
            }
        if(inputHandler.GetKeyDown(KeyCode.P,currentTurn))
            {
                if(isclaimopen())
                {
                    idoubt.gamestate="claiming";
                }
                passcheck[currentTurn]=true;
                currentTurn=NextTurn(idoubt.numberOfPlayers);
            }
    }
    public override int firstplayer()
    {
        for(int i=0;i<idoubt.hands.Count;i++)
        {
            for(int j=0;j<idoubt.hands[i].Count;j++)
            {
                if(idoubt.hands[i][j].name=="Card_Heart-King")
                return i;
            }
        }
        return 0;
    }
    public override int NextTurn(int noOfPlayers)
    {
        idoubt.navigatedCardindex=0;
        idoubt.hands[currentTurn][idoubt.navigatedCardindex].transform.localScale = idoubt.oldscale;
        idoubt.hands[currentTurn][idoubt.navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
        if(doubtcall)
        {
            doubtcall=false;
            if(isdoubt)
            {
                return currentTurn;
            }
            else
            {
                return (currentTurn -1) % noOfPlayers;
            }
        }
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
        idoubt.gamestate="navigating";
        idoubt.claim=dropdownInstance.options[currentIndex].text;
    }

    void CloseDropdown()
    {
        dropdownInstance.Hide();
        isOpen = false;
        currentIndex = dropdownInstance.value; // Reset highlight
        HighlightOption(currentIndex);
    }
    public bool isclaimopen()
    {
        for(int i=0;i<passcheck.Length;i++)
        {
            if(i!=currentTurn)
            {
                if(!passcheck[i])
                {
                    return false;
                }
            }
        }
        return true;
    }
    
}
}

