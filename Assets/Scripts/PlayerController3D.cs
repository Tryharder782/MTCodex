using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float mouseSensitivity = 2f;
    
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.4f;
    public LayerMask groundLayerMask = 1;
    
    private Rigidbody rb;
    private Camera playerCamera;
    private bool isGrounded;
    private float xRotation = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        
        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // Create camera if it doesn't exist
        if (playerCamera == null)
        {
            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(transform);
            cameraObj.transform.localPosition = new Vector3(0, 0.6f, 0);
            cameraObj.AddComponent<Camera>();
            playerCamera = cameraObj.GetComponent<Camera>();
        }
    }
    
    void Update()
    {
        // Mouse look
        HandleMouseLook();
        
        // Check if player is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        // Handle jumping with W key or Space
        if ((Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            Jump();
        }
        
        // Allow cursor unlock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
    
    void FixedUpdate()
    {
        // Get WASD input
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D keys
        float verticalInput = Input.GetAxisRaw("Vertical");     // W/S keys
        
        // Calculate movement direction relative to where player is looking
        Vector3 direction = transform.right * horizontalInput + transform.forward * verticalInput;
        direction.Normalize();
        
        // Apply movement
        Vector3 moveVelocity = direction * moveSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }
    
    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate the player body left and right
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate the camera up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw ground check sphere in scene view
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}