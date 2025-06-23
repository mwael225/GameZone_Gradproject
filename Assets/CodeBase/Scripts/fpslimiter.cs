using UnityEngine;

public class fpslimiter : MonoBehaviour
{

    // Set your desired FPS limit here
    public int targetFPS = 1;

    void Start()
    {
        // Set the target frame rate
        Application.targetFrameRate = targetFPS;
        
    }
    public void Update()
    {
        //Debug.Log(Application.targetFrameRate);
    }
}


