using UnityEngine;

public class BottlePickup : MonoBehaviour
{
    [SerializeField] private Rigidbody bottleRb;
    [SerializeField] private float moveSpeed = 0.7f;
    [SerializeField] private int destroyLayer = 8;
    [SerializeField] private string playerTag = "Player";

    private bool wasCollected;

    private void Awake()
    {
        if (bottleRb == null)
        {
            bottleRb = GetComponent<Rigidbody>();
        }

        if (bottleRb == null)
        {
            bottleRb = gameObject.AddComponent<Rigidbody>();
        }
    }

    public void Initialize(float speed)
    {
        moveSpeed = speed;
    }

    private void FixedUpdate()
    {
        bottleRb.AddForce(-Vector3.forward * moveSpeed, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject other)
    {
        if (other.layer == destroyLayer)
        {
            Destroy(gameObject);
            return;
        }

        if (wasCollected || !other.CompareTag(playerTag))
        {
            return;
        }

        wasCollected = true;

        if (PlayerScore.Instance != null)
        {
            PlayerScore.Instance.AddBottle();
        }

        Destroy(gameObject);
    }
}
