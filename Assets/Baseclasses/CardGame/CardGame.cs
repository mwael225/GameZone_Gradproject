using System.Collections.Generic;
using UnityEngine;
using System.Collections;


namespace GameSystem
{
    public class CardGame: Game
    {
        
        public List<Vector3> playerRotations,cardSpacing ,pickPosition,pickRotation,discard_pileSpacing,centralpileLocalpos ; 
        public List<GameObject> deck;
        public string  gamestate;
        public LinkedList<GameObject> discardpile;
        public List<List<Vector3>> handspostions ;
        public List<List<GameObject>> hands;
        public Vector3 oldscale,discardpileRotation ;
        public int cplayer,navigatedCardindex = 0;
        public List<int> selectedCardsindex ;
        public GameObject pickedcard;
        protected float discard_pileSpacingZ = 0.0f;

        public CardGame(string name, int numberOfPlayers,InputHandler inputHandler) : base(name, numberOfPlayers,inputHandler)
        {
            if(inputHandler!=null)
            {
                Debug.Log("Input handler is not null");
            }
            
            deck = new List<GameObject>();
            hands =new()
                {
                new List<GameObject> {},new List<GameObject> {},new List<GameObject> {},new List<GameObject> {},
                };
            discardpile = new LinkedList<GameObject>();
            discard_pileSpacing= new List<Vector3>();
            
        }

        protected virtual void Assemble(List<GameObject> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                deck[i].transform.localPosition = new Vector3(0, 0,discard_pileSpacingZ);
                discard_pileSpacingZ += 0.005f;
            }
            discard_pileSpacingZ = 0;
        }


        public virtual void shuffledeck(List<GameObject> cards)
        {
            System.Random rand = new System.Random();
            while (cards.Count > 0)
            {
                int x = cards.Count;
                int randomIndex = rand.Next(0, x);
                GameObject temp = cards[randomIndex];
                cards.RemoveAt(randomIndex);    
                deck.Add(temp);
            }

        }
        
        public virtual void DealCards(int numberofcards)
        {
            
            for (int i = 0; i < numberOfPlayers; i++)
            {
                for (int j = 0; j <numberofcards; j++)
                {
                    hands[i].Add(deck[deck.Count - 1]);
                    deck.RemoveAt(deck.Count - 1);  
                }
            }
        }
        public virtual void DealCards()
        {
            while(deck.Count>0)
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    if(deck.Count!=0)
                    {
                    hands[i].Add(deck[deck.Count - 1]);
                    deck.RemoveAt(deck.Count - 1);  
                    }
                }
            }
        }
        protected virtual void MovetoPostion()
        {
            for (int i = 0; i < hands.Count; i++)
            {
                for (int j = 0; j < hands[i].Count; j++)
                {
                    
                //hands[i][j].transform.Rotate(playerrotations[i]);
                hands[i][j].transform.localRotation=Quaternion.Euler(playerRotations[i]);
                hands[i][j].transform.localPosition = handspostions[i][j];
                
                }
        }
        }

        public virtual GameObject PickCard(int player)
        {
            GameObject card = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            card.transform.localPosition = pickPosition[player];
            card.transform.localRotation = Quaternion.Euler(pickRotation[player]);
            return card;    
        }

        public virtual void setupposition(){}

        public virtual IEnumerator navigatedCards(int currentPlayer=0)
        {
            
            if (gamestate == "end")
            {
                yield break;
            }
            for(int i =0;i<hands[currentPlayer].Count;i++)
                {
                    
                    if(selectedCardsindex!=null)
                    {
                        if(!selectedCardsindex.Contains(i))
                        {
                        hands[currentPlayer][i].transform.localScale = oldscale;
                        }
                    }
                    else
                    {
                        hands[currentPlayer][i].transform.localScale = oldscale;
                    }
                }
            
            if (inputHandler.GetKeyDown(KeyCode.Q,currentPlayer))
                {
                    navigatedCardindex =(navigatedCardindex - 1+hands[currentPlayer].Count )% hands[currentPlayer].Count;
                }
            else if (inputHandler.GetKeyDown(KeyCode.W,currentPlayer))
                {
                    navigatedCardindex = (navigatedCardindex + 1) % hands[currentPlayer].Count;
                }
            if(selectedCardsindex!=null)
            {
                if(!selectedCardsindex.Contains(navigatedCardindex))
                {
                GameObject navigatedCard = hands[currentPlayer][navigatedCardindex];
                navigatedCard.transform.localScale = oldscale * 1.2f;
                }
            }
            else
            {
                GameObject navigatedCard = hands[currentPlayer][navigatedCardindex];
                navigatedCard.transform.localScale = oldscale * 1.2f;
            }
            yield return null;
        }
        public virtual void throwCard(int player, int cardIndex,string place="last")
        {
            if (cardIndex < 0 || cardIndex >= hands[player].Count)
            {
                Debug.LogWarning("Invalid card index!");
                return;
            }
            GameObject card = hands[player][cardIndex];
            hands[player].RemoveAt(cardIndex);
            if(place =="last")
            {
            discardpile.AddLast(card);
            card.transform.localScale = oldscale;
            card.transform.localPosition = centralpileLocalpos[0];
            centralpileLocalpos[0]+=discard_pileSpacing[0];
            }
            else 
            {  
            discardpile.AddFirst(card);
            card.transform.localScale = oldscale;
            centralpileLocalpos[1]-=discard_pileSpacing[1];
            card.transform.localPosition = centralpileLocalpos[1];
            }
            card.transform.localRotation = Quaternion.Euler(discardpileRotation);
        }
        public virtual void throwcard(GameObject card,string place="last")
        {
            discardpile.AddLast(card);
            card.transform.localScale = oldscale;
            centralpileLocalpos[0]+=discard_pileSpacing[0];
            card.transform.localPosition = centralpileLocalpos[0];
            Debug.Log("discard pile last is "+discardpile.Last.Value.name);
            card.transform.localRotation = Quaternion.Euler(discardpileRotation);
        }
        public virtual int Zangles()
        {
            System.Random random = new System.Random();
            int number=random.Next(-15, 15);
            return number;
        }
                
    }
} 