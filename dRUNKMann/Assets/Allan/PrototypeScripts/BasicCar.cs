using UnityEngine;

public class BasicCar : MonoBehaviour
{
    [SerializeField] private Rigidbody carRb;
    [SerializeField] private float carVelocity;

    void FixedUpdate()
    {
        carRb.AddForce(-Vector3.forward * carVelocity, ForceMode.VelocityChange);
    }
}
