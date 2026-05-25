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

    [Header("Gameplay UI")]
    [SerializeField] private GameObject pauseButton;

    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text finalRunCoinsText;
    [SerializeField] private TMP_Text finalTotalCoinsText;
    [SerializeField] private TMP_Text newRecordText;

    private int[] topScores;
    private bool isGameOver;
    private bool isPaused;
    private bool isGameplayActive;

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

        HideGameplayUi();
        ShowMainMenu();

        if (shouldStartImmediatelyAfterReload)
        {
            shouldStartImmediatelyAfterReload = false;
            StartGame();
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        isGameOver = false;
        isPaused = false;
        isGameplayActive = true;

        topScores = HighScoreStore.LoadTopScores();

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
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

    public void PauseGame()
    {
        if (!isGameplayActive || isGameOver || isPaused)
        {
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        if (!isPaused || isGameOver)
        {
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        Time.timeScale = 1f;

        isGameOver = true;
        isPaused = false;
        isGameplayActive = false;

        SetGameplayActive(false);
        HideGameplayUi();

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
        Time.timeScale = 1f;
        shouldStartImmediatelyAfterReload = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        shouldStartImmediatelyAfterReload = false;
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

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
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

        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isNewBestRecord);

            if (isNewBestRecord)
            {
                newRecordText.text = "New record!";
            }
        }

        Debug.Log($"Game Over. Final score: {finalScore}");
        Debug.Log($"Run coins: {(coinManager != null ? coinManager.RunCoins : 0)}");
        Debug.Log($"Total coins: {(coinManager != null ? coinManager.TotalCoins : 0)}");
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

    private void HideGameplayUi()
    {
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
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

    [ContextMenu("Debug/Clear Total Coins")]
    private void ClearTotalCoinsForDebug()
    {
        if (coinManager != null)
        {
            coinManager.ClearTotalCoins();
        }

        Debug.Log("Total coins cleared.");
    }
}