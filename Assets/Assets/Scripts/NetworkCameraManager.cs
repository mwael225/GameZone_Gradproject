using UnityEngine;
using Unity.Netcode;

public class NetworkCameraManager : NetworkBehaviour
{
    [Header("Camera Assignment")]
    public Camera mainCamera;        // Assign Main Camera in inspector
    public Camera mainCamera2;       // Assign Main Camera 2 in inspector
    
    [Header("Camera Control")]
    public bool enableRightMouseControl = true;
    
    private Camera activeCamera;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Wait a frame to ensure network is fully initialized
        Invoke(nameof(AssignCamera), 0.1f);
    }
    
    void AssignCamera()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("NetworkManager not found, using default camera assignment");
            AssignDefaultCamera();
            return;
        }
        
        // Check if this is the host or client
        if (NetworkManager.Singleton.IsHost)
        {
            AssignHostCamera();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            AssignClientCamera();
        }
        else
        {
            Debug.LogWarning("Not connected to network, using default camera assignment");
            AssignDefaultCamera();
        }
    }
    
    void AssignHostCamera()
    {
        Debug.Log("Assigning camera for HOST");
        
        // Disable all cameras first
        DisableAllCameras();
        
        // Enable Main Camera 2 for host
        if (mainCamera2 != null)
        {
            mainCamera2.gameObject.SetActive(true);
            mainCamera2.tag = "MainCamera";
            activeCamera = mainCamera2;
            
            // Add camera control script if enabled
            if (enableRightMouseControl)
            {
                AddCameraControl(mainCamera2);
            }
            
            Debug.Log("Host camera assigned: Main Camera 2");
        }
        else
        {
            Debug.LogError("Main Camera 2 not assigned in NetworkCameraManager!");
        }
    }
    
    void AssignClientCamera()
    {
        Debug.Log("Assigning camera for CLIENT");
        
        // Disable all cameras first
        DisableAllCameras();
        
        // Enable Main Camera for client
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            mainCamera.tag = "MainCamera";
            activeCamera = mainCamera;
            
            // Add camera control script if enabled
            if (enableRightMouseControl)
            {
                AddCameraControl(mainCamera);
            }
            
            Debug.Log("Client camera assigned: Main Camera");
        }
        else
        {
            Debug.LogError("Main Camera not assigned in NetworkCameraManager!");
        }
    }
    
    void AssignDefaultCamera()
    {
        Debug.Log("Assigning default camera");
        
        // Disable all cameras first
        DisableAllCameras();
        
        // Enable Main Camera as default
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            mainCamera.tag = "MainCamera";
            activeCamera = mainCamera;
            
            // Add camera control script if enabled
            if (enableRightMouseControl)
            {
                AddCameraControl(mainCamera);
            }
            
            Debug.Log("Default camera assigned: Main Camera");
        }
        else
        {
            Debug.LogError("Main Camera not assigned in NetworkCameraManager!");
        }
    }
    
    void DisableAllCameras()
    {
        // Disable main camera
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
            mainCamera.tag = "Untagged";
        }
        
        // Disable main camera 2
        if (mainCamera2 != null)
        {
            mainCamera2.gameObject.SetActive(false);
            mainCamera2.tag = "Untagged";
        }
        
        // Also disable any other cameras in the scene
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam != mainCamera && cam != mainCamera2)
            {
                cam.gameObject.SetActive(false);
                cam.tag = "Untagged";
            }
        }
    }
    
    void AddCameraControl(Camera camera)
    {
        // Remove existing camera control scripts
        RightMouseCameraControl existingControl = camera.GetComponent<RightMouseCameraControl>();
        if (existingControl != null)
        {
            DestroyImmediate(existingControl);
        }
        
        // Add new camera control script
        RightMouseCameraControl newControl = camera.gameObject.AddComponent<RightMouseCameraControl>();
        Debug.Log($"Added RightMouseCameraControl to {camera.name}");
    }
    
    // Public method to manually reassign cameras (useful for testing)
    [ContextMenu("Reassign Cameras")]
    public void ReassignCameras()
    {
        AssignCamera();
    }
    
    // Public method to get the currently active camera
    public Camera GetActiveCamera()
    {
        return activeCamera;
    }
} 