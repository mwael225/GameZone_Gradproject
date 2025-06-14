using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
public class Screw: CardGame
{
    string prefabpath = "ScrewDeck";
    public string[] specialcardsnames = { "Card_Seven", "Card_Eight", "Card_Nine", "Card_Ten", "Card_Match", "Card_Lookaround", "Card_Swap" };

    public Dictionary<string, int> cardscores = new Dictionary<string, int>
    {
        {"Card_One", 1},{"Card_Two", 2},{"Card_Three", 3},{"Card_Four", 4},
        {"Card_Five", 5},{"Card_Six", 6},{"Card_Seven", 7},{"Card_Eight", 8},
        {"Card_Nine", 9},{"Card_Ten", 10},{"Card_Match", 10},{"Card_Lookaround", 10},
        {"Card_Swap", 10},{"Card_RedScrew", 25},{"Card_GreenScrew", 0},{"Card_Plus20", 20},
        {"Card_negative1", -1}
    };
    public int naviagedplayerindex, navigatedplayercard, lookaroundcounter = 0;
    bool lookedaround = false;
    public int lookaroundplayercounter = 0;
    GameObject origin;

    public enum GameState
    {
        Start, Normal, Choosing1, Choosing2, Swapping1, Swapping2, Picking,
        Action, Basra, Matching, Seeothercard, Swapwplayer, Lookaround, End, NextTurn
    }
    public GameState gameState;

