using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateScoreText : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    void Update()
    {
        scoreText.text = Mathf.Round(PlayerScore.Instance.playerScore).ToString();
    }
}
