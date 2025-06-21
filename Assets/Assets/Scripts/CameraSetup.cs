using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [Header("Camera Setup")]
    public Vector3 cameraStartPosition = new Vector3(0, 5, -10);
    public Vector3 cameraStartRotation = new Vector3(15, 0, 0);
    
    void Start()
    {
        SetupMainCamera();
    }
    
    void SetupMainCamera()
    {
        // Check if there's already a main camera
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            // Create a new camera if none exists
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            
            // Set camera properties
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            mainCamera.backgroundColor = Color.black;
            mainCamera.fieldOfView = 60f;
            mainCamera.nearClipPlane = 0.3f;
            mainCamera.farClipPlane = 1000f;
            
            // Set position and rotation
            cameraObject.transform.position = cameraStartPosition;
            cameraObject.transform.rotation = Quaternion.Euler(cameraStartRotation);
            
            Debug.Log("Created new Main Camera at position: " + cameraStartPosition);
        }
        
        // Add the right mouse camera control script if it doesn't exist
        RightMouseCameraControl cameraControl = mainCamera.GetComponent<RightMouseCameraControl>();
        if (cameraControl == null)
        {
            cameraControl = mainCamera.gameObject.AddComponent<RightMouseCameraControl>();
            Debug.Log("Added RightMouseCameraControl to Main Camera");
        }
        
        // Add audio listener if it doesn't exist
        AudioListener audioListener = mainCamera.GetComponent<AudioListener>();
        if (audioListener == null)
        {
            audioListener = mainCamera.gameObject.AddComponent<AudioListener>();
            Debug.Log("Added AudioListener to Main Camera");
        }
    }
} 