    public Screw(InputHandler inputHandler) : base("Screw", 4, inputHandler)
    {
        gameState = new GameState();
        centralpileLocalpos = new List<Vector3>();
        pickRotation = new List<Vector3>
        {
            new Vector3(-120, 0, 0), new Vector3(0, 120, -90), new Vector3(120,0 ,180 ), new Vector3(0, -120, 90)
        };
        discardpileRotation = new Vector3(0, 180, 180);
        oldscale = new Vector3(150, 225, 0.540000081f);
        playerRotations = new List<Vector3>
        {
            new Vector3(0, 0, 180), new Vector3(0, 0, 90), new Vector3(0,0 ,0), new Vector3(0, 0, -90)
        };
        discard_pileSpacing = new List<Vector3> { new Vector3(0, 0, 0.005f) };
        origin = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs_N/Screw_CardDeck_N"));
        origin.GetComponent<NetworkObject>().Spawn();
        GameObjects = spawnobjects(prefabpath);
        for (int i = 0; i < GameObjects.Count; i++)
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
        for (int i = 0; i < hands.Count; i++)
        {
            hands[i][2].transform.Translate(0, 0, 1f);
            hands[i][2].transform.localRotation = Quaternion.Euler(pickRotation[i]);
            hands[i][3].transform.Translate(0, 0, 1f);
            hands[i][3].transform.localRotation = Quaternion.Euler(pickRotation[i]);
        }
        yield return new WaitForSeconds(10f);
        for (int i = 0; i < hands.Count; i++)
        {
            hands[i][2].transform.localPosition = handspostions[i][2];
            hands[i][2].transform.localRotation = Quaternion.Euler(playerRotations[i]);
            hands[i][3].transform.localPosition = handspostions[i][3];
            hands[i][3].transform.localRotation = Quaternion.Euler(playerRotations[i]);
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
                    new Vector3(-2.9f, -0.6f, 0f),new Vector3(-2.9f, 0.5f, 0f),new Vector3(-4.5f, -0.6f, 0f),new Vector3(-4.5f, 0.5f, 0f),new Vector3(-5.6f,-0.6f,0f),new Vector3(-5.6f,0.5f,0f)
                },
                new()
                {
                    new Vector3(0.12f, 2.3f, 0),new Vector3(-1, 2.3f, 0),new Vector3(0.12f, 3.9f, 0),new Vector3(-1, 3.9f, 0), new Vector3(-2.1f,2.3f,0),new Vector3(-2.1f,3.9f,0)
                },
                new()
                {
                    new Vector3(2f, -0.6f, 0f),new Vector3(2f, 0.5f, 0f),new Vector3(3.5f, -0.6f, 0f),new Vector3(3.5f, 0.5f, 0f), new Vector3(4.6f,-0.6f,0f),new Vector3(4.6f,0.5f,0f)
                },
          };
        centralpileLocalpos.Add(new Vector3(-1, 0, 0));
        pickPosition = new List<Vector3>
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
        discardpile.AddLast(deck[deck.Count - 1]);
        deck.RemoveAt(deck.Count - 1);
    }
    protected override void MovetoPostion()
    {
        base.MovetoPostion();
        discardpile.First.Value.transform.localPosition = centralpileLocalpos[0];
        discardpile.First.Value.transform.Rotate(0, 180, 0);
    }
    public void swapwpickedcard(int player)
    {
        Debug.Log("entered function");
        GameObject temp = hands[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localScale = oldscale;
        hands[player][navigatedCardindex] = pickedcard;
        hands[player][navigatedCardindex].transform.localPosition = handspostions[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localRotation = Quaternion.Euler(playerRotations[player]);
        throwcard(temp);
        pickedcard = null;
        gameState = GameState.NextTurn;

    }
    public void swapwdiscardpile(int player)
    {
        GameObject card = hands[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localScale = oldscale;
        hands[player][navigatedCardindex] = discardpile.Last.Value;
        hands[player][navigatedCardindex].transform.localPosition = handspostions[player][navigatedCardindex];
        hands[player][navigatedCardindex].transform.localRotation = Quaternion.Euler(playerRotations[player]);
        discardpile.RemoveLast();
        centralpileLocalpos[0] -= discard_pileSpacing[0];
        throwcard(card);
        gameState = GameState.NextTurn;
    }
    public void match(int player)
    {
        if (gameState == GameState.Basra)
        {
            hands[player][navigatedCardindex].transform.localScale = oldscale;
            throwCard(player, navigatedCardindex);
            pickedcard = null;
            gameState = GameState.NextTurn;
            navigatedCardindex = 0;
        }
        else
        {
            if (hands[player][navigatedCardindex].name.Split(" ")[0] == discardpile.Last.Value.name.Split(" ")[0])
            {
                Debug.Log("they match");
                hands[player][navigatedCardindex].transform.localScale = oldscale;
                throwCard(player, navigatedCardindex);
                gameState = GameState.NextTurn; 
                navigatedCardindex = 0;
            }
            else
            {
                Debug.Log("they don't match");
                hands[player].Add(discardpile.Last.Value);
                hands[player][hands[player].Count - 1].transform.localPosition = handspostions[player][hands[player].Count - 1];
                hands[player][hands[player].Count - 1].transform.localRotation = Quaternion.Euler(playerRotations[player]);
                discardpile.RemoveLast();
                gameState = GameState.NextTurn;
            }
        }
    }

    public IEnumerator seeurcard(int player)
    {
        if (inputHandler.GetKeyDown(KeyCode.Return, player))
        {
            hands[player][navigatedCardindex].transform.localScale = oldscale;
            hands[player][navigatedCardindex].transform.localPosition = pickPosition[player];
            hands[player][navigatedCardindex].transform.localRotation = Quaternion.Euler(pickRotation[player]);
            yield return new WaitForSeconds(2f);
            hands[player][navigatedCardindex].transform.localPosition = handspostions[player][navigatedCardindex];
            hands[player][navigatedCardindex].transform.localRotation = Quaternion.Euler(playerRotations[player]);
            pickedcard = null;
            gameState = GameState.NextTurn;
        }
        yield return null;
    }
    public IEnumerator seeotherscard(int player)
    {
        hands[naviagedplayerindex][navigatedplayercard].transform.localPosition = pickPosition[player];
        hands[naviagedplayerindex][navigatedplayercard].transform.localRotation = Quaternion.Euler(pickRotation[player]);
        yield return new WaitForSeconds(2f);
        hands[naviagedplayerindex][navigatedplayercard].transform.localPosition = handspostions[naviagedplayerindex][navigatedplayercard];
        hands[naviagedplayerindex][navigatedplayercard].transform.localRotation = Quaternion.Euler(playerRotations[naviagedplayerindex]);
        gameState = GameState.NextTurn;
        firsttime = true;
        pickedcard = null;
    }
    public IEnumerator lookaround(int player)
    {
        if (lookedaround == true)
        {
            lookaroundcounter = 0;
            gameState = GameState.NextTurn;
            pickedcard = null;
            lookaroundcounter = 0;
            lookedaround = false;
            yield break;
        }
        lookaroundplayercounter = (player + lookaroundcounter) % numberOfPlayers;

        for (int i = 0; i < hands[lookaroundplayercounter].Count; i++)
        {
            if (i != navigatedplayercard)
                hands[lookaroundplayercounter][i].transform.localScale = oldscale;
        }
        if (inputHandler.GetKeyDown(KeyCode.Q, player))
        {
            navigatedplayercard = (navigatedplayercard - 1 + hands[lookaroundplayercounter].Count) % hands[lookaroundplayercounter].Count;
        }
        else if (inputHandler.GetKeyDown(KeyCode.W, player))
        {
            navigatedplayercard = (navigatedplayercard + 1) % hands[lookaroundplayercounter].Count;
        }
        if (inputHandler.GetKeyDown(KeyCode.Return, player))
        {
            hands[lookaroundplayercounter][navigatedplayercard].transform.localPosition = pickPosition[player];
            hands[lookaroundplayercounter][navigatedplayercard].transform.localRotation = Quaternion.Euler(pickRotation[player]);
            yield return new WaitForSeconds(2f);
            hands[lookaroundplayercounter][navigatedplayercard].transform.localPosition = handspostions[lookaroundplayercounter][navigatedplayercard];
            hands[lookaroundplayercounter][navigatedplayercard].transform.localRotation = Quaternion.Euler(playerRotations[lookaroundplayercounter]);
            lookaroundcounter++;
            navigatedplayercard = 0;
            Debug.Log("lookaroundcounter" + lookaroundcounter);
        }
        Debug.Log("lookaroundplayercounter" + lookaroundplayercounter);
        GameObject navigatedCard = hands[lookaroundplayercounter][navigatedplayercard];
        navigatedCard.transform.localScale = oldscale * 1.2f;
        if (lookaroundcounter == 4)
        {
            lookedaround = true;
        }
        yield return null;
    }
    bool firsttime = true;
    public IEnumerator navigateplayers(int player)
    {
        if (firsttime)
        {
            naviagedplayerindex = (player + 1) % numberOfPlayers;
            firsttime = false;
        }
        Debug.Log("entered navigateplayers function");
        for (int i = 0; i < hands[naviagedplayerindex].Count; i++)
        {
            if (i != navigatedplayercard)
            {
                hands[naviagedplayerindex][i].transform.localScale = oldscale;
            }
        }
        if (inputHandler.GetKeyDown(KeyCode.A, player))
        {
            hands[naviagedplayerindex][navigatedplayercard].transform.localScale = oldscale;
            navigatedplayercard = 0;
            naviagedplayerindex = ((naviagedplayerindex - 1 + hands.Count) % numberOfPlayers) != player ? (naviagedplayerindex - 1 + hands.Count) % numberOfPlayers : (naviagedplayerindex - 2 + hands.Count) % numberOfPlayers;
        }
        else if (inputHandler.GetKeyDown(KeyCode.D, player))
        {
            hands[naviagedplayerindex][navigatedplayercard].transform.localScale = oldscale;
            navigatedplayercard = 0;
            naviagedplayerindex = ((naviagedplayerindex + 1) % numberOfPlayers) != player ? (naviagedplayerindex + 1) % numberOfPlayers : (naviagedplayerindex + 2) % numberOfPlayers;
        }
        if (inputHandler.GetKeyDown(KeyCode.Q, player))
        {
            Debug.Log("i am working");
            navigatedplayercard = (navigatedplayercard - 1 + hands[naviagedplayerindex].Count) % hands[naviagedplayerindex].Count;
        }
        else if (inputHandler.GetKeyDown(KeyCode.W, player))
        {
            Debug.Log("i am working");
            navigatedplayercard = (navigatedplayercard + 1) % hands[naviagedplayerindex].Count;
        }
        Debug.Log("navigatedplayerindex: " + naviagedplayerindex + " navigatedplayercard: " + navigatedplayercard);
        GameObject navigatedCard = hands[naviagedplayerindex][navigatedplayercard];
        navigatedCard.transform.localScale = oldscale * 1.2f;
        yield return null;
    }
    public void scoresheet()
    {
        int[] scores = { 0, 0, 0, 0 };
        for (int i = 0; i < hands.Count; i++)
        {
            for (int j = 0; j < hands[i].Count; j++)
            {
                Debug.Log("player " + i + " card " + hands[i][j].name);
                scores[i] += cardscores[hands[i][j].name.TrimEnd()];
            }
        }
        for (int i = 0; i < scores.Length; i++)
        {

            Debug.Log("player " + i + " score: " + scores[i]);
        }
        showcards();
    }
    public void showcards()
    {
        for (int i = 0; i < hands.Count; i++)
        {
            for (int j = 0; j < hands[i].Count; j++)
            {
                hands[i][j].transform.localScale = oldscale;
                hands[i][j].transform.Rotate(0, 180, 0);
            }
        }
    }
    public void restock()
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < discardpile.Count; i++)
        {
            discardpile.Last.Value.transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
            list.Add(discardpile.Last.Value);
            discardpile.RemoveLast();
        }
        centralpileLocalpos[0]=new Vector3(-1, 0, 0);
        shuffledeck(list);
        Assemble(deck);
    }
}
