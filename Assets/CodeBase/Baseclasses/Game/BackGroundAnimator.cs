using System.Collections;
using UnityEngine;

public class BackGroundAnimator : MonoBehaviour
{
    public Animator animationcontroller;

    public int GetRandomNumber()
    {
        return Random.Range(1, 5); 
    }

    public IEnumerator clap()
    {
        animationcontroller.SetBool("clap", true);
        yield return new WaitForSeconds(3f);
        animationcontroller.SetBool("clap", false);
    }

    public IEnumerator Laughing()
    {
        animationcontroller.SetBool("Laughing", true);
        yield return new WaitForSeconds(3f);
        animationcontroller.SetBool("Laughing", false);
    }

    public IEnumerator disbelief()
    {
        animationcontroller.SetBool("disbelief", true);
        yield return new WaitForSeconds(3f);
        animationcontroller.SetBool("disbelief", false);
    }

    public IEnumerator banging()
    {
        animationcontroller.SetBool("banging", true);
        yield return new WaitForSeconds(3f);
        animationcontroller.SetBool("banging", false);
    }

    private IEnumerator PlayRandomAnimations()
    {
        while (true)
        {
            int randomNumber = GetRandomNumber();
            Debug.Log("Random number: " + randomNumber);

            if (randomNumber == 1)
            {
                yield return StartCoroutine(clap());
            }
            else if (randomNumber == 2)
            {
                yield return StartCoroutine(banging());
            }
            else if (randomNumber == 3)
            {
                yield return StartCoroutine(Laughing());
            }
            else if (randomNumber == 4)
            {
                yield return StartCoroutine(disbelief());
            }

            yield return new WaitForSeconds(5f); 
        }
    }

    void Start()
    {
        animationcontroller = GetComponent<Animator>();
        StartCoroutine(PlayRandomAnimations());
    }
}
