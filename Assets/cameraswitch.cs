using UnityEngine;

public class cameraswitch : MonoBehaviour
{
    private Camera[] cameras;
    private int currentCameraIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get all cameras in the scene
        cameras = Camera.allCameras;
        
        // Disable all cameras except the first one
        for (int i = 1; i < cameras.Length; i++)
        {
            cameras[i].enabled = false;
        }
        
        // Enable the first camera
        if (cameras.Length > 0)
        {
            cameras[0].enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if 'C' key is pressed
        if (Input.GetKeyDown(KeyCode.C) && cameras.Length > 1)
        {
            // Disable current camera
            cameras[currentCameraIndex].enabled = false;
            
            // Move to next camera
            currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
            
            // Enable new camera
            cameras[currentCameraIndex].enabled = true;
        }
    }
}
