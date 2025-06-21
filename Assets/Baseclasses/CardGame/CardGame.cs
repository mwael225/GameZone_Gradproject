using System.Collections.Generic;
using UnityEngine;
using System.Collections;


namespace GameSystem
{
    public class CardGame: Game
    {
        public CardTransformations cardtransformations;
        public List<GameObject> deck;
        public string  gamestate;
        public LinkedList<GameObject> discardpile;
        public List<List<GameObject>> hands;
        public int cplayer,navigatedCardindex = 0;
        public List<int> selectedCardsindex ;
        public GameObject pickedcard;
        protected float discard_pileSpacingZ = 0.0f;
        public List<GameObject> holder;

        public CardGame(string name, int numberOfPlayers, InputHandler inputHandler) : base(name, numberOfPlayers, inputHandler)
        {
            cardtransformations = new CardTransformations();
            if (inputHandler != null)
            {
                Debug.Log("Input handler is not null");
            }

            deck = new List<GameObject>();
            hands = new()
                {
                new List<GameObject> {},new List<GameObject> {},new List<GameObject> {},new List<GameObject> {},
                };
            discardpile = new LinkedList<GameObject>();

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
            while (deck.Count > 0)
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    if (deck.Count != 0)
                    {
                        hands[i].Add(deck[deck.Count - 1]);
                        deck.RemoveAt(deck.Count - 1);
                    }
                }
        
            }
        }

        public virtual GameObject PickCard(int player)
        {
            GameObject card = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            cardtransformations.MovetoPickPostion(card, player);
            return card;    
        }

        public virtual void callsetupposition(){cardtransformations.SetupPosition();}

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
                        cardtransformations.scalecard(hands[currentPlayer][i]);
                        }
                    }
                    else
                    {
                        cardtransformations.scalecard(hands[currentPlayer][i]);
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
                cardtransformations.scalecard(navigatedCard,true);
                }
            }
            else
            {
                GameObject navigatedCard = hands[currentPlayer][navigatedCardindex];
                cardtransformations.scalecard(navigatedCard,true);
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
            cardtransformations.scalecard(card);
            cardtransformations.movetodiscardpile(card,place);
            }
            else 
            {  
            discardpile.AddFirst(card);
            cardtransformations.scalecard(card);
            cardtransformations.movetodiscardpile(card,place);
            }
        }
        public virtual void throwcard(GameObject card,string place="last")
        {
            discardpile.AddLast(card);
            cardtransformations.scalecard(card);
            cardtransformations.movetodiscardpile(card,place);
        }

                
    }
} 