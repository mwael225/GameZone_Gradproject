using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System;


namespace GameSystem
{
    public class CardTransformations
    {
        public List<Vector3> playerRotations, cardSpacing, pickPosition, pickRotation, discard_pileSpacing, centralpileLocalpos;
        public List<List<Vector3>> handspostions;
        public Vector3 oldscale, discardpileRotation;
        protected float discard_pileSpacingZ = 0.0f;
        public CardTransformations()
        {
            discard_pileSpacing = new List<Vector3>();
            pickPosition = new List<Vector3>();
            pickRotation = new List<Vector3>();
            playerRotations = new List<Vector3>();
            handspostions = new List<List<Vector3>>();
            cardSpacing = new List<Vector3>();
            centralpileLocalpos = new List<Vector3>();
            oldscale = new Vector3(1, 1, 1);
            discardpileRotation = new Vector3(0, 0, 0);
        }
        public virtual void SetupPosition() { }
        public virtual void MovetoPostion(List<List<GameObject>> hands)
        {
            for (int i = 0; i < hands.Count; i++)
            {
                for (int j = 0; j < hands[i].Count; j++)
                {
                    //hands[i][j].transform.Rotate(playerrotations[i]);
                    hands[i][j].transform.localRotation = Quaternion.Euler(playerRotations[i]);
                    hands[i][j].transform.localPosition = handspostions[i][j];
                }
            }
        }
        public virtual void MovetoPickPostion(GameObject card, int player)
        {
            card.transform.localPosition = pickPosition[player];
            card.transform.localRotation = Quaternion.Euler(pickRotation[player]);
        }
        public virtual void scalecard(GameObject card, bool enlarge = false)
        {
            if (enlarge)
            {
                card.transform.localScale = oldscale * 1.2f;
            }
            else
            {
                card.transform.localScale = oldscale;
            }
        }
        public virtual void Assemble(List<GameObject> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                deck[i].transform.localPosition = new Vector3(0, 0, discard_pileSpacingZ);
                discard_pileSpacingZ += 0.005f;
            }
            discard_pileSpacingZ = 0;
        }
        public virtual int Zangles()
        {
            System.Random random = new System.Random();
            int number = random.Next(-15, 15);
            return number;
        }
        public virtual void movetodiscardpile(GameObject card, string place = "last")
        {
            if (place == "last")
            {
                centralpileLocalpos[0] += discard_pileSpacing[0];
                card.transform.localPosition = centralpileLocalpos[0];
            }
            else
            {
                centralpileLocalpos[1] -= discard_pileSpacing[1];
                card.transform.localPosition = centralpileLocalpos[1];
            }
            card.transform.localRotation = Quaternion.Euler(discardpileRotation);
        }

        public virtual void moveandrotate(List<List<GameObject>> hands, int player, int naviagtedcard, List<Vector3> positions, List<Vector3> rotations)
        {
            hands[player][naviagtedcard].transform.localPosition = positions[player];
            hands[player][naviagtedcard].transform.localRotation = Quaternion.Euler(rotations[player]);
        }
        public virtual void moveandrotate(List<List<GameObject>> hands, int player, int naviagtedcard, List<List<Vector3>> positions, List<Vector3> rotations)
        {
            hands[player][naviagtedcard].transform.localPosition = positions[player][naviagtedcard];
            hands[player][naviagtedcard].transform.localRotation = Quaternion.Euler(rotations[player]);
        }
        public virtual void moveandrotate(GameObject card, List<Vector3> positions, List<Vector3> rotations, int index)
        {
            card.transform.localPosition = positions[index];
            card.transform.localRotation = Quaternion.Euler(rotations[index]);
        }

        
        
        
        
        
    }
}