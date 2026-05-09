using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float playerMovementX;
    float playerMovementY;
    [SerializeField] float movementSpeed = 10f;
    
    void Update()
    {
        playerMovementX = Input.GetAxis("Horizontal");
        playerMovementY = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(playerMovementX, 0f, playerMovementY);
        
        transform.Translate(movement * (movementSpeed * Time.deltaTime));
        
    }
    
}
