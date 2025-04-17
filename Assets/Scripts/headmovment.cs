using UnityEngine;

public class headmovment : MonoBehaviour
{

    public float sensitivityX = 2.0f; // Horizontal sensitivity
    public float sensitivityY = 2.0f; // Vertical sensitivity


    public float minimumY = -60.0f;   // Minimum vertical angle
    public float maximumY = 60.0f;    // Maximum vertical angle
    public float minimumX = -60.0f;   // Minimum horizontal angle
    public float maximumX = 60.0f;    // Maximum horizontal angle


    public float moveSpeed = 10f;     // Scroll movement speed

    private float rotationX = 0f;   // Current horizontal rotation
    private float rotationY = 0f;    // Current vertical rotation

    void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
    }

    private void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        

        // Update rotation values
        rotationX += mouseX * sensitivityX;
        rotationY -= mouseY * sensitivityY;

        // Clamp the rotations
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
        rotationX = Mathf.Clamp(rotationX, minimumX, maximumX);

        // Smoothly apply the rotation (optional)
        Quaternion targetRotation = Quaternion.Euler(rotationY, rotationX, 0);

        transform.localRotation = targetRotation;
        
    }

}
