using UnityEngine;

public class BasicCar : MonoBehaviour
{
    [SerializeField] private Rigidbody carRb;
    [SerializeField] private float carVelocity;

    void FixedUpdate()
    {
        carRb.AddForce(-Vector3.forward * carVelocity, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Destroy(gameObject);
        }
    }
}
