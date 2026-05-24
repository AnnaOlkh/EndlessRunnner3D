using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    private static bool shouldStartImmediatelyAfterReload;

    [Header("Gameplay")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Main Menu UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TMP_Text menuRecordsText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

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

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (scoreManager != null)
        {
            int bestScore = topScores.Length > 0 ? topScores[0] : 0;
            scoreManager.BeginScoring(bestScore);
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

        HighScoreStore.TryAddScore(finalScore, out topScores, out _);

        ShowGameOver(finalScore);
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

        if (menuRecordsText != null)
        {
            menuRecordsText.text = HighScoreStore.FormatTopScores(topScores);
        }
    }

    private void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Your score: {finalScore}";
        }

        Debug.Log($"Game Over. Final score: {finalScore}");
    }

    private void SetGameplayActive(bool active)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = active;
        }
    }
}