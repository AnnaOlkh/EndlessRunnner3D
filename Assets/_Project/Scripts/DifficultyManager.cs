using UnityEngine;

public sealed class DifficultyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private RoadSpawner roadSpawner;

    [Header("Progression")]
    [SerializeField] private float scoreForMaxDifficulty = 1000f;
    [SerializeField] private int updateStep = 50;

    [Header("Speed")]
    [SerializeField] private float minForwardSpeed = 8f;
    [SerializeField] private float maxForwardSpeed = 18f;

    [Header("Obstacles")]
    [SerializeField] private int minObstacleGroupsPerSegment = 1;
    [SerializeField] private int maxObstacleGroupsPerSegment = 4;

    [SerializeField, Range(0f, 1f)] private float minObstacleChancePerGroup = 0.55f;
    [SerializeField, Range(0f, 1f)] private float maxObstacleChancePerGroup = 0.95f;

    [SerializeField, Range(0f, 1f)] private float minTwoLaneObstacleChance = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maxTwoLaneObstacleChance = 0.8f;

    private int lastAppliedScoreStep = -1;

    private void Start()
    {
        ApplyDifficulty(0);
    }

    private void Update()
    {
        if (scoreManager == null)
        {
            return;
        }

        int currentScore = scoreManager.CurrentScore;
        int currentScoreStep = currentScore / Mathf.Max(1, updateStep);

        if (currentScoreStep == lastAppliedScoreStep)
        {
            return;
        }

        lastAppliedScoreStep = currentScoreStep;
        ApplyDifficulty(currentScore);
    }

    private void ApplyDifficulty(int score)
    {
        float difficulty = Mathf.Clamp01(score / scoreForMaxDifficulty);

        float forwardSpeed = Mathf.Lerp(
            minForwardSpeed,
            maxForwardSpeed,
            difficulty
        );

        int obstacleGroups = Mathf.RoundToInt(
            Mathf.Lerp(
                minObstacleGroupsPerSegment,
                maxObstacleGroupsPerSegment,
                difficulty
            )
        );

        float obstacleChance = Mathf.Lerp(
            minObstacleChancePerGroup,
            maxObstacleChancePerGroup,
            difficulty
        );

        float twoLaneChance = Mathf.Lerp(
            minTwoLaneObstacleChance,
            maxTwoLaneObstacleChance,
            difficulty
        );

        if (playerMovement != null)
        {
            playerMovement.SetForwardSpeed(forwardSpeed);
        }

        if (roadSpawner != null)
        {
            roadSpawner.ConfigureDifficulty(
                obstacleGroups,
                obstacleChance,
                twoLaneChance
            );
        }
    }
}