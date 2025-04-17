using UnityEngine;

namespace GameSystem
{
    public class Card
    {
        private string name;
        private GameObject cardObj;
        private CardType type;

        public enum CardType
        {
            Normal,
            Action
        }

        public Card(string name, GameObject cardObj, CardType type)
        {
            this.name = name;
            this.cardObj = cardObj;
            this.type = type;
        }


        public virtual void SpecialAction()
        {}

        public string Name => name;
        public GameObject CardObj => cardObj;
        public CardType Type => type;
    }
} 