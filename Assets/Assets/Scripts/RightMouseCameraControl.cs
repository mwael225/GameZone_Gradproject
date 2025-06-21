using UnityEngine;

public class RightMouseCameraControl : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    public float mouseSensitivity = 2f;
    public float zoomSpeed = 5f;
    public float minZoom = 1f;
    public float maxZoom = 20f;
    
    private bool isRightMouseDown = false;
    private float currentZoom = 10f;
    
    // Store rotation values to prevent tilt
    private float rotationX = 0f;
    private float rotationY = 0f;
    
    void Start()
    {
        // Make sure cursor is visible by default
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Initialize zoom based on camera position
        currentZoom = Vector3.Distance(transform.position, Vector3.zero);
        
        // Initialize rotation from camera's current orientation
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
    }
    
    void Update()
    {
        HandleRightMouseInput();
        HandleZoom();
        
        if (isRightMouseDown)
        {
            HandleCameraRotation();
        }
    }
    
    void HandleRightMouseInput()
    {
        // Start right mouse button press
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // End right mouse button press
        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void HandleCameraRotation()
    {
        // Get continuous mouse movement using Input.GetAxis
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Update rotation values
        rotationX += mouseX;
        rotationY -= mouseY;
        
        // Apply rotation using eulerAngles to prevent tilt (roll)
        transform.eulerAngles = new Vector3(rotationY, rotationX, 0f);
    }
    
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Move camera forward/backward based on scroll wheel
            Vector3 zoomDirection = transform.forward;
            transform.position += zoomDirection * scroll * zoomSpeed;
            
            // Update current zoom distance
            currentZoom = Vector3.Distance(transform.position, Vector3.zero);
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }
} 