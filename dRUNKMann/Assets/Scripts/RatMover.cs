using UnityEngine;
using UnityEngine.SceneManagement;

public class RatMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private Vector3 moveDirection = Vector3.back;
    [Tooltip("Assign the BoxCollider trigger that should count as the rat's actual player/blocker hitbox.")]
    [SerializeField] private BoxCollider hitTrigger;
    [SerializeField] private int destroyLayer = 8;
    [SerializeField] private int playerHazardLayer = 7;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody ratRb;
    private bool isReloadingScene;

    private void Awake()
    {
        gameObject.layer = playerHazardLayer;
        ratRb = GetComponent<Rigidbody>();

        if (ratRb == null)
        {
            ratRb = gameObject.AddComponent<Rigidbody>();
        }

        ratRb.useGravity = false;
        ratRb.isKinematic = true;
        ratRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        ratRb.interpolation = RigidbodyInterpolation.Interpolate;

        SetupAssignedTrigger();
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

    public void HandleTriggerHit(GameObject other)
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

    private void SetupAssignedTrigger()
    {
        if (hitTrigger == null)
        {
            Debug.LogWarning($"{name} is missing a rat hit trigger. Assign a BoxCollider trigger on the RatMover component.", this);
            return;
        }

        hitTrigger.isTrigger = true;
        hitTrigger.gameObject.layer = playerHazardLayer;

        RatHitTrigger triggerForwarder = hitTrigger.GetComponent<RatHitTrigger>();

        if (triggerForwarder == null)
        {
            triggerForwarder = hitTrigger.gameObject.AddComponent<RatHitTrigger>();
        }

        triggerForwarder.Initialize(this);
    }
}
