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
        
        // check jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            shouldJump = true;
        }
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

    //jump method
    void jump()
    {
        rb.AddForce(jumpForce * jumpPressed * Vector3.up  , ForceMode.Impulse);
    }

    //gizmo method
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position * sphereDistance, sphereRadius);
        
    }
}


