using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateScoreText : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text playerSpeedText;

    void Update()
    {
        scoreText.text = Mathf.Round(PlayerScore.Instance.playerScore).ToString();
        playerSpeedText.text = "Player Speed x " + PlayerScore.Instance.playerSpeedModifier;
    }
}
