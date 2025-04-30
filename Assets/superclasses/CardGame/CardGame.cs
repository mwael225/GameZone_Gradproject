using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;


namespace GameSystem
{
    public class CardGame : Game
    {
        public List<Vector3> playerrotations,cardSpacing ,pickposition,pickrotation ; 
        protected List<Vector3> discard_pilespcaing;
        public List<GameObject> deck;
        public string  gamestate;
        public LinkedList<GameObject> centralPile;
        public List<List<Vector3>> handspostions ;
        public List<List<GameObject>> hands;
        public Vector3 oldscale,discardpileRotation ;
        protected List<Vector3> centralpileLocalpos ;
        public int cplayer,navigatedCardindex = 0;
        public List<int> selectedCardsindex ;

        public GameObject pickedcard;

        protected float sum = 0.0f;

        public CardGame(string name, int numberOfPlayers) : base(name, numberOfPlayers)
        {
            deck = new List<GameObject>();
            hands =new()
                {
                new List<GameObject> {},new List<GameObject> {},new List<GameObject> {},new List<GameObject> {},
                };
            centralPile = new LinkedList<GameObject>();
            discard_pilespcaing= new List<Vector3>();

        }

        protected virtual void Assemble(List<GameObject> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                deck[i].transform.localPosition = new Vector3(0, 0, sum);
                sum += 0.005f;
                
            }
            sum = 0;
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
                hands[i][j].transform.localRotation=Quaternion.Euler(playerrotations[i]);
                hands[i][j].transform.localPosition = handspostions[i][j];
                }
        }
        }

        public GameObject PickCard(int player)
        {
            if (deck.Count == 0)
            {
                Debug.LogWarning("Deck is empty!");
            }

            GameObject card = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            card.transform.localPosition = pickposition[player];
            card.transform.localRotation = Quaternion.Euler(pickrotation[player]);
            return card;
        }

        public virtual void setupposition(){}

        public virtual void discardpileposition()
        {}

        public virtual IEnumerator navigatedCards(int currentPlayer=0)
        {
            while (gamestate == "seeothercard"||gamestate=="choosing2")
            {
                //hands[currentPlayer][navigatedCardindex].transform.localScale = oldscale;
                //hands[currentPlayer][navigatedCardindex].GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("naviagted card is now paused");
                yield return null;
            }
            if(gamestate=="end")
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
                        hands[currentPlayer][i].GetComponent<Renderer>().material.color = Color.white;
                        }
                    }
                    else
                    {
                        hands[currentPlayer][i].transform.localScale = oldscale;
                        hands[currentPlayer][i].GetComponent<Renderer>().material.color = Color.white;
                    }
                }
            
            if (Input.GetKeyDown(KeyCode.Q))
                {
                    navigatedCardindex =(navigatedCardindex - 1+hands[currentPlayer].Count )% hands[currentPlayer].Count;
                }
            else if (Input.GetKeyDown(KeyCode.W))
                {
                    navigatedCardindex = (navigatedCardindex + 1) % hands[currentPlayer].Count;
                }
            if(selectedCardsindex!=null)
            {
                //Debug.Log("selectedCardsindex!=null");
                if(!selectedCardsindex.Contains(navigatedCardindex))
                {
                GameObject navigatedCard = hands[currentPlayer][navigatedCardindex];
                navigatedCard.transform.localScale = oldscale * 1.2f;
                navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
                }
            }
            else
            {
                //Debug.Log("selectedCardsindex==null");
                GameObject navigatedCard = hands[currentPlayer][navigatedCardindex];
                navigatedCard.transform.localScale = oldscale * 1.2f;
                navigatedCard.GetComponent<Renderer>().material.color = Color.cyan;
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
            centralPile.AddLast(card);
            card.transform.localScale = oldscale;
            card.GetComponent<Renderer>().material.color = Color.white;
            card.transform.localPosition = centralpileLocalpos[0];
            centralpileLocalpos[0]+=discard_pilespcaing[0];
            }
            else 
            {  
            centralPile.AddFirst(card);
            card.transform.localScale = oldscale;
            card.GetComponent<Renderer>().material.color = Color.white;
            centralpileLocalpos[1]-=discard_pilespcaing[1];
            card.transform.localPosition = centralpileLocalpos[1];
            }
            card.transform.localRotation = Quaternion.Euler(discardpileRotation);
        }
        public virtual void throwcard(GameObject card,string place="last")
        {
            //if(place =="last")
            
            centralPile.AddLast(card);
            card.transform.localScale = oldscale;
            card.GetComponent<Renderer>().material.color = Color.white;
            centralpileLocalpos[0]+=discard_pilespcaing[0];
            card.transform.localPosition = centralpileLocalpos[0];
            Debug.Log("central pile last is "+centralPile.Last.Value.name);
            
            /*else 
            {  
            centralPile.AddFirst(card);
            card.transform.localScale = oldscale;
            card.GetComponent<Renderer>().material.color = Color.white;
            centralpileLocalpos[1]-=discard_pilespcaing[1];
            card.transform.localPosition = centralpileLocalpos[1];
            }*/
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