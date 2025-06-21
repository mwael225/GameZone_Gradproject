using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 2f;
    public float zoomSpeed = 5f;
    
    [Header("Rotation Limits")]
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;
    
    [Header("Zoom Limits")]
    public float minZoom = 1f;
    public float maxZoom = 20f;
    
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private float currentZoom = 10f;
    
    private bool isRightMouseDown = false;
    private Vector3 lastMousePosition;
    
    void Start()
    {
        // Initialize rotation from current camera rotation
        Vector3 currentRotation = transform.eulerAngles;
        currentRotationX = currentRotation.y;
        currentRotationY = currentRotation.x;
        
        // Initialize zoom based on camera position
        currentZoom = Vector3.Distance(transform.position, Vector3.zero);
    }
    
    void Update()
    {
        HandleRightMouseButton();
        HandleMouseMovement();
        HandleZoom();
    }
    
    void HandleRightMouseButton()
    {
        // Check if right mouse button is pressed
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
            lastMousePosition = Input.mousePosition;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Check if right mouse button is released
        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void HandleMouseMovement()
    {
        if (!isRightMouseDown) return;
        
        // Get mouse delta
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
        
        // Handle rotation
        currentRotationX += mouseDelta.x * rotationSpeed * Time.deltaTime;
        currentRotationY -= mouseDelta.y * rotationSpeed * Time.deltaTime;
        
        // Clamp vertical rotation
        currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
        
        // Apply rotation
        transform.rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0f);
        
        // Handle movement
        Vector3 moveDirection = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W))
            moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S))
            moveDirection += -transform.forward;
        if (Input.GetKey(KeyCode.A))
            moveDirection += -transform.right;
        if (Input.GetKey(KeyCode.D))
            moveDirection += transform.right;
        if (Input.GetKey(KeyCode.Q))
            moveDirection += Vector3.down;
        if (Input.GetKey(KeyCode.E))
            moveDirection += Vector3.up;
        
        // Apply movement
        if (moveDirection.magnitude > 0)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
        }
        
        lastMousePosition = Input.mousePosition;
    }
    
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            
            // Move camera forward/backward based on zoom
            Vector3 zoomDirection = transform.forward;
            transform.position += zoomDirection * scroll * zoomSpeed;
        }
    }
} 