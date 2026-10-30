// ============================================================
//  GameManager.cs  —  state machine + game-over detection
// ============================================================
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum GameState { Idle, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    private const float InterstitialIntervalSeconds = 180f;
    private const int InterstitialWarningSeconds = 3;

    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Idle;
    public event Action<GameState> OnStateChanged;

    // ── Dependencies ─────────────────────────────────────────
    private ScoreManager      scoreMgr;
    private FruitSpawner      spawner;
    private UIManager         uiMgr;
    private ScreenshotManager ssMgr;

    // ── Game-over tracking ───────────────────────────────────
    private readonly List<Fruit> activeFruits = new();
    private bool gameOverInProgress;
    private bool rewardedContinuePending;
    private bool rewardedContinueEarned;
    private float timeScaleBeforeGameOver = 1f;

    public bool IsRewardedContinuePending => rewardedContinuePending;

    // ─────────────────────────────────────────────────────────
    public void Initialize(ScoreManager sm, FruitSpawner sp,
                           UIManager ui, ScreenshotManager ss)
    {
        Instance = this;
        scoreMgr = sm; spawner = sp; uiMgr = ui; ssMgr = ss;
        StartCoroutine(TimedInterstitialRoutine());
        StartGame();
    }

    // ── Public API ────────────────────────────────────────────
    public void StartGame()
    {
        Time.timeScale = 1f;
        gameOverInProgress = false;
        rewardedContinuePending = false;
        rewardedContinueEarned = false;

        // Return all active fruits to pool before clearing
        var pool = FruitPool.Instance;
        if (pool != null)
        {
            for (int i = activeFruits.Count - 1; i >= 0; i--)
            {
                if (activeFruits[i] != null && activeFruits[i].gameObject.activeSelf)
                    pool.Return(activeFruits[i]);
            }
        }
        activeFruits.Clear();
        scoreMgr.Reset();
        spawner.ResetSpawner();
        uiMgr.HideGameOver();
        SetState(GameState.Playing);
        
        AudioManager.Instance?.PlayGameStart();
        
        spawner.SpawnNextHeld();
    }

    public void TriggerGameOver()
    {
        if (State != GameState.Playing || gameOverInProgress) return;

        gameOverInProgress = true;
        timeScaleBeforeGameOver = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        SetState(GameState.GameOver);
        GoogleAdMobController.Instance?.LoadRewarded();
        StartCoroutine(GameOverRoutine());
    }

    public bool TryContinueWithRewarded()
    {
        if (State != GameState.GameOver || rewardedContinuePending)
            return false;

        RemoveFruitsOutsideBasket();

        GoogleAdMobController adMob = GoogleAdMobController.Instance;
        if (adMob == null || !adMob.IsRewardedConfigured)
        {
            Debug.LogWarning("[Game] Rewarded continue is unavailable: no ad unit ID is configured.");
            return false;
        }

        if (!adMob.IsRewardedReady())
        {
            adMob.LoadRewarded();
            return false;
        }

        rewardedContinuePending = true;
        rewardedContinueEarned = false;

        if (adMob.ShowRewarded(OnRewardedContinueEarned, OnRewardedContinueClosed))
            return true;

        rewardedContinuePending = false;
        return false;
    }

    private void OnRewardedContinueEarned(GoogleMobileAds.Api.Reward _)
    {
        rewardedContinueEarned = true;
    }

    private void OnRewardedContinueClosed()
    {
        bool shouldContinue = rewardedContinuePending && rewardedContinueEarned;
        rewardedContinuePending = false;
        rewardedContinueEarned = false;

        if (shouldContinue)
            ContinueAfterRewarded();
    }

    private void ContinueAfterRewarded()
    {
        if (State != GameState.GameOver)
            return;

        FruitPool pool = FruitPool.Instance;
        for (int i = activeFruits.Count - 1; i >= 0; i--)
        {
            Fruit fruit = activeFruits[i];
            if (fruit == null || !fruit.gameObject.activeSelf)
                continue;

            pool?.DowngradeOneTier(fruit);
        }

        Time.timeScale = timeScaleBeforeGameOver;
        gameOverInProgress = false;
        uiMgr.HideGameOver();
        SetState(GameState.Playing);
    }

    public void RegisterFruit  (Fruit f) { if (!activeFruits.Contains(f)) activeFruits.Add(f); }
    public void UnregisterFruit(Fruit f) => activeFruits.Remove(f);

    private void RemoveFruitsOutsideBasket()
    {
        FruitPool pool = FruitPool.Instance;
        Fruit[] sceneFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);

        foreach (Fruit fruit in sceneFruits)
        {
            if (fruit == null || fruit.IsHeld || !IsOutsideBasket(fruit))
                continue;

            if (pool != null)
                pool.Return(fruit);
            else
            {
                UnregisterFruit(fruit);
                Destroy(fruit.gameObject);
            }
        }
    }

    // ── Game-over line check (every frame) ──────────────────
    private void Update()
    {
        if (State != GameState.Playing) return;

        for (int i = activeFruits.Count - 1; i >= 0; i--)
        {
            Fruit fruit = activeFruits[i];
            if (fruit == null || !fruit.gameObject.activeSelf)
            {
                activeFruits.RemoveAt(i);
                continue;
            }

            if (fruit.IsHeld) continue;

            if (IsBelowBasketFloor(fruit))
            {
                TriggerGameOver();
                return;
            }
        }
    }

    private static bool IsOutsideBasket(Fruit fruit)
    {
        float x = fruit.transform.position.x;
        return x < -GameConfig.HalfWidth || x > GameConfig.HalfWidth;
    }

    private static bool IsBelowBasketFloor(Fruit fruit)
    {
        return fruit.WorldBounds.max.y < GameConfig.FloorY;
    }

    // ── Helpers ───────────────────────────────────────────────
    private IEnumerator GameOverRoutine()
    {
        AudioManager.Instance?.PlayGameOver();
        yield return new WaitForEndOfFrame();
        var shot = ssMgr.Capture();
        uiMgr.ShowGameOver(shot, scoreMgr.CurrentScore, scoreMgr.BestScore);
    }

    private IEnumerator TimedInterstitialRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(
                InterstitialIntervalSeconds - InterstitialWarningSeconds);

            while (State != GameState.Playing)
                yield return new WaitForSecondsRealtime(0.25f);

            for (int seconds = InterstitialWarningSeconds; seconds > 0; seconds--)
            {
                if (State != GameState.Playing)
                    break;

                uiMgr.ShowInterstitialWarning(seconds);
                yield return new WaitForSecondsRealtime(1f);
            }

            uiMgr.HideInterstitialWarning();

            if (State == GameState.Playing)
                GoogleAdMobController.Instance?.ShowInterstitialAd();
        }
    }

    private void SetState(GameState s) { State = s; OnStateChanged?.Invoke(s); }
}