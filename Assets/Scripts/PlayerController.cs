using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //variables
    
    //movement
    float playerMovementX;
    float playerMovementY;
    
    //jumping
    float jumpPressed;
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

    void Start()
    {
        //get rigidbody
        rb = GetComponent<Rigidbody>();
        
    }
    void FixedUpdate()
    {
        //jump if shouldJump is true
        if (shouldJump)
        {
            jump();
            shouldJump = false;
        }
        
    }
    
    void Update()
    {   
        //get player input
        playerMovementX = Input.GetAxisRaw("Horizontal");
        playerMovementY = Input.GetAxisRaw("Vertical");
        jumpPressed = Input.GetAxis("Jump");
        
        //convert player input into Vector3
        Vector3 movement = new Vector3(playerMovementX, 0f, playerMovementY);
        movement.Normalize();
        
        //player movement
        transform.Translate(  movementSpeed * Time.deltaTime * movement,Space.World);

        //rotate the player with movement
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);            
        }
        //raycast
        isGrounded = Physics.CheckSphere(transform.position * sphereDistance, sphereRadius, groundLayer);

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;       
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
}


