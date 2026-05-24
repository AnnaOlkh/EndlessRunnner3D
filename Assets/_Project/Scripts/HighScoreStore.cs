using UnityEngine;

public static class HighScoreStore
{
    private const string KeyPrefix = "PB_";
    private const int ScoreCount = 3;

    public static int[] LoadTopScores()
    {
        int[] scores = new int[ScoreCount];

        for (int i = 0; i < ScoreCount; i++)
        {
            scores[i] = Mathf.Max(0, PlayerPrefs.GetInt(KeyPrefix + i, 0));
        }

        return scores;
    }

    public static bool TryAddScore(int score, out int[] updatedScores, out int rank)
    {
        int[] scores = LoadTopScores();

        rank = -1;

        for (int i = 0; i < ScoreCount; i++)
        {
            if (score > scores[i])
            {
                rank = i;
                break;
            }
        }

        if (rank == -1)
        {
            updatedScores = scores;
            return false;
        }

        for (int i = ScoreCount - 1; i > rank; i--)
        {
            scores[i] = scores[i - 1];
        }

        scores[rank] = score;

        SaveTopScores(scores);

        updatedScores = scores;
        return true;
    }

    public static string FormatTopScores(int[] scores)
    {
        return
            $"PB:\n" +
            $"1. {scores[0]}\n" +
            $"2. {scores[1]}\n" +
            $"3. {scores[2]}";
    }

    private static void SaveTopScores(int[] scores)
    {
        for (int i = 0; i < ScoreCount; i++)
        {
            PlayerPrefs.SetInt(KeyPrefix + i, scores[i]);
        }

        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        for (int i = 0; i < ScoreCount; i++)
        {
            PlayerPrefs.DeleteKey(KeyPrefix + i);
        }

        PlayerPrefs.Save();
    }
}