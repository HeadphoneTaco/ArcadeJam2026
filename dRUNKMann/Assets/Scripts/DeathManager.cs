using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    [SerializeField] private int hazardLayer = 7;
    [SerializeField] private float sceneReloadDelay = 0.25f;

    private bool isReloadingScene;

    private void OnCollisionEnter(Collision collision)
    {
        if (isReloadingScene || collision.gameObject.layer != hazardLayer)
        {
            return;
        }

        isReloadingScene = true;
        PlayHitSfx(collision.gameObject);
        GameSfxPlayer.PlayDeathSfx();
        StartCoroutine(ReloadActiveSceneAfterDelay());
    }

    private void PlayHitSfx(GameObject hazard)
    {
        if (hazard.GetComponentInParent<RatMover>() != null || hazard.transform.root.name.ToLowerInvariant().Contains("rat"))
        {
            GameSfxPlayer.PlayRatHitSfx();
            return;
        }

        GameSfxPlayer.PlayCarHitSfx();
    }

    private IEnumerator ReloadActiveSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneReloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
