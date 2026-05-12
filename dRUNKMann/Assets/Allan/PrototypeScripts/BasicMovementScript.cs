using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BasicMovementScript : MonoBehaviour
{
    public float moveSpeed;
    [SerializeField] private float diveHeight;
    [SerializeField] private float diveDirectionalStrength;
    [SerializeField] private float groundCheckSphereVerticalOffset;
    [SerializeField] private float groundCheckSphereRadius = 2f;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private Rigidbody rb;
    public float maxVelocity;
    public float maxDiveVelocity;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float playerRagdollDuration;
    [SerializeField] private float playerGetUpDuration;

    private bool _shouldPlayerUseDiveVelocity;
    public bool isPlayerRagdoll;
    private Vector2 moveInput;
    private RigidbodyConstraints rbDefaultConstraints;
    private Quaternion uprightPlayerRotation;

    private void Start()
    {
        //rb.maxLinearVelocity = maxVelocity;
        _shouldPlayerUseDiveVelocity = false;
        rbDefaultConstraints = rb.constraints;
        uprightPlayerRotation = rb.rotation;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Dive(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            Quaternion targetLeanRotation = Quaternion.Euler(moveInput.y * 45, 0f, -moveInput.x * 45);
            isPlayerRagdoll = true;
            rb.constraints = RigidbodyConstraints.None;
            //rb.maxLinearVelocity = maxDiveVelocity;
            _shouldPlayerUseDiveVelocity = true;
            rb.AddForce(new Vector3(moveInput.x * diveDirectionalStrength, diveHeight, moveInput.y * diveDirectionalStrength), ForceMode.Impulse);
            rb.MoveRotation(targetLeanRotation);
            StartCoroutine(ResetPlayerRotationAfterDiving());
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(new Vector3(playerCapsule.transform.position.x, playerCapsule.transform.position.y - groundCheckSphereVerticalOffset, playerCapsule.transform.position.z), groundCheckSphereRadius, groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(playerCapsule.transform.position.x, playerCapsule.transform.position.y - groundCheckSphereVerticalOffset, playerCapsule.transform.position.z), groundCheckSphereRadius);
    }

    private void FixedUpdate()
    {
        if (IsGrounded() && !isPlayerRagdoll)
        {
            rb.AddForce(new Vector3(moveInput.x, 0f, moveInput.y).normalized * moveSpeed, ForceMode.VelocityChange);
        }

        LimitHorizontalVelocity();
    }

    private IEnumerator ResetPlayerRotationAfterDiving()
    {
        yield return new WaitForSeconds(playerRagdollDuration);

        float timeElapsed = 0f;

        Quaternion startRotation = rb.rotation;

        while (timeElapsed < playerGetUpDuration)
        {
            timeElapsed += Time.deltaTime;
            float getUpPercentage = Mathf.Clamp01(timeElapsed /  playerGetUpDuration);
            rb.MoveRotation(Quaternion.Slerp(startRotation, uprightPlayerRotation, getUpPercentage));

            yield return null;
        }

        //rb.maxLinearVelocity = maxVelocity;
        _shouldPlayerUseDiveVelocity = false;
        rb.constraints = rbDefaultConstraints;
        isPlayerRagdoll = false;
    }

    private void LimitHorizontalVelocity()
    {
        if (_shouldPlayerUseDiveVelocity)
        {
            Vector3 limitedHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).normalized * maxDiveVelocity;

            if (rb.linearVelocity.magnitude > maxDiveVelocity)
            {
                rb.linearVelocity = new Vector3(limitedHorizontalVelocity.x, rb.linearVelocity.y, limitedHorizontalVelocity.z);
            }
        }
        else
        {
            Vector3 limitedHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).normalized * maxVelocity;

            if (rb.linearVelocity.magnitude > maxVelocity)
            {
                rb.linearVelocity = new Vector3(limitedHorizontalVelocity.x, rb.linearVelocity.y, limitedHorizontalVelocity.z);
            }
        }
    }
}
