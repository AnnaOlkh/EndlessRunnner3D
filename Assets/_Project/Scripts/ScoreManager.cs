using TMPro;
using UnityEngine;

public sealed class ScoreManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float scoreMultiplier = 1f;

    [SerializeField] private Color normalScoreColor = Color.white;
    [SerializeField] private Color recordScoreColor = new Color(1f, 0.84f, 0f);

    private float startZ;
    private int currentScore;
    private int lastDisplayedScore = -1;
    private int scoreToBeat;
    private bool isScoring;
    private bool recordColorApplied;

    public int CurrentScore => currentScore;

    private void Awake()
    {
        HideScore();
    }

    public void BeginScoring(int currentBestScore)
    {
        scoreToBeat = currentBestScore;
        startZ = player != null ? player.position.z : 0f;

        currentScore = 0;
        lastDisplayedScore = -1;
        recordColorApplied = false;
        isScoring = true;

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.color = normalScoreColor;
        }

        SetScoreText(0);
    }

    public void HideScore()
    {
        isScoring = false;

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isScoring || player == null || scoreText == null)
        {
            return;
        }

        float distance = Mathf.Max(0f, player.position.z - startZ);
        currentScore = Mathf.FloorToInt(distance * scoreMultiplier);

        if (scoreToBeat > 0 && currentScore > scoreToBeat && !recordColorApplied)
        {
            scoreText.color = recordScoreColor;
            recordColorApplied = true;
        }

        if (currentScore == lastDisplayedScore)
        {
            return;
        }

        SetScoreText(currentScore);
    }

    private void SetScoreText(int score)
    {
        lastDisplayedScore = score;
        scoreText.text = $"Score: {score}";
    }
}