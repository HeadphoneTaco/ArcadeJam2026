using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateScoreText : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text playerSpeedText;
    [SerializeField] private TMP_Text bottleCountText;

    void Update()
    {
        scoreText.text = Mathf.FloorToInt(PlayerScore.Instance.playerScore).ToString();
        playerSpeedText.text = "Player Speed x " + PlayerScore.Instance.playerSpeedModifier.ToString("0.0");

        if (bottleCountText != null)
        {
            bottleCountText.text = PlayerScore.Instance.bottleCount.ToString();
        }
    }
}
