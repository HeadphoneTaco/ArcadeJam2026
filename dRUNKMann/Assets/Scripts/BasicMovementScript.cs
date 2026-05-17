using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BasicMovementScript : MonoBehaviour
{
    [Header("Speed")]
    [Tooltip("Target walking speed. The actual top speed is capped by maxVelocity.")]
    public float moveSpeed;
    [Tooltip("Walking speed cap. If this is lower than moveSpeed, this becomes the actual top speed.")]
    public float maxVelocity;
    public float maxDiveVelocity;

    [Header("Movement Smoothing")]
    [Tooltip("How many seconds it takes to reach full walking speed from a stop.")]
    [SerializeField] private float secondsToReachFullSpeed = 0.4f;
    [Tooltip("Fastest possible stop time, in seconds, after releasing movement input.")]
    [SerializeField] private float shortestRandomStopTime = 1.2f;
    [Tooltip("Slowest possible stop time, in seconds, after releasing movement input.")]
    [SerializeField] private float longestRandomStopTime = 3f;
    [SerializeField] private float movementInputDeadZone = 0.01f;
    [SerializeField] private bool requireGroundedToMove = false;

    [Header("Side Lean")]
    [Tooltip("Small side lean while the player is moving left or right.")]
    [SerializeField] private float movingSideLeanAngle = 8f;
    [Tooltip("Bigger side lean while the player is slowly stopping after sideways movement.")]
    [SerializeField] private float stoppingSideLeanAngle = 22f;
    [Tooltip("How long the lean takes to settle into its target angle. Higher is smoother/slower.")]
    [SerializeField] private float sideLeanSmoothTime = 0.25f;

    [Header("Dive")]
    [SerializeField] private float diveHeight;
    [SerializeField] private float diveDirectionalStrength;
    [SerializeField] private float diveLeanAngle = 55f;
    [SerializeField] private float diveLeanInDuration = 0.2f;
    [SerializeField] private float diveAirSpinTorque = 8f;
    [SerializeField] private float diveCooldown = 0.75f;
    [SerializeField] private float groundCheckSphereVerticalOffset;
    [SerializeField] private float groundCheckSphereRadius = 2f;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float playerRagdollDuration;
    [SerializeField] private float playerGetUpDuration;

    private bool _shouldPlayerUseDiveVelocity;
    [System.NonSerialized] public bool isPlayerRagdoll;
    private Vector2 moveInput;
    private RigidbodyConstraints rbDefaultConstraints;
    private Quaternion uprightPlayerRotation;
    private float sideLeanAmount;
    private float sideLeanVelocity;
    private float currentStopDuration;
    private float stopTimeElapsed;
    private Vector3 stopStartVelocity;
    private bool wasTryingToMove;
    private bool isRandomlyStopping;
    private float nextDiveAllowedTime;
    private Coroutine diveLeanCoroutine;
    private Coroutine diveRecoveryCoroutine;

    private void Awake()
    {
        isPlayerRagdoll = false;
    }

    private void Start()
    {
        //rb.maxLinearVelocity = maxVelocity;
        isPlayerRagdoll = false;
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
        if (!context.performed || !CanStartDive())
        {
            return;
        }

        StartDive();
    }

    private bool CanStartDive()
    {
        return !isPlayerRagdoll && Time.time >= nextDiveAllowedTime && IsGrounded();
    }

    private void StartDive()
    {
        isPlayerRagdoll = true;
        nextDiveAllowedTime = Time.time + diveCooldown;
        rb.constraints = RigidbodyConstraints.None;
        //rb.maxLinearVelocity = maxDiveVelocity;
        _shouldPlayerUseDiveVelocity = true;
        isRandomlyStopping = false;
        stopTimeElapsed = 0f;
        sideLeanVelocity = 0f;

        Vector3 currentVelocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        Vector3 diveDirection = GetMoveDirection();

        if (diveDirection.sqrMagnitude <= 0f)
        {
            Vector3 forward = transform.forward;
            diveDirection = new Vector3(forward.x, 0f, forward.z).normalized;

            if (diveDirection.sqrMagnitude <= 0f)
            {
                diveDirection = Vector3.forward;
            }
        }

        rb.AddForce(new Vector3(diveDirection.x * diveDirectionalStrength, diveHeight, diveDirection.z * diveDirectionalStrength), ForceMode.Impulse);
        rb.AddTorque(GetDiveTorque(diveDirection), ForceMode.Impulse);

        if (diveLeanCoroutine != null)
        {
            StopCoroutine(diveLeanCoroutine);
        }

        if (diveRecoveryCoroutine != null)
        {
            StopCoroutine(diveRecoveryCoroutine);
        }

        diveLeanCoroutine = StartCoroutine(LeanIntoDive(diveDirection));
        diveRecoveryCoroutine = StartCoroutine(ResetPlayerRotationAfterDiving());
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
        if (CanApplyNormalMovement())
        {
            ApplyGroundMovement();
            ApplySideLean();
        }
        else
        {
            wasTryingToMove = false;
        }

        LimitHorizontalVelocity();
    }

    private bool CanApplyNormalMovement()
    {
        if (isPlayerRagdoll)
        {
            return false;
        }

        return !requireGroundedToMove || IsGrounded();
    }

    private void ApplyGroundMovement()
    {
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 moveDirection = GetMoveDirection();
        bool isTryingToMove = moveDirection.sqrMagnitude > 0f;

        if (isTryingToMove)
        {
            isRandomlyStopping = false;
            Vector3 targetHorizontalVelocity = moveDirection * GetTargetWalkSpeed();
            currentHorizontalVelocity = ApplyWalkingAcceleration(currentHorizontalVelocity, moveDirection, targetHorizontalVelocity);
        }
        else
        {
            if (!isRandomlyStopping && currentHorizontalVelocity.sqrMagnitude > 0.01f)
            {
                BeginRandomStop(currentHorizontalVelocity);
            }

            if (isRandomlyStopping)
            {
                currentHorizontalVelocity = GetRandomStopVelocity();
            }
        }

        rb.linearVelocity = new Vector3(currentHorizontalVelocity.x, rb.linearVelocity.y, currentHorizontalVelocity.z);
        wasTryingToMove = isTryingToMove;
    }

    private Vector3 ApplyWalkingAcceleration(Vector3 currentHorizontalVelocity, Vector3 moveDirection, Vector3 targetHorizontalVelocity)
    {
        float speedChangeThisFrame = GetWalkingSpeedChangePerSecond() * Time.fixedDeltaTime;

        if (IsTryingToMoveAgainstMomentum(currentHorizontalVelocity, moveDirection))
        {
            return Vector3.MoveTowards(currentHorizontalVelocity, Vector3.zero, speedChangeThisFrame);
        }

        return Vector3.MoveTowards(currentHorizontalVelocity, targetHorizontalVelocity, speedChangeThisFrame);
    }

    private bool IsTryingToMoveAgainstMomentum(Vector3 currentHorizontalVelocity, Vector3 moveDirection)
    {
        return currentHorizontalVelocity.sqrMagnitude > 0.01f && Vector3.Dot(currentHorizontalVelocity.normalized, moveDirection) < -0.25f;
    }

    private float GetWalkingSpeedChangePerSecond()
    {
        return GetTargetWalkSpeed() / Mathf.Max(0.01f, secondsToReachFullSpeed);
    }

    private float GetTargetWalkSpeed()
    {
        return Mathf.Max(0f, Mathf.Min(moveSpeed, maxVelocity));
    }

    private Vector3 GetMoveDirection()
    {
        float sideInput = Mathf.Abs(moveInput.x) > movementInputDeadZone ? moveInput.x : 0f;
        return new Vector3(sideInput, 0f, 1f).normalized;
    }

    private bool IsPressingSideways()
    {
        return Mathf.Abs(moveInput.x) > movementInputDeadZone;
    }

    private void BeginRandomStop(Vector3 horizontalVelocity)
    {
        float shortestStopTime = Mathf.Min(shortestRandomStopTime, longestRandomStopTime);
        float longestStopTime = Mathf.Max(shortestRandomStopTime, longestRandomStopTime);
        float randomStopTime = Mathf.Max(0.01f, Random.Range(shortestStopTime, longestStopTime));

        currentStopDuration = randomStopTime;
        stopTimeElapsed = 0f;
        stopStartVelocity = horizontalVelocity;
        isRandomlyStopping = true;
    }

    private Vector3 GetRandomStopVelocity()
    {
        stopTimeElapsed += Time.fixedDeltaTime;
        float stopPercentage = Mathf.Clamp01(stopTimeElapsed / currentStopDuration);
        Vector3 stopVelocity = Vector3.Lerp(stopStartVelocity, Vector3.zero, stopPercentage);

        if (stopPercentage >= 1f || stopVelocity.sqrMagnitude <= 0.01f)
        {
            isRandomlyStopping = false;
            return Vector3.zero;
        }

        return stopVelocity;
    }

    private void ApplySideLean()
    {
        float targetLeanAmount = GetTargetSideLeanAmount();
        sideLeanAmount = Mathf.SmoothDampAngle(sideLeanAmount, targetLeanAmount, ref sideLeanVelocity, Mathf.Max(0.01f, sideLeanSmoothTime), Mathf.Infinity, Time.fixedDeltaTime);

        Quaternion targetRotation = uprightPlayerRotation * Quaternion.Euler(0f, 0f, sideLeanAmount);
        float leanPercentage = 1f - Mathf.Exp(-(1f / Mathf.Max(0.01f, sideLeanSmoothTime)) * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, leanPercentage));
    }

    private float GetTargetSideLeanAmount()
    {
        if (IsPressingSideways())
        {
            return -Mathf.Clamp(moveInput.x, -1f, 1f) * movingSideLeanAngle;
        }

        float sidewaysSpeed = rb.linearVelocity.x;
        float sidewaysSpeedPercentage = Mathf.Clamp(sidewaysSpeed / Mathf.Max(0.01f, GetTargetWalkSpeed()), -1f, 1f);
        return -sidewaysSpeedPercentage * stoppingSideLeanAngle;
    }

    private IEnumerator LeanIntoDive(Vector3 diveDirection)
    {
        float timeElapsed = 0f;
        Quaternion startRotation = rb.rotation;
        Quaternion diveRotation = uprightPlayerRotation * Quaternion.Euler(diveDirection.z * diveLeanAngle, 0f, -diveDirection.x * diveLeanAngle);

        while (timeElapsed < diveLeanInDuration)
        {
            timeElapsed += Time.deltaTime;
            float leanPercentage = Mathf.Clamp01(timeElapsed / Mathf.Max(0.01f, diveLeanInDuration));
            rb.MoveRotation(Quaternion.Slerp(startRotation, diveRotation, leanPercentage));

            yield return null;
        }

        diveLeanCoroutine = null;
    }

    private Vector3 GetDiveTorque(Vector3 diveDirection)
    {
        Vector3 tumbleAxis = Vector3.Cross(Vector3.up, diveDirection).normalized;
        Vector3 rollAxis = diveDirection * Random.Range(-0.35f, 0.35f);

        return (tumbleAxis + rollAxis) * diveAirSpinTorque;
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
        nextDiveAllowedTime = Mathf.Max(nextDiveAllowedTime, Time.time + diveCooldown);
        diveRecoveryCoroutine = null;
    }

    private void LimitHorizontalVelocity()
    {
        float currentMaxVelocity = _shouldPlayerUseDiveVelocity ? maxDiveVelocity : maxVelocity;
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.sqrMagnitude <= currentMaxVelocity * currentMaxVelocity)
        {
            return;
        }

        Vector3 limitedHorizontalVelocity = horizontalVelocity.normalized * currentMaxVelocity;
        rb.linearVelocity = new Vector3(limitedHorizontalVelocity.x, rb.linearVelocity.y, limitedHorizontalVelocity.z);
    }
}
