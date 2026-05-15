using UnityEngine;

public class RatHitTrigger : MonoBehaviour
{
    private RatMover owner;

    public void Initialize(RatMover ratMover)
    {
        owner = ratMover;
    }

    private void OnTriggerEnter(Collider other)
    {
        owner?.HandleTriggerHit(other.gameObject);
    }
}
