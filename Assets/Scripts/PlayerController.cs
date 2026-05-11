using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float playerMovementX;
    float playerMovementY;
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] float rotationSpeed = 10f;
    
    void Update()
    {
        playerMovementX = Input.GetAxisRaw("Horizontal");
        playerMovementY = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(playerMovementX, 0f, playerMovementY);
        movement.Normalize();
        
        transform.Translate(  movementSpeed * Time.deltaTime * movement,Space.World);

        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);            
        }
        
    }
    
}
