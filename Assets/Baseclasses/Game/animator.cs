using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animator : MonoBehaviour
{
   public List<Animator> animationcontroller;
   public  List<GameObject> players;
    GameObject temp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
        
      public void startme()
        {
        players = new List<GameObject>();
        animationcontroller = new List<Animator>();
        players.Add(GameObject.Find("astraplayable1(Clone)"));
        Transform x = players[0].transform.GetChild(0);
        Debug.Log("name" + x.gameObject.name);
        animationcontroller.Add(x.gameObject.GetComponent<Animator>());
        }

     public IEnumerator clap(int player)
        {
            animationcontroller[player].SetBool("clap",true);
            yield return new WaitForSeconds(2f);
            animationcontroller[player].SetBool("clap", false);
        }

    public IEnumerator Laughing(int player)
    {
        animationcontroller[player].SetBool("laughing", true);
        yield return new WaitForSeconds(2f);
        animationcontroller[player].SetBool("laughing", false);
    }
    public IEnumerator throwing(int player)
        {
        animationcontroller[player].SetBool("throwing", true);
        Debug.Log("entered 1");
        yield return new WaitForSeconds(2f);
        animationcontroller[player].SetBool("throwing", false);
        Debug.Log("courtine working");
        }

  public IEnumerator banging(int player)
        {
            animationcontroller[player].SetBool("banging", true);
            yield return new WaitForSeconds(2f);
           animationcontroller[player].SetBool("banging", false);
        }

      public IEnumerator disbelief(int player)
        {
            animationcontroller[player].SetBool("disbelief", true);
            yield return new WaitForSeconds(2f);
            animationcontroller[player].SetBool("disbelief", true);
        }
    public IEnumerator pointtinglmr(int player, int index, int hand)
    {
        Debug.Log("i am working ");
        if (index < 0.5 * hand)
        {
            animationcontroller[player].SetBool("pointing L", true);
            yield return new WaitForSeconds(2f);
            animationcontroller[player].SetBool("pointing L", false);
        }
        if (index > 0.5 * hand)
        {  
            animationcontroller[player].SetBool("pointing R", true);
            yield return new WaitForSeconds(2f);
            animationcontroller[player].SetBool("pointing R", false);
        }

        if(index == 0.5 * hand)
        {
            animationcontroller[player].SetBool("pointing m", true);
            yield return new WaitForSeconds(2f);
            animationcontroller[player].SetBool("pointing m", false);
        }
    }
        // Update is called once per frame
        void Update()
        {

        }
    }

