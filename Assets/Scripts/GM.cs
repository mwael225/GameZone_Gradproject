using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Playmode;
using UnityEngine;

public class GM : MonoBehaviour
{
    List<GameObject> cards = new();
    List<GameObject> Shuffled_deck = new();
    List<GameObject> centralpile = new();
    int p = 0;
    int[] rotations = { 0, -90, 180, 90 };
    Boolean basra = false;
    int count=0;
    
    Boolean pick = false;
    
    List<List<Vector3>> listpos = new List<List<Vector3>>
    {
        new()
        {
            new Vector3(0.12f, 0, 2.3f),
            new Vector3(-1, 0, 2.3f),
            new Vector3(0.12f, 0, 3.9f),
            new Vector3(-1, 0, 3.9f),
        },
        new()
        {
            new Vector3(-2.9f, 0, 0.6f),
            new Vector3(-2.9f, 0, -0.5f),
            new Vector3(-4.5f, 0, 0.6f),
            new Vector3(-4.5f, 0, -0.5f)
        },
        new()
        {
            new Vector3(0.12f, 0, -2.3f),
            new Vector3(-1, 0, -2.3f),
            new Vector3(0.12f, 0, -3.9f),
            new Vector3(-1, 0, -3.9f)
        },
        new()
        {
            new Vector3(2f, 0, 0.6f),
            new Vector3(2f, 0, -0.5f),
            new Vector3(3.5f, 0, 0.6f),
            new Vector3(3.5f, 0, -0.5f)
        },
    };

    System.Random rand = new();
    int cplayer = 0;
    Vector3 centralpileLocalpos = new Vector3(-1, 0, 0);
    List<Vector3> pickposition = new List<Vector3>
    {
        new Vector3(-0.4f, 1, 3.7f),
        new Vector3(-3.7f, 1, -0.4f),
        new Vector3(-0.4f, 1, -3.7f),
        new Vector3(3.7f, 1, -0.4f)
    };
    GameObject temp;

    Boolean ongoing,
        ongoing2,
        ongoing3,
        ongoing4,
        ongoing5 = false;

