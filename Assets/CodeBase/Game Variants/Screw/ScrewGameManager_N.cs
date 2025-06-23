using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Android;
using Unity.VisualScripting;
using Unity.Netcode;
namespace GameSystem    
{
    public class ScrewGameManager : GameManager
    {
        Screw screw;
        int actionnumber = -1;
        bool click = false;
        bool screwdeclared = false;
        int endgamecounter, framecount = 0;
        InputHandler inputHandler;
        GameObject gameMenu;
        List<KeyCode> keyCodes = new List<KeyCode>
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Q,
            KeyCode.W,
            KeyCode.A,
            KeyCode.D,
            KeyCode.Return,
            KeyCode.Alpha9
        };
        GameObject card1, card2;
        public void Start()
        {
            if (!IsServer) return;
            inputHandler = GetComponent<InputHandler>();
            inputHandler.Keymap(keyCodes);
            screw = new Screw(inputHandler);
            currentTurn = firstplayer();
            //arrowdirection();
            //screw.gameState = "start";
            screw.gameState = Screw.GameState.Start;
            StartCoroutine(screw.memorizecard());
            gameMenu = GameObject.Find("GameMenu(Clone)");
            gameMenu.SetActive(false);


        }
        bool x = false;
        public void Update()
        {
            if (!IsServer) return;

            if (inputHandler.GetKeyDown(KeyCode.Alpha9, 0))
            {
                StopAllCoroutines();
                killGame();
                return;
            }
            if (screw.gameState == Screw.GameState.End)
            {
                Debug.Log("game ended");
                return;
            }
            if (endgamecounter == 4)
            {
                screw.gameState = Screw.GameState.End;
                screw.scoresheet();
            }

            currentTurn = NextTurn(screw.numberOfPlayers);
            framecount++;
            if (framecount % 120 == 0)
            {
                framecount = 0;
                Debug.Log(screw.gameState);
            }
            if (screw.gameState != Screw.GameState.Seeothercard && screw.gameState != Screw.GameState.Choosing2 && screw.gameState != Screw.GameState.Lookaround)
            {
                StartCoroutine(screw.navigatedCards(currentTurn));
            }
            if (screw.gameState == Screw.GameState.Seeothercard || screw.gameState == Screw.GameState.Choosing1 || screw.gameState == Screw.GameState.Choosing2 || screw.gameState == Screw.GameState.Lookaround)
            {
                actioncall(currentTurn, actionnumber);
            }
            else if (screw.gameState == Screw.GameState.Action || screw.gameState == Screw.GameState.Swapping1 || screw.gameState == Screw.GameState.Picking || inputHandler.GetKeyDown(KeyCode.Alpha1, currentTurn))
            {
                if (screw.gameState == Screw.GameState.Normal || screw.gameState == Screw.GameState.Start)
                {
                    screw.gameState = Screw.GameState.Picking;
                }
                if (screw.pickedcard == null)
                {
                    if (screw.deck.Count != 0)
                    {
                        screw.pickedcard = screw.PickCard(currentTurn);
                    }
                    else
                    {
                        screw.gameState = Screw.GameState.End;
                        screw.scoresheet();
                    }
                }
                else
                {
                    if (framecount % 120 == 0)
                        Debug.Log("you already picked a card");
                }
                if (!click)
                    StartCoroutine(WaitAndAllowClickAgain(0.5f));
                if (click)
                {
                    pickingcard();
                    if (screw.pickedcard == null)
                        click = false;
                }
            }
            else if (inputHandler.GetKeyDown(KeyCode.Alpha2, currentTurn))
            {
                screw.swapwdiscardpile(currentTurn);
            }
            else if (screw.gameState == Screw.GameState.Basra || screw.gameState == Screw.GameState.Matching || inputHandler.GetKeyDown(KeyCode.Alpha3, currentTurn))
            {
                if (screw.gameState == Screw.GameState.Basra)
                {
                    if (inputHandler.GetKeyDown(KeyCode.Return, currentTurn))
                    {
                        matching();
                    }
                }
                else
                {
                    matching();
                }
            }
            else if (inputHandler.GetKeyDown(KeyCode.Alpha4, currentTurn))
            {
                screwdeclared = true;
                screw.gameState = Screw.GameState.NextTurn;
            }
        }
        public void pickingcard()
        {
            if (screw.gameState == Screw.GameState.Swapping1 || inputHandler.GetKeyDown(KeyCode.Alpha1, currentTurn))
            {
                screw.gameState = Screw.GameState.Swapping1;
                Debug.Log("swaped");
                screw.swapwpickedcard(currentTurn);
            }
            if (screw.gameState == Screw.GameState.Action || inputHandler.GetKeyDown(KeyCode.Alpha2, currentTurn))
            {
                if (screw.gameState != Screw.GameState.Action)
                {
                    screw.throwcard(screw.pickedcard);
                    actionnumber = isaction();
                }
                if (screw.gameState == Screw.GameState.Action || actionnumber != -1)
                {
                    screw.gameState = Screw.GameState.Action;
                    actioncall(currentTurn, actionnumber);
                }
                else
                {
                    screw.pickedcard = null;
                    screw.gameState = Screw.GameState.NextTurn;
                }
            }
        }
        public int isaction()
        {
            for (int i = 0; i < screw.specialcardsnames.Length; i++)
            {
                if (screw.discardpile.Last.Value.name.Split(" ")[0] == screw.specialcardsnames[i])
                {
                    Debug.Log("action card name: " + screw.discardpile.Last.Value.name);
                    return i;
                }
            }
            Debug.Log("not a action card : " + screw.discardpile.Last.Value.name);
            return -1;
        }
        private IEnumerator WaitAndAllowClickAgain(float waitTime)
        {
            Debug.Log("Waiting for " + waitTime + " seconds...");
            yield return new WaitForSeconds(waitTime);
            click = true;
            Debug.Log("Ready for another click!");
        }
        public void actioncall(int player, int number)
        {
            if (number == 0 || number == 1)
            {
                StartCoroutine(screw.seeurcard(player));
            }
            else if (number == 2 || number == 3)
            {
                screw.gameState = Screw.GameState.Seeothercard;
                if (inputHandler.GetKeyDown(KeyCode.Return, player))
                {
                    StartCoroutine(screw.seeotherscard(player));
                }
                StartCoroutine(screw.navigateplayers(player));
            }
            else if (number == 4)
            {
                screw.gameState = Screw.GameState.Basra;
            }
            else if (number == 5)
            {
                Debug.Log("lookaround");
                screw.gameState = Screw.GameState.Lookaround;
                StartCoroutine(screw.lookaround(player));
            }
            else
            {
                swapwplayer();
            }
        }
        public void matching()
        {
            if (screw.gameState == Screw.GameState.Basra)
            {
                Debug.Log("basra");
            }
            screw.match(currentTurn);
            UpdatecardPositions();

        }
        public override int NextTurn(int noOfPlayers)
        {
            if (screw.gameState == Screw.GameState.NextTurn)
            {
                screw.gameState = Screw.GameState.Normal;
                screw.cardtransformations.scalecard(screw.hands[currentTurn][screw.navigatedCardindex]);
                screw.navigatedCardindex = 0;
                if (screwdeclared)
                {
                    endgamecounter++;
                }
                currentTurn = (currentTurn + 1) % noOfPlayers;
                //arrowdirection();
                return currentTurn;
            }
            return currentTurn;
        }
        public void swapwplayer()
        {
            if (screw.gameState == Screw.GameState.Choosing2)
            {
                StartCoroutine(screw.navigateplayers(currentTurn));
            }
            if (screw.gameState != Screw.GameState.Choosing2)
            {
                screw.gameState = Screw.GameState.Choosing1;
            }
            if (screw.gameState == Screw.GameState.Choosing1 && inputHandler.GetKeyDown(KeyCode.Return, currentTurn))
            {
                card1 = screw.hands[currentTurn][screw.navigatedCardindex];
                screw.gameState = Screw.GameState.Choosing2;
            }
            else if (screw.gameState == Screw.GameState.Choosing2 && inputHandler.GetKeyDown(KeyCode.Return, currentTurn))
            {
                card2 = screw.hands[currentTurn][screw.navigatedCardindex];
                StopCoroutine(screw.navigateplayers(currentTurn));
                screw.hands[currentTurn][screw.navigatedCardindex] = screw.hands[screw.naviagedplayerindex][screw.navigatedplayercard];
                screw.hands[screw.naviagedplayerindex][screw.navigatedplayercard] = card1;
                screw.cardtransformations.moveandrotate(screw.hands, currentTurn, screw.navigatedCardindex, screw.cardtransformations.handspostions, screw.cardtransformations.playerRotations);
                screw.cardtransformations.moveandrotate(screw.hands, screw.naviagedplayerindex, screw.navigatedplayercard, screw.cardtransformations.handspostions, screw.cardtransformations.playerRotations);
                screw.gameState = Screw.GameState.NextTurn;
                screw.pickedcard = null;
            }
        }
        public void UpdatecardPositions()
        {
            screw.cardtransformations.MovetoPostion(screw.hands);
        }
        public override void killGame()
        {
            //screw.origin.GetComponent<NetworkObject>().Despawn(true);
            Debug.Log(screw.holder.Count);
            for (int i = 0; i < screw.holder.Count; i++)
            {
                Debug.Log("name : " + screw.holder[i].name);
                screw.holder[i].GetComponent<NetworkObject>().Despawn(true);
            }
            gameObject.GetComponent<NetworkObject>().Despawn(true);
            gameMenu.SetActive(true);
        }
        
    }
}
