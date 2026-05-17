using UnityEngine;
using UnityEngine.SceneManagement;

public class RatMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private Vector3 moveDirection = Vector3.back;
    [Tooltip("Assign the solid BoxCollider that should restart the scene when it collides with the player.")]
    [SerializeField] private BoxCollider hitCollider;
    [SerializeField] private int destroyLayer = 8;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody ratRb;
    private bool isReloadingScene;

    private void Awake()
    {
        ratRb = GetComponent<Rigidbody>();

        if (ratRb == null)
        {
            ratRb = gameObject.AddComponent<Rigidbody>();
        }

        ratRb.useGravity = false;
        ratRb.isKinematic = true;
        ratRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        ratRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Initialize(float speed, Vector3 direction)
    {
        moveSpeed = speed;
        moveDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.back;
    }

    private void FixedUpdate()
    {
        Vector3 movement = moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;

        if (ratRb != null)
        {
            ratRb.MovePosition(ratRb.position + movement);
        }
        else
        {
            transform.position += movement;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!DidCollisionUseHitCollider(collision))
        {
            return;
        }

        HandleCollisionHit(collision.gameObject);
    }

    private bool DidCollisionUseHitCollider(Collision collision)
    {
        if (hitCollider == null)
        {
            return false;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).thisCollider == hitCollider)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleCollisionHit(GameObject other)
    {
        if (other.layer == destroyLayer)
        {
            Destroy(gameObject);
            return;
        }

        if (!isReloadingScene && other.CompareTag(playerTag))
        {
            isReloadingScene = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
