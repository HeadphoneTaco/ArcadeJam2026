using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovementScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float diveHeight;
    [SerializeField] private float diveDirectionalStrength;
    [SerializeField] private float groundCheckSphereVerticalOffset;
    [SerializeField] private float groundCheckSphereRadius = 2f;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float maxVelocity;

    private Vector2 moveInput;

    private void Start()
    {
        rb.maxLinearVelocity = maxVelocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log(moveInput);
    }

    public void Dive(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            Debug.Log("Diving now!");
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(new Vector3(playerCapsule.transform.position.x, playerCapsule.transform.position.y - groundCheckSphereVerticalOffset, playerCapsule.transform.position.y), 2f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(playerCapsule.transform.position.x, playerCapsule.transform.position.y - groundCheckSphereVerticalOffset, playerCapsule.transform.position.z), groundCheckSphereRadius);
    }

    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(moveInput.x * moveSpeed, 0f, moveInput.y * moveSpeed).normalized, ForceMode.VelocityChange);
    }
}
