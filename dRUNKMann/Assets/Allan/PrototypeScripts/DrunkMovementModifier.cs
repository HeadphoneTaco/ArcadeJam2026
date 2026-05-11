using UnityEngine;
using System.Collections;

public class DrunkMovementModifier : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float drunkSwayMinCooldown;
    [SerializeField] private float drunkSwayMaxCooldown;
    [SerializeField] private float swayForce;
    [SerializeField] private float swayAngle;
    [SerializeField] private float playerStopLeaningDuration;
    [SerializeField] private float swayDuration;

    [SerializeField] private BasicMovementScript movementScript;

    private float _drunkSwayCooldown;

    private Quaternion _uprightPlayerRotation;

    private bool _shouldSwayDrunk;

    void Start()
    {
        _shouldSwayDrunk = true;
        _uprightPlayerRotation = rb.rotation;
        StartCoroutine(SwayDrunkWithCooldown());
    }

    private IEnumerator SwayDrunkWithCooldown()
    {
        while (_shouldSwayDrunk)
        {
            int sideToSway = Random.Range(0, 2);
            
            Quaternion swayAngleLeft = Quaternion.Euler(0, 0, swayAngle);
            Quaternion swayAngleRight = Quaternion.Euler(0, 0, -swayAngle);

            //Sway left
            if (sideToSway == 0 && !movementScript.isPlayerRagdoll)
            {
                float timeElapsedForSwayingLeft = 0f;
                Quaternion startRotationForSwayLeft = rb.rotation;

                while (timeElapsedForSwayingLeft < swayDuration)
                {
                    timeElapsedForSwayingLeft += Time.deltaTime; 
                    rb.AddForce(-Vector3.right * swayForce, ForceMode.Impulse);
                    float swayLeftPercentage = Mathf.Clamp01(timeElapsedForSwayingLeft / playerStopLeaningDuration);
                    rb.MoveRotation(Quaternion.Slerp(startRotationForSwayLeft, swayAngleLeft, swayLeftPercentage));
                    yield return null;
                }
            }
            //Sway right
            else if (sideToSway == 1 && !movementScript.isPlayerRagdoll)
            {
                float timeElapsedForSwayingRight = 0f;
                Quaternion startRotationForSwayRight = rb.rotation;

                while (timeElapsedForSwayingRight < swayDuration)
                {
                    timeElapsedForSwayingRight += Time.deltaTime;
                    rb.AddForce(Vector3.right * swayForce, ForceMode.Impulse);
                    float swayRightPercentage = Mathf.Clamp01(timeElapsedForSwayingRight / playerStopLeaningDuration);
                    rb.MoveRotation(Quaternion.Slerp(startRotationForSwayRight, swayAngleRight, swayRightPercentage));
                    yield return null;
                }
            }

            float timeElapsed = 0f;

            Quaternion startRotation = rb.rotation;

            while (timeElapsed < playerStopLeaningDuration)
            {
                timeElapsed += Time.deltaTime;
                float getUpPercentage = Mathf.Clamp01(timeElapsed / playerStopLeaningDuration);
                rb.MoveRotation(Quaternion.Slerp(startRotation, _uprightPlayerRotation, getUpPercentage));

                yield return null;
            }

            _drunkSwayCooldown = Random.Range(drunkSwayMinCooldown, drunkSwayMaxCooldown);
            yield return new WaitForSeconds(_drunkSwayCooldown);
        }
    }
}
