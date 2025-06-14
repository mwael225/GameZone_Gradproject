using UnityEngine;
using UnityEngine.SceneManagement;

public class persistentScript : MonoBehaviour
{
    public static persistentScript Instance;
    bool test = false;

    private void Awake()
    {
        // Check if an instance already exists, if so, destroy the new one.
        if (Instance == null)
        {
            Instance = this; // Set this as the instance
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        if (SceneManager.GetActiveScene().name == "MainMenuBasic")
        {
            test = true;
        }
    }

    void Update()
    {
        // Put logic that needs to run every frame here.
        Debug.Log("Persistent script is running");
        Debug.Log("test : " + test);
    }
}
