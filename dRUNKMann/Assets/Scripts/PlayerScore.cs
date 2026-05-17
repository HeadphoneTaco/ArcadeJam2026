using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public float playerScore;
    public int bottleCount;
    public float playerSpeedModifier;
    [SerializeField] private BasicMovementScript basicMovement;
    [SerializeField] private float playerSpeedModifierMultiplier = .05f;
    public static PlayerScore Instance { get; private set; }
    private float defaultPlayerSpeed;
    private float defaultPlayerMaxVelocity;
    private float defaultPlayerMaxDiveVelocity;

    private float nextUpgradeThreshold = 10f;

    private void Start()
    {
        playerSpeedModifier = 1f;
        defaultPlayerSpeed = basicMovement.moveSpeed;
        defaultPlayerMaxVelocity = basicMovement.maxVelocity;
        defaultPlayerMaxDiveVelocity = basicMovement.maxDiveVelocity;
    }

    void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        IncreasePlayerSpeed();
    }

    public void AddScore(float amount)
    {
        playerScore += amount;
        IncreasePlayerSpeed();
    }

    public void AddBottle()
    {
        bottleCount++;
    }

    void IncreasePlayerSpeed()
    {
        while (playerScore >= nextUpgradeThreshold)
        {
            playerSpeedModifier += playerSpeedModifierMultiplier;
            basicMovement.moveSpeed = defaultPlayerSpeed * playerSpeedModifier;
            basicMovement.maxVelocity = defaultPlayerMaxVelocity * playerSpeedModifier;
            basicMovement.maxDiveVelocity = defaultPlayerMaxDiveVelocity * playerSpeedModifier;
            nextUpgradeThreshold += 10f;
        }
    }
}
