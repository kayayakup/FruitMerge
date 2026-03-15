// ============================================================
//  ScoreManager.cs
// ============================================================
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    private const string KEY_BEST = "MF_Best";

    public int CurrentScore { get; private set; }
    public int BestScore    { get; private set; }

    /// Fires after every score change: (current, best)
    public event Action<int, int> OnScoreChanged;

    public void Initialize()
    {
        CurrentScore = 0;
        BestScore    = PlayerPrefs.GetInt(KEY_BEST, 0);
    }

    public void AddScore(int pts)
    {
        CurrentScore += pts;
        if (CurrentScore > BestScore)
        {
            BestScore = CurrentScore;
            PlayerPrefs.SetInt(KEY_BEST, BestScore);
            PlayerPrefs.Save();
        }
        OnScoreChanged?.Invoke(CurrentScore, BestScore);
    }

    public void Reset()
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(CurrentScore, BestScore);
    }
}