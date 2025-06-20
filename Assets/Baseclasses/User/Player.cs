using UnityEngine;
using System.IO;
using TMPro;

public class Player : MonoBehaviour
{
    public TMP_InputField inputField;  // Assign this in the inspector

    private string documentsPath;
    private string gameZonePath;
    private string playerDataPath;
    public GameObject panel;

    void Start()
    {
        documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        gameZonePath = Path.Combine(documentsPath, "GameZone");
        playerDataPath = Path.Combine(gameZonePath, "playerdata");

        if (!Directory.Exists(gameZonePath))
        {
            Directory.CreateDirectory(gameZonePath);
            Debug.Log("'GameZone' folder created.");
        }

        if (File.Exists(playerDataPath))
        {
            string content = File.ReadAllText(playerDataPath);
            Debug.Log("Contents of 'playerdata':" + content);
        }
        else
        {
            panel.SetActive(true);
        }
    }

    public void SavePlayerData()
    {
        if (inputField == null)
        {
            Debug.LogError("InputField is not assigned.");
            return;
        }

        string userInput = inputField.text;
        File.WriteAllText(playerDataPath, userInput);
        Debug.Log("'playerdata' has been saved with new content.");
        panel.SetActive(false);
    }
}
