using UnityEngine;

public class NearMiss : MonoBehaviour
{
    [SerializeField] private float nearMissScoreMultiplier;

    private bool isInNearMissZone;

    private void Start()
    {
        isInNearMissZone = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInNearMissZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInNearMissZone = false;

        }
    }

    private void Update()
    {
        if (isInNearMissZone)
        {
            PlayerScore.Instance.AddScore(nearMissScoreMultiplier * Time.deltaTime);
        }
    }
}