    List<KeyCode> keycodes = new List<KeyCode>
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4
    };
    List<KeyCode> cardcodes = new List<KeyCode> { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R };

    List<List<GameObject>> listarr =
        new()
        {
            new List<GameObject> { },
            new List<GameObject> { },
            new List<GameObject> { },
            new List<GameObject> { },
        };
    float sum = 0;
    String[] card = new string[]
    {
        "One",
        "Two",
        "Three",
        "Four",
        "Five",
        "Six",
        "Seven",
        "Eight",
        "Nine",
        "Ten",

    };
    void Start()
    {
        String cname = "";
        for (int i = 0; i < card.Length; i++)
        {
            if (i < card.Length - 7)
            {
                for (int j = 1; j <= 4; j++)
                {
                    cname = "Card_" + card[i] + " (" + j + ")";
                    cards.Add(GameObject.Find(cname));
                }
            }
            else
            {
                cname = "Card_" + card[i] + " (" + 1 + ")";

                cards.Add(GameObject.Find(cname));
            }
        }
        while (cards.Count > 0)
        {
            int x = cards.Count;
            int randomIndex = rand.Next(0, x);
            temp = cards[randomIndex];
            cards.RemoveAt(randomIndex);
            Debug.Log(cards.Count);
            Debug.Log(temp.name);

            Shuffled_deck.Add(temp);
        }
        Debug.Log(Shuffled_deck);
        Assemble(Shuffled_deck);
        Gamestart();
    }

    void Update()
    {
        //centralpile[centralpile.Count-1].transform.localPosition=MoveSmoothly(centralpile[centralpile.Count-1].transform.localPosition,centralpileLocalpos,3f);

        if (Input.GetKeyDown(keycodes[0]) | ongoing)
        {
            pickcard(p);
        }
        else if (Input.GetKeyDown(keycodes[1]) | ongoing2)
        {
            Swap(p);
        }
        else if (Input.GetKeyDown(keycodes[2]) | ongoing3)
        {
            Match(p);
        }
    }

    void Gamestart()
    {
        int lastindex = Shuffled_deck.Count - 1;
        temp = Shuffled_deck[lastindex];
        Shuffled_deck.RemoveAt(lastindex);
        centralpile.Add(temp);
        centralpile[0].transform.Rotate(0, 180, 0);
        centralpile[0].transform.localPosition = centralpileLocalpos;

        for (int i = 0; i < listarr.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                lastindex = Shuffled_deck.Count - 1;
                temp = Shuffled_deck[lastindex];
                Shuffled_deck.RemoveAt(lastindex);
                listarr[i].Add(temp);
                listarr[i][j].transform.Rotate(0, 0, rotations[i]);
                listarr[i][j].transform.localPosition = listpos[i][j];
            }
        }
    }

    void pickcard(int p)
    {
        if (!ongoing4)
        {
            ongoing = true;
            Shuffled_deck[Shuffled_deck.Count - 1].transform.localPosition = pickposition[p];
            Shuffled_deck[Shuffled_deck.Count - 1].transform.Rotate(0, 0, 180);
            Shuffled_deck[Shuffled_deck.Count - 1].transform.Rotate(-90, 0, 0); // Rotate 1 degree per frame
            ongoing4 = true;
            StartCoroutine(WaitAndAllowClickAgain(0.5f));
        }
        if (pick)
        {
            if (Input.GetKeyDown(keycodes[0])||ongoing5) //swap
            {
                Debug.Log("swap");
                ongoing5 = true;    
                for (int i = 0; i < cardcodes.Count; i++)
                {
                    if (Input.GetKeyDown(cardcodes[i]))
                    {
                        temp = listarr[0][i];
                        listarr[0][i] = Shuffled_deck[Shuffled_deck.Count - 1];
                        centralpile.Add(temp);
                        Shuffled_deck.RemoveAt(Shuffled_deck.Count - 1);
                        centralpileLocalpos += new Vector3(0, 0.01f, 0f);
                        listarr[0][i].transform.Rotate(90, 0, 0);
                        listarr[0][i].transform.Rotate(0, 0, 180);
                        listarr[0][i].transform.localPosition = listpos[0][i];
                        centralpile[centralpile.Count - 1].transform.Rotate(0, 180, 0);
                        centralpile[centralpile.Count - 1].transform.localPosition=centralpileLocalpos;
                        pick = false;
                        ongoing = false;
                        ongoing4 = false;
                    }
                }
            }
            else if (Input.GetKeyDown(keycodes[1])) //return
            {
                Debug.Log("return");
                    Shuffled_deck[Shuffled_deck.Count - 1].transform.Rotate(-90, 0, 0);
                    centralpileLocalpos += new Vector3(0, 0.01f, 0f);
                    centralpile.Add(Shuffled_deck[Shuffled_deck.Count - 1]);
                    centralpile[centralpile.Count - 1].transform.localPosition=centralpileLocalpos;
                    Shuffled_deck.RemoveAt(Shuffled_deck.Count - 1);
                    pick = false;
                    ongoing = false;
                    ongoing4 = false;
                    count = 0;

               if (centralpile[centralpile.Count - 1].name.Split()[0] == "Card_Match")
                {
                    basra = true;
                    Match(p);
                }
                if (
                    centralpile[centralpile.Count - 1].name.Split()[0] == "Card_Seven"
                    || centralpile[centralpile.Count - 1].name.Split()[0] == "Card_Eight"
                )
                {
                    StartCoroutine(Seeurcard(p));
                }
                if (centralpile[centralpile.Count - 1].name.Split()[0] == "Card_Swap")
                {
                    //StartCoroutine(Swapwithplayer());
                }
            }
        }
    }

    void Swap(int p)
    {
        ongoing2 = true;
        for (int i = 0; i < cardcodes.Count; i++)
        {
            if (Input.GetKeyDown(cardcodes[i]))
            {
                temp = listarr[0][i];
                listarr[0][i] = centralpile[centralpile.Count - 1];
                centralpile.RemoveAt(centralpile.Count - 1);
                centralpile.Add(temp);
                listarr[0][i].transform.Rotate(180, 0, 0);
                listarr[0][i].transform.Rotate(0, 0, 180);
                listarr[0][i].transform.localPosition = listpos[0][i];
                centralpile[centralpile.Count - 1].transform.Rotate(0, 180, 0);
                centralpile[centralpile.Count - 1].transform.localPosition = centralpileLocalpos;
                ongoing2 = false;
            }
        }
    }
    void Match(int p)
    {
        ongoing3 = true;
        for (int i = 0; i < cardcodes.Count; i++)
        {
            if (Input.GetKeyDown(cardcodes[i]))
            {
                if (listarr[0][i].name.Split()[0] == centralpile[centralpile.Count - 1].name.Split()[0]|| basra)
                {
                    temp = listarr[0][i];
                    listarr[0][i] = null;
                    centralpile.Add(temp);
                    centralpileLocalpos += new Vector3(0, 0.01f, 0f);
                    centralpile[centralpile.Count - 1].transform.Rotate(180, 0, 0);
                    centralpile[centralpile.Count - 1].transform.Rotate(0, 0, 180);
                    centralpile[centralpile.Count - 1].transform.localPosition = centralpileLocalpos;
                    ongoing3 = false;
                }
                else
                {
                    temp = centralpile[centralpile.Count - 1];
                    centralpile.RemoveAt(centralpile.Count - 1);
                    listarr[0].Add(temp);
                    listarr[0][listarr[0].Count - 1].transform.Rotate(180, 0, 0);
                    listarr[0][listarr[0].Count - 1].transform.Rotate(0, 0, 180);
                    listarr[0][listarr[0].Count - 1].transform.localPosition = listpos[0][listarr[0].Count - 1];
                    ongoing3 = false;
                }
            }
        }
    }
    IEnumerator Seeurcard(int p)
    {
        ongoing4 = true;
        while (ongoing4)
        {
            for (int i = 0; i < cardcodes.Count; i++)
            {
                if (Input.GetKeyDown(cardcodes[i]))
                {
                                                                
                    listarr[0][i].transform.Rotate(90, 0, 0);
                    listarr[0][i].transform.Rotate(0, 0, 180);
                    listarr[0][i].transform.localPosition=pickposition[0];                    
                    yield return new WaitForSeconds(3f);    
                    listarr[0][i].transform.Rotate(0, 0, 180);
                    listarr[0][i].transform.Rotate(-90, 0, 0);
                    listarr[0][i].transform.localPosition=listpos[0][i];                    
                    ongoing = false;
                    pick = false;
                    ongoing4 = false;
                }
            
            }
            Debug.Log("testing123");
            yield return null;
        }
    }
    IEnumerator seeothercard(int p)
    {
        ongoing4 = true;
        while (ongoing4)
        {
            for (int i = 0; i < cardcodes.Count; i++)
            {
                if(Input.GetKeyDown(keycodes[i]))
                {
                    if (Input.GetKeyDown(cardcodes[i]))
                    {

                        listarr[0][i].transform.Rotate(0, 90, 0);
                        listarr[0][i].transform.localPosition=pickposition[0];
                        Debug.Log("1,2,3");
                        yield return new WaitForSeconds(3f);    
                        ongoing = false;
                        pick = false;
                        ongoing4 = false;

                    }
            }
            }
            Debug.Log("testing123");
            yield return null;
        }
    }
    void Assemble(List<GameObject> x)
    {
        for (int i = 0; i < x.Count; i++)
        {
            Debug.Log(x[i].name);
            x[i].transform.Translate(0, 0, sum);
            sum += 0.01f;
        }
        sum = 0;
    }
    
    
    /*IEnumerator Swapwithplayer()
    {
        ongoing=true;
        pick=true;
        while(ongoing5)
        {
        for(int i =0; i<cardcodes.Count;i++)
            {
            if(Input.GetKeyDown(cardcodes[i]))
                    {
                        listarr[0][i].transform.Rotate(0,180,0);
                        Debug.Log(listarr[0][i].name);
                        //yield return new WaitForSeconds(3f);
                        //listarr[0][i].transform.Rotate(0,-180,0);
                        ongoing=false;
                        pick=false;
                        ongoing5=false;
                    }          
            }
            yield return null;
        }

    }*/

    Vector3 MoveSmoothly(Vector3 start, Vector3 end, float speed)
    {
        Vector3 diff = start - end;
        if (diff.x > 0.0005 || diff.y > 0.0005 || diff.z > 0.0005)
        {
            return Vector3.Lerp(start, end, speed * Time.deltaTime);
        }
        else
        {
            return end;
        }
    }
    private IEnumerator WaitAndAllowClickAgain(float waitTime)
    {
        Debug.Log("Waiting for " + waitTime + " seconds...");
        // Wait for the specified time (0.5 seconds)
        yield return new WaitForSeconds(waitTime);

        // After the wait time, allow the button to be clicked again
        pick = true;
        Debug.Log("Ready for another click!");
    }
}
