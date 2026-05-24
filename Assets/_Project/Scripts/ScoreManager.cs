using TMPro;
using UnityEngine;

public sealed class ScoreManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text recordProgressText;
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
        scoreToBeat = Mathf.Max(0, currentBestScore);
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

        if (recordProgressText != null)
        {
            recordProgressText.gameObject.SetActive(true);
            recordProgressText.color = normalScoreColor;
        }

        SetScoreText(0);
        SetRecordProgressText(0);
    }

    public void HideScore()
    {
        isScoring = false;

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }

        if (recordProgressText != null)
        {
            recordProgressText.gameObject.SetActive(false);
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

            if (recordProgressText != null)
            {
                recordProgressText.color = recordScoreColor;
            }

            recordColorApplied = true;
        }

        if (currentScore == lastDisplayedScore)
        {
            return;
        }

        SetScoreText(currentScore);
        SetRecordProgressText(currentScore);
    }

    private void SetScoreText(int score)
    {
        lastDisplayedScore = score;
        scoreText.text = $"Score: {score}";
    }

    private void SetRecordProgressText(int score)
    {
        if (recordProgressText == null)
        {
            return;
        }

        if (scoreToBeat <= 0)
        {
            recordProgressText.text = "Set first record";
            return;
        }

        int remaining = scoreToBeat - score;

        if (remaining > 0)
        {
            recordProgressText.text = $"To record: {remaining}";
            return;
        }

        recordProgressText.text = "Record beaten";
    }
}