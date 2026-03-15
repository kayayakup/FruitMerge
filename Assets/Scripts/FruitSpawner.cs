// ============================================================
//  FruitSpawner.cs  —  held-fruit control, drop, cooldown
// ============================================================
using UnityEngine;
using System.Collections;

public class FruitSpawner : MonoBehaviour
{
    public static FruitSpawner Instance { get; private set; }

    public  Fruit HeldFruit    { get; private set; }
    public  int   NextTypeIdx  { get; private set; }
    public  bool  CanDrop      => !_dropping
                               && GameManager.Instance?.State == GameState.Playing
                               && HeldFruit != null;

    private FruitPool    pool;
    private ScoreManager scoreMgr;
    private bool         _dropping;
    private float        _currentX;
    private int          _maxIdx;
    private int          _pendingNextIdx = -1;   // pre-rolled next type
    private LineRenderer dropLine;

    // ─────────────────────────────────────────────────────────
    public void Initialize(FruitPool fp, ScoreManager sm)
    {
        Instance = this; pool = fp; scoreMgr = sm;
        _maxIdx   = GameConfig.InitMaxIdx;
        _currentX = 0f;
        BuildDropLine();
        scoreMgr.OnScoreChanged += OnScoreChanged;
    }

    public void ResetSpawner()
    {
        _dropping = false; _maxIdx = GameConfig.InitMaxIdx;
        _pendingNextIdx = -1;
        if (HeldFruit != null) { pool.Return(HeldFruit); HeldFruit = null; }
        SetDropLineActive(false);
    }

    // ── Called by GameManager after state = Playing ───────────
    public void SpawnNextHeld()
    {
        if (GameManager.Instance?.State != GameState.Playing) return;
        if (HeldFruit != null) return;

        // Use the pre-rolled next type if available, otherwise generate a new one
        int spawnIdx;
        if (_pendingNextIdx >= 0)
        {
            spawnIdx = _pendingNextIdx;
        }
        else
        {
            spawnIdx = Random.Range(0, Mathf.Min(_maxIdx + 1, GameConfig.MaxSpawnIdx + 1));
        }

        HeldFruit = pool.Get((FruitType)spawnIdx);
        HeldFruit.Spawn(new Vector3(_currentX, GameConfig.SpawnY, 0f), true);
        UpdateDropLine();

        // Pre-roll the NEXT type and show it in the preview
        _pendingNextIdx = Random.Range(0, Mathf.Min(_maxIdx + 1, GameConfig.MaxSpawnIdx + 1));
        NextTypeIdx = _pendingNextIdx;
        UIManager.Instance?.RefreshNextPreview(NextTypeIdx);
    }

    // ── Called by InputHandler ────────────────────────────────
    public void SetPointerX(float worldX)
    {
        float r = HeldFruit != null ? HeldFruit.Data.radius : 0.3f;
        _currentX = Mathf.Clamp(worldX,
                                -GameConfig.HalfWidth + r,
                                 GameConfig.HalfWidth - r);
        if (HeldFruit != null)
        {
            HeldFruit.transform.position = new Vector3(_currentX, GameConfig.SpawnY, 0f);
            UpdateDropLine();
        }
    }

    public void Drop()
    {
        if (!CanDrop) return;
        StartCoroutine(DropRoutine());
    }

    // ── Private ───────────────────────────────────────────────
    private IEnumerator DropRoutine()
    {
        _dropping = true;
        var f = HeldFruit; HeldFruit = null;
        SetDropLineActive(false);
        f.Release();
        yield return new WaitForSeconds(GameConfig.SpawnCooldown);
        _dropping = false;
        SpawnNextHeld();
    }

    private void OnScoreChanged(int cur, int _)
    {
        int tier = Mathf.Min(cur / GameConfig.ScorePerTier, GameConfig.MaxSpawnIdx);
        _maxIdx   = Mathf.Max(_maxIdx, Mathf.Min(GameConfig.InitMaxIdx + tier, GameConfig.MaxSpawnIdx));
    }

    // ── Drop guide line ───────────────────────────────────────
    private void BuildDropLine()
    {
        var go = new GameObject("DropLine"); go.transform.SetParent(transform);
        dropLine = go.AddComponent<LineRenderer>();
        dropLine.positionCount = 2;
        dropLine.startWidth    = dropLine.endWidth = 0.025f;
        dropLine.useWorldSpace = true;
        dropLine.sortingOrder  = 9;
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 1f, 1f, 0.35f);
        dropLine.material = mat;
        dropLine.enabled  = false;
    }

    private void UpdateDropLine()
    {
        if (dropLine == null) return;
        dropLine.enabled = HeldFruit != null;
        dropLine.SetPosition(0, new Vector3(_currentX, GameConfig.SpawnY - 0.2f, 0f));
        dropLine.SetPosition(1, new Vector3(_currentX, GameConfig.FloorY,          0f));
    }

    private void SetDropLineActive(bool v) { if (dropLine != null) dropLine.enabled = v; }
}