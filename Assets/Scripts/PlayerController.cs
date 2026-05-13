using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // get input actions
    MyInputActions inputActions;
    
    //variables
    
    //movement
    Vector2 inputValue;
    Vector3 movement;
    
    //jumping
    bool shouldJump;
    [SerializeField] float jumpForce = 20f;
    bool isGrounded;
    
    //jump buffer variables
    [SerializeField] float jumpBufferTime = 0.2f;
    float jumpBufferCounter;
    
    //references
    Rigidbody rb;
    [SerializeField] LayerMask groundLayer;
    
    //inspector
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float sphereRadius = 0.1f;
    [SerializeField] float sphereDistance = 0.1f;
    [SerializeField] Transform footLocation;

    void Awake()
    {
        // instantiate input actions
        inputActions = new MyInputActions();
        inputActions.Enable();    
    }

    void Start()
    {
        //get rigidbody
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        inputActions.Player.Movement.performed += onMove;
        inputActions.Player.Movement.canceled += onMove;
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Movement.performed -= onMove;
        inputActions.Player.Movement.canceled -= onMove;
    }

    void FixedUpdate()
    {
        handleMovement();
        
        //jump if shouldJump is true
        if (shouldJump)
        {
            jump();
            shouldJump = false;
        }
        
    }
    
    void Update()
    {   
        //converting input into a Vector3
        movement = new Vector3(inputValue.x, 0, inputValue.y);
        movement = movement.normalized;
        
        //rotate the player with movement
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);            
        }
        //raycast
        isGrounded = Physics.CheckSphere(footLocation.position , sphereRadius, groundLayer);

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;   
            Debug.Log($"The value for is grounded is {isGrounded}");
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // check jump input
        if (isGrounded && jumpBufferCounter > 0)
        {
            shouldJump = true;
            jumpBufferCounter = 0f;
        }
    }

   

    //jump method
    void jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    //gizmo method
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position * sphereDistance, sphereRadius);
        
    }
    
    //onMove method

    void onMove(InputAction.CallbackContext context)
    {
        inputValue = context.ReadValue<Vector2>();    
    }

    void handleMovement()
    {
        rb.linearVelocity = new Vector3(movement.x * movementSpeed, rb.linearVelocity.y, movement.z * movementSpeed);
    }
}


