using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public float playerScore;
    public static PlayerScore Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        Debug.Log(playerScore);
    }
}
