using UnityEngine;
using GameSystem;
using System.Collections.Generic;
using System.Threading;
public class Screw :CardGame
{
    string prefabpath = "Prefabs/Screw_CardDeck";
    public Screw() : base("Screw", 4)
    {
    oldscale =new Vector3(150,225.000015f,0.540000081f);
    playerrotations = new List<Vector3>
            {
                new Vector3(0, 0, 0), new Vector3(0, 0, -90), new Vector3(0,0 ,180 ), new Vector3(0, 0, 90)
            };

        setupposition();
        GameObjects=prefabtoGamebojects(prefabpath);
        shuffledeck(GameObjects);
        DealCards(4);
        MovetoPostion();
        Assemble(deck);
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
    public override void setupposition()
    {
            handspostions = new List<List<Vector3>>
            {
                new()
                {
                    new Vector3(0.12f, -2.3f, 0), new Vector3(-1, -2.3f, 0),new Vector3(0.12f, -3.9f, 0), new Vector3(-1, -3.9f, 0),
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
          centralpileLocalpos[0] = new Vector3(-1, 0, 0);
    }


    
}
