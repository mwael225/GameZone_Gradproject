using UnityEngine;
using Unity.Netcode;

public class NetworkCameraSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoFindCameras = true;
    public string mainCameraName = "Main Camera";
    public string mainCamera2Name = "Main Camera 2";
    
    [Header("Manual Assignment (if auto find fails)")]
    public Camera mainCamera;
    public Camera mainCamera2;
    
    private NetworkCameraManager cameraManager;
    
    void Start()
    {
        SetupNetworkCameraManager();
    }
    
    void SetupNetworkCameraManager()
    {
        // Check if NetworkCameraManager already exists
        cameraManager = FindObjectOfType<NetworkCameraManager>();
        
        if (cameraManager == null)
        {
            // Create a new GameObject for the camera manager
            GameObject managerObject = new GameObject("NetworkCameraManager");
            cameraManager = managerObject.AddComponent<NetworkCameraManager>();
            
            // Add NetworkObject component if it doesn't exist
            if (managerObject.GetComponent<NetworkObject>() == null)
            {
                managerObject.AddComponent<NetworkObject>();
            }
            
            Debug.Log("Created NetworkCameraManager");
        }
        
        // Auto-find cameras if enabled
        if (autoFindCameras)
        {
            FindCameras();
        }
        
        // Assign cameras to the manager
        AssignCamerasToManager();
    }
    
    void FindCameras()
    {
        // Find Main Camera
        if (mainCamera == null)
        {
            GameObject mainCamObj = GameObject.Find(mainCameraName);
            if (mainCamObj != null)
            {
                mainCamera = mainCamObj.GetComponent<Camera>();
                Debug.Log($"Found Main Camera: {mainCamObj.name}");
            }
            else
            {
                Debug.LogWarning($"Could not find camera with name: {mainCameraName}");
            }
        }
        
        // Find Main Camera 2
        if (mainCamera2 == null)
        {
            GameObject mainCam2Obj = GameObject.Find(mainCamera2Name);
            if (mainCam2Obj != null)
            {
                mainCamera2 = mainCam2Obj.GetComponent<Camera>();
                Debug.Log($"Found Main Camera 2: {mainCam2Obj.name}");
            }
            else
            {
                Debug.LogWarning($"Could not find camera with name: {mainCamera2Name}");
            }
        }
        
        // If still not found, try to find any cameras in the scene
        if (mainCamera == null || mainCamera2 == null)
        {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            Debug.Log($"Found {allCameras.Length} cameras in scene:");
            
            for (int i = 0; i < allCameras.Length; i++)
            {
                Debug.Log($"Camera {i}: {allCameras[i].name}");
                
                // Assign first camera as main camera if not assigned
                if (mainCamera == null && i == 0)
                {
                    mainCamera = allCameras[i];
                    Debug.Log($"Auto-assigned {allCameras[i].name} as Main Camera");
                }
                // Assign second camera as main camera 2 if not assigned
                else if (mainCamera2 == null && i == 1)
                {
                    mainCamera2 = allCameras[i];
                    Debug.Log($"Auto-assigned {allCameras[i].name} as Main Camera 2");
                }
            }
        }
    }
    
    void AssignCamerasToManager()
    {
        if (cameraManager != null)
        {
            cameraManager.mainCamera = mainCamera;
            cameraManager.mainCamera2 = mainCamera2;
            
            Debug.Log("Cameras assigned to NetworkCameraManager:");
            Debug.Log($"Main Camera: {(mainCamera != null ? mainCamera.name : "Not assigned")}");
            Debug.Log($"Main Camera 2: {(mainCamera2 != null ? mainCamera2.name : "Not assigned")}");
        }
        else
        {
            Debug.LogError("NetworkCameraManager not found!");
        }
    }
    
    // Public method to manually trigger camera assignment
    [ContextMenu("Setup Cameras")]
    public void ManualSetup()
    {
        SetupNetworkCameraManager();
    }
    
    // Public method to reassign cameras
    [ContextMenu("Reassign Cameras")]
    public void ReassignCameras()
    {
        if (cameraManager != null)
        {
            cameraManager.ReassignCameras();
        }
        else
        {
            Debug.LogWarning("NetworkCameraManager not found. Run Setup Cameras first.");
        }
    }
} 