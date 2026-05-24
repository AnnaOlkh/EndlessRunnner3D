using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    private static bool shouldStartImmediatelyAfterReload;

    [Header("Gameplay")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private CoinManager coinManager;

    [Header("Main Menu UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TMP_Text menuRecordsText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text newRecordText;
    [SerializeField] private TMP_Text finalRunCoinsText;
    [SerializeField] private TMP_Text finalTotalCoinsText;


    private int[] topScores;
    private bool isGameOver;

    private void Awake()
    {
        topScores = HighScoreStore.LoadTopScores();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        SetGameplayActive(false);

        if (scoreManager != null)
        {
            scoreManager.HideScore();
        }
        if (coinManager != null)
        {
            coinManager.HideCoins();
        }

        ShowMainMenu();

        if (shouldStartImmediatelyAfterReload)
        {
            shouldStartImmediatelyAfterReload = false;
            StartGame();
        }
    }

    public void StartGame()
    {
        isGameOver = false;
        topScores = HighScoreStore.LoadTopScores();

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(false);
        }

        if (scoreManager != null)
        {
            int bestScore = topScores.Length > 0 ? topScores[0] : 0;
            scoreManager.BeginScoring(bestScore);
        }
        if (coinManager != null)
        {
            coinManager.BeginCounting();
        }

        SetGameplayActive(true);
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        SetGameplayActive(false);

        int finalScore = scoreManager != null ? scoreManager.CurrentScore : 0;

        if (scoreManager != null)
        {
            scoreManager.HideScore();
        }
        if (coinManager != null)
        {
            coinManager.HideCoins();
        }

        int oldBestScore = topScores.Length > 0 ? topScores[0] : 0;

        bool enteredTopScores = HighScoreStore.TryAddScore(
            finalScore,
            out topScores,
            out int rank
        );

        bool isNewBestRecord = enteredTopScores && rank == 0 && finalScore > oldBestScore;

        ShowGameOver(finalScore, isNewBestRecord);
    }

    public void RestartRun()
    {
        shouldStartImmediatelyAfterReload = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(false);
        }

        UpdateMenuRecordsText();
    }

    private void ShowGameOver(int finalScore, bool isNewBestRecord)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Your score: {finalScore}";
        }

        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isNewBestRecord);

            if (isNewBestRecord)
            {
                newRecordText.text = $"New record: {finalScore}";
            }
        }
        if (finalRunCoinsText != null)
        {
            int runCoins = coinManager != null ? coinManager.RunCoins : 0;
            finalRunCoinsText.text = $"Coins collected: {runCoins}";
        }

        if (finalTotalCoinsText != null)
        {
            int totalCoins = coinManager != null ? coinManager.TotalCoins : 0;
            finalTotalCoinsText.text = $"Total coins: {totalCoins}";
        }

        Debug.Log($"Game Over. Final score: {finalScore}");
        Debug.Log(HighScoreStore.FormatTopScores(topScores));
    }

    private void UpdateMenuRecordsText()
    {
        if (menuRecordsText == null)
        {
            return;
        }

        menuRecordsText.text = HighScoreStore.FormatTopScores(topScores);
    }

    private void SetGameplayActive(bool active)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = active;
        }
    }

    [ContextMenu("Debug/Log Top Scores")]
    private void LogTopScoresForDebug()
    {
        int[] scores = HighScoreStore.LoadTopScores();
        Debug.Log(HighScoreStore.FormatTopScores(scores));
    }

    [ContextMenu("Debug/Clear Top Scores")]
    private void ClearTopScoresForDebug()
    {
        HighScoreStore.Clear();
        topScores = HighScoreStore.LoadTopScores();
        UpdateMenuRecordsText();
        Debug.Log("Top scores cleared.");
    }
}