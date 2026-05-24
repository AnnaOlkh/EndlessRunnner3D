using System.Collections.Generic;
using UnityEngine;

public static class HighScoreStore
{
    private const string KeyPrefix = "PB_";
    private const int ScoreCount = 3;

    public static int[] LoadTopScores()
    {
        List<int> uniqueScores = new List<int>();

        for (int i = 0; i < ScoreCount; i++)
        {
            int score = Mathf.Max(0, PlayerPrefs.GetInt(KeyPrefix + i, 0));

            if (score <= 0)
            {
                continue;
            }

            if (!uniqueScores.Contains(score))
            {
                uniqueScores.Add(score);
            }
        }

        uniqueScores.Sort((left, right) => right.CompareTo(left));

        int[] scores = new int[ScoreCount];

        for (int i = 0; i < Mathf.Min(ScoreCount, uniqueScores.Count); i++)
        {
            scores[i] = uniqueScores[i];
        }

        return scores;
    }

    public static bool TryAddScore(int score, out int[] updatedScores, out int rank)
    {
        int[] currentScores = LoadTopScores();

        rank = -1;

        if (score <= 0)
        {
            updatedScores = currentScores;
            return false;
        }

        if (ContainsScore(currentScores, score))
        {
            updatedScores = currentScores;
            return false;
        }

        List<int> candidates = new List<int>();

        for (int i = 0; i < currentScores.Length; i++)
        {
            if (currentScores[i] > 0)
            {
                candidates.Add(currentScores[i]);
            }
        }

        candidates.Add(score);
        candidates.Sort((left, right) => right.CompareTo(left));

        int[] newScores = new int[ScoreCount];

        for (int i = 0; i < Mathf.Min(ScoreCount, candidates.Count); i++)
        {
            newScores[i] = candidates[i];
        }

        for (int i = 0; i < ScoreCount; i++)
        {
            if (newScores[i] == score)
            {
                rank = i;
                break;
            }
        }

        if (rank == -1)
        {
            updatedScores = currentScores;
            return false;
        }

        SaveTopScores(newScores);

        updatedScores = newScores;
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

    public static void Clear()
    {
        for (int i = 0; i < ScoreCount; i++)
        {
            PlayerPrefs.DeleteKey(KeyPrefix + i);
        }

        PlayerPrefs.Save();
    }

    private static bool ContainsScore(int[] scores, int targetScore)
    {
        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] == targetScore)
            {
                return true;
            }
        }

        return false;
    }

    private static void SaveTopScores(int[] scores)
    {
        for (int i = 0; i < ScoreCount; i++)
        {
            PlayerPrefs.SetInt(KeyPrefix + i, scores[i]);
        }

        PlayerPrefs.Save();
    }
}