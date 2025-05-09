using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
public class Screw_N :CardGame_N
{
    string prefabpath = "ScrewDeck";
    public string [] specialcardsnames= {"Card_Seven","Card_Eight","Card_Nine","Card_Ten","Card_Match","Card_Lookaround","Card_Swap"};
    public int naviagedplayerindex,navigatedplayercard,lookaroundcounter=0;
    bool lookedaround =false;

    GameObject origin;

    public Screw_N(InputHandler inputHandler) : base("Screw", 4,inputHandler)
    {
        centralpileLocalpos = new List<Vector3>();
        pickrotation = new List<Vector3>
        {
            new Vector3(-120, 0, 0), new Vector3(0, 120, -90), new Vector3(120,0 ,180 ), new Vector3(0, -120, 90)
        };
        discardpileRotation = new Vector3(0, 180, 180);
        oldscale =new Vector3(150,225,0.540000081f);
        playerrotations = new List<Vector3>
        {
            new Vector3(0, 0, 180), new Vector3(0, 0, 90), new Vector3(0,0 ,0), new Vector3(0, 0, -90)
        };
        discard_pilespcaing= new List<Vector3> {new Vector3(0, 0, 0.005f)};
        origin = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs_N/Screw_CardDeck_N"));
        origin.GetComponent<NetworkObject>().Spawn();
        GameObjects=spawnobjects(prefabpath);
        for(int i=0;i<GameObjects.Count;i++)
        {
            GameObjects[i].transform.SetParent(origin.transform);
        }
        shuffledeck(GameObjects);
        DealCards(4);
        Assemble(deck);
        setupposition();
        MovetoPostion();
    }
    public IEnumerator memorizecard()
    {
        for(int i =0;i<hands.Count;i++)
        {
            hands[i][2].transform.Translate(0,0,1f);
            hands[i][2].transform.localRotation = Quaternion.Euler(pickrotation[i]);
            hands[i][3].transform.Translate(0,0,1f);
            hands[i][3].transform.localRotation = Quaternion.Euler(pickrotation[i]);
        }
        yield return new WaitForSeconds(4f);
        for(int i =0;i<hands.Count;i++)
        {
            hands[i][2].transform.localPosition = handspostions[i][2];
            hands[i][2].transform.localRotation = Quaternion.Euler(playerrotations[i]);
            hands[i][3].transform.localPosition = handspostions[i][3];
            hands[i][3].transform.localRotation = Quaternion.Euler(playerrotations[i]);
        } 
    }
    public override void setupposition()
    {
        handspostions = new List<List<Vector3>>
            {
                new()
                {
                    new Vector3(0.12f, -2.3f, 0), new Vector3(-1, -2.3f, 0),new Vector3(0.12f, -3.9f, 0), new Vector3(-1, -3.9f, 0),new Vector3(-2.1f,-2.3f,0),new Vector3(-2.1f,-3.9f,0)
                },
                new()
                {
                    new Vector3(-2.9f, -0.6f, 0f),new Vector3(-2.9f, 0.5f, 0f),new Vector3(-4.5f, -0.6f, 0f),new Vector3(-4.5f, 0.5f, 0f)
                },
                new()
                {
                    new Vector3(0.12f, 2.3f, 0),new Vector3(-1, 2.3f, 0),new Vector3(0.12f, 3.9f, 0),new Vector3(-1, 3.9f, 0)
                },
                new()
                {
                    new Vector3(2f, -0.6f, 0f),new Vector3(2f, 0.5f, 0f),new Vector3(3.5f, -0.6f, 0f),new Vector3(3.5f, 0.5f, 0f)
                },
          };
        centralpileLocalpos.Add(new Vector3(-1, 0, 0));
        pickposition = new List<Vector3>
            {
                new Vector3(-0.4f, -3.7f, 1),
                new Vector3(-3.9f, -0.033f, 1),
                new Vector3(-0.4f, 3.7f, 1),
                new Vector3(2.95f,0.033f,1)
            }; 
    }
    public override void DealCards(int numberofcards)
    {
        base.DealCards(numberofcards);
        centralPile.AddLast(deck[deck.Count - 1]);
        deck.RemoveAt(deck.Count - 1);
    }
    protected override void MovetoPostion() 
    {
        base.MovetoPostion();
        centralPile.First.Value.transform.localPosition = centralpileLocalpos[0];
        centralPile.First.Value.transform.Rotate(0, 180, 0);
    }
    public void swapwpickedcard(int player)
    {
        Debug.Log("entered function");
        GameObject temp = hands[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localScale = oldscale;
        hands[player][navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
        hands[player][navigatedCardindex] = pickedcard;
        hands[player][navigatedCardindex].transform.localPosition=handspostions[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localRotation=Quaternion.Euler(playerrotations[player]);
        throwcard(temp);
        pickedcard = null;
        gamestate = "normal";
        
    }
    public void swapwdiscardpile(int player)
    {
        GameObject card = hands[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localScale = oldscale;
        hands[player][navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
        hands[player][navigatedCardindex] = centralPile.Last.Value;
        hands[player][navigatedCardindex].transform.localPosition=handspostions[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localRotation=Quaternion.Euler(playerrotations[player]);
        centralPile.RemoveLast();
        centralpileLocalpos[0]-= discard_pilespcaing[0];
        throwcard(card);
        gamestate = "normal";
    }
    public void match(int player)
    {
        if (gamestate =="basra")
        {
            hands[player][navigatedCardindex].transform.localScale = oldscale;
            hands[player][navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
            throwCard(player, navigatedCardindex);
            pickedcard = null;
            gamestate = "normal";
        }
        else
        {
            if(hands[player][navigatedCardindex].name.Split(" ")[0]==centralPile.Last.Value.name.Split(" ")[0])
            {
                Debug.Log("they match");
                hands[player][navigatedCardindex].transform.localScale = oldscale;
                hands[player][navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
                throwCard(player, navigatedCardindex);
                gamestate ="normal";
            }
            else
            {
                Debug.Log("they don't match");
                hands[player].Add(centralPile.Last.Value);
                hands[player][hands[player].Count-1].transform.localPosition=handspostions[player][hands[player].Count-1];
                hands[player][hands[player].Count-1].transform.localRotation=Quaternion.Euler(playerrotations[player]);
                centralPile.RemoveLast();
                gamestate ="normal";
            }
        }
        
    }

    public IEnumerator seeurcard(int player)
    {
        if(inputHandler.GetKeyDown(KeyCode.Return,player))
        {
        hands[player][navigatedCardindex].transform.localScale = oldscale;
        hands[player][navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
        hands[player][navigatedCardindex].transform.localPosition = pickposition[player];
        hands[player][navigatedCardindex].transform.localRotation = Quaternion.Euler(pickrotation[player]);
        yield return new WaitForSeconds(2f);
        hands[player][navigatedCardindex].transform.localPosition = handspostions[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localRotation = Quaternion.Euler(playerrotations[player]);
        pickedcard = null;
        gamestate = "normal";
        }
        yield return null;
    }
    public IEnumerator seeotherscard(int player)
    {

        if(inputHandler.GetKeyDown(KeyCode.Return,player))
        {
        hands[naviagedplayerindex][navigatedplayercard].transform.localPosition = pickposition[player];
        hands[naviagedplayerindex][navigatedplayercard].transform.localRotation = Quaternion.Euler(pickrotation[player]);
        yield return new WaitForSeconds(2f);
        hands[naviagedplayerindex][navigatedplayercard].transform.localPosition = handspostions[naviagedplayerindex][navigatedplayercard];
        hands[naviagedplayerindex][navigatedplayercard].transform.localRotation = Quaternion.Euler(playerrotations[naviagedplayerindex]);
        gamestate = "normal";
        pickedcard = null;
        }
    }
    public IEnumerator lookaround(int player)
    {
        if(lookedaround==true)
        {
            gamestate = "normal";
            pickedcard = null;
            lookaroundcounter = 0;
            lookedaround = false;
            yield break;
        }
        for(int i =0;i<hands[naviagedplayerindex].Count;i++)
        {
                Debug.Log("player "+player);
                Debug.Log("lookaroundcounter "+lookaroundcounter+" card: "+i);
                hands[player+lookaroundcounter%numberOfPlayers][i].transform.localScale = oldscale;
                hands[player+lookaroundcounter%numberOfPlayers][i].GetComponent<Renderer>().material.color = Color.white;    
        }
        if (inputHandler.GetKeyDown(KeyCode.Q,player))
                {
                    navigatedplayercard =(navigatedplayercard - 1+hands[player+lookaroundcounter%numberOfPlayers].Count )% hands[player+lookaroundcounter%numberOfPlayers].Count;
                }
        else if (inputHandler.GetKeyDown(KeyCode.W,player))
                {
                    navigatedplayercard = (navigatedplayercard + 1) % hands[player+lookaroundcounter&numberOfPlayers].Count;
                }
        if(inputHandler.GetKeyDown(KeyCode.Return,player))
        {
        hands[player+lookaroundcounter%numberOfPlayers][navigatedplayercard].transform.localPosition = pickposition[player];
        hands[player+lookaroundcounter%numberOfPlayers][navigatedplayercard].transform.localRotation = Quaternion.Euler(pickrotation[player]);
        yield return new WaitForSeconds(2f);
        hands[player+lookaroundcounter%numberOfPlayers][navigatedplayercard].transform.localPosition = handspostions[player+lookaroundcounter%numberOfPlayers][navigatedplayercard];
        hands[player+lookaroundcounter%numberOfPlayers][navigatedplayercard].transform.localRotation = Quaternion.Euler(playerrotations[player+lookaroundcounter%numberOfPlayers]);
        lookaroundcounter++;
        player = (player + lookaroundcounter) % numberOfPlayers;
        Debug.Log("lookaroundcounter"+lookaroundcounter);
        }
        GameObject navigatedCard = hands[player+lookaroundcounter%numberOfPlayers][navigatedplayercard];
        navigatedCard.transform.localScale = oldscale * 1.2f;
        navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
        if(lookaroundcounter==4)
        {
            lookedaround = true;
        }   
        yield return null;
    }

    public IEnumerator navigateplayers(int player)
    {
        Debug.Log("entered navigateplayers function");  
        if(gamestate!="seeothercard"||gamestate!="swapwplayer")
        {
            hands[naviagedplayerindex][navigatedplayercard].transform.localScale = oldscale;
            hands[naviagedplayerindex][navigatedplayercard].GetComponent<Renderer>().material.color = Color.white;
            yield return null;
        }
        for(int i =0;i<hands[naviagedplayerindex].Count;i++)
            {
                hands[naviagedplayerindex][i].transform.localScale = oldscale;
                hands[naviagedplayerindex][i].GetComponent<Renderer>().material.color = Color.white;
            }
        if(naviagedplayerindex==player)
            {
                    naviagedplayerindex =(naviagedplayerindex - 1+numberOfPlayers )%numberOfPlayers;
            }
        if (inputHandler.GetKeyDown(KeyCode.A,player))
            {      
                    naviagedplayerindex =(naviagedplayerindex - 1+numberOfPlayers )%numberOfPlayers;
            }
        else if (inputHandler.GetKeyDown(KeyCode.D,player))
                {
                    naviagedplayerindex = (naviagedplayerindex + 1) % numberOfPlayers;
                    if(naviagedplayerindex==player)
                    {
                        naviagedplayerindex = (naviagedplayerindex + 1) % numberOfPlayers;
                    }
                }
        if (inputHandler.GetKeyDown(KeyCode.Q,player))
                {
                    navigatedplayercard =(navigatedplayercard - 1+hands[naviagedplayerindex].Count )% hands[naviagedplayerindex].Count;
                    Debug.Log(navigatedCardindex);
                }
        else if (inputHandler.GetKeyDown(KeyCode.W,player))
                {
                    Debug.Log(navigatedCardindex);
                    navigatedplayercard = (navigatedplayercard + 1) % hands[naviagedplayerindex].Count;
                }
                GameObject navigatedCard = hands[naviagedplayerindex][navigatedplayercard];
                navigatedCard.transform.localScale = oldscale * 1.2f;
                navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
                if(gamestate=="normal")
                {
                    hands[naviagedplayerindex][navigatedplayercard].transform.localScale = oldscale;
                    hands[naviagedplayerindex][navigatedplayercard].GetComponent<Renderer>().material.color = Color.white;
                }
            yield return null;
    }
    public void scoresheet()
    {   
        int []scores = {0,0,0,0};
        for(int i =0;i<hands.Count;i++)
        {
            for(int j =0;j<hands[i].Count;j++)
            {
                switch (hands[i][j].name.Split(" ")[0])
                {
                    
                    case "Card_One":
                        scores[i] += 1;
                        break;
                    case "Card_Two":
                        scores[i] += 2;
                        break;
                    case "Card_Three":
                        scores[i] += 3;
                        break;
                    case "Card_Four":
                        scores[i] += 4;
                        break;
                    case "Card_Five":
                        scores[i] += 5;
                        break;
                    case "Card_Six":
                        scores[i] += 6;
                        break;
                    case "Card_Seven":
                        scores[i] += 7;
                        break;
                    case "Card_Eight":
                        scores[i] += 8;
                        break;
                    case "Card_Nine":
                        scores[i] += 9;
                        break;
                    case "Card_Ten":
                        scores[i] += 10;
                        break;
                    case "Card_Lookaround":
                        scores[i] += 10;
                        break;
                    case "Card_Swap":
                        scores[i] += 10;
                        break;
                    case "Card_Match":
                        scores[i] += 10;
                        break;
                    case "Card_RedScrew":
                        scores[i] += 25;
                        break;
                    case "Card_GreenScrew":
                        scores[i] += 0;
                        break;
                    case "Card_Plus20":
                        scores[i] += 20;
                        break;
                    
                }
            }
        }
        for(int i =0;i<scores.Length;i++)
        {
            Debug.Log("player "+i+" score: "+scores[i]);
        }
    }


}
