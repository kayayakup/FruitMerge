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

    // ─────────────────────────────────────────────────────────
    public void Initialize(ScoreManager sm, FruitSpawner sp,
                           UIManager ui, ScreenshotManager ss)
    {
        Instance = this;
        scoreMgr = sm; spawner = sp; uiMgr = ui; ssMgr = ss;
        StartGame();
    }

    // ── Public API ────────────────────────────────────────────
    public void StartGame()
    {
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
        spawner.SpawnNextHeld();
    }

    public void TriggerGameOver()
    {
        if (State == GameState.GameOver) return;
        StartCoroutine(GameOverRoutine());
    }

    public void RegisterFruit  (Fruit f) { if (!activeFruits.Contains(f)) activeFruits.Add(f); }
    public void UnregisterFruit(Fruit f) => activeFruits.Remove(f);

    // ── Game-over line check (every frame) ──────────────────
    private void Update()
    {
        if (State != GameState.Playing) return;

        bool fallenOut = false;
        foreach (var f in activeFruits)
        {
            if (f == null || !f.gameObject.activeSelf) continue;
            if (f.IsHeld) continue;

            // Trigger game over if fruit falls significantly below the basket bottom
            if (f.transform.position.y < GameConfig.BottomCenterY - 10f)
            {
                fallenOut = true;
                break;
            }
        }

        if (fallenOut) TriggerGameOver();
    }

    // ── Helpers ───────────────────────────────────────────────
    private IEnumerator GameOverRoutine()
    {
        SetState(GameState.GameOver);
        yield return new WaitForEndOfFrame();
        var shot = ssMgr.Capture();
        uiMgr.ShowGameOver(shot, scoreMgr.CurrentScore, scoreMgr.BestScore);
    }

    private void SetState(GameState s) { State = s; OnStateChanged?.Invoke(s); }
}