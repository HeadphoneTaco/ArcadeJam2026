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
            PlayPassingSfx();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInNearMissZone = false;

        }
    }

    private void PlayPassingSfx()
    {
        if (GetComponentInParent<RatMover>() != null || transform.root.name.ToLowerInvariant().Contains("rat"))
        {
            GameSfxPlayer.PlayRatsPassingSfx();
            return;
        }

        GameSfxPlayer.PlayCarsPassingSfx();
    }

    private void Update()
    {
        if (isInNearMissZone)
        {
            PlayerScore.Instance.AddScore(nearMissScoreMultiplier * Time.deltaTime);
        }
    }
}
