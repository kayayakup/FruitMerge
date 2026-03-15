// ============================================================
//  GameConfig.cs  —  all tunable constants in one place
// ============================================================
public static class GameConfig
{
    // ── Container ─────────────────────────────────────────────
    public const float HalfWidth       = 8.625f;  // New half-width (swapped with height)
    public const float WallThick       = 0.5f;    // Thicker walls to prevent tunnel through
    public const float BottomCenterY   = -12.0f;  // Lowered
    public const float FloorY          = -11.75f; // Lowered

    // ── Key y-levels ─────────────────────────────────────────
    public const float GameOverLineY   = 6.87f;   // Lowered (basket top is this + 3)
    public const float SpawnY          = 14.87f;  // 5 units above wall top (6.87 + 3 + 5)

    // ── Camera ───────────────────────────────────────────────
    public const float CamOrthoSize   = 24.0f;    // Increased to show higher spawn
    public const float CamY           = 1.5f;     // Lowered to follow lowered basket

    // ── Physics ──────────────────────────────────────────────
    public const float GravityY       = -22f;    // applied via Physics2D.gravity
    public const float GravityScale   = 1f;
    public const float LinearDamp     = 0.15f;
    public const float AngularDamp    = 0.9f;
    public const float Bounciness     = 0.15f;
    public const float Friction       = 0.5f;

    // ── Gameplay ─────────────────────────────────────────────
    public const float GameOverGrace  = 2.0f;    // secs a fruit must be over line → game over
    public const float SpawnCooldown  = 0.05f;   // secs between drops
    public const float MergeImmunity  = 0.40f;   // new fruit is immune to merging this long
    public const float HeldAlpha      = 0.85f;   // transparency of held fruit

    // ── Pool ─────────────────────────────────────────────────
    public const int   PoolSize       = 8;

    // ── Difficulty ───────────────────────────────────────────
    public const int   InitMaxIdx     = 3;   // spawn types 0–3 at start
    public const int   MaxSpawnIdx    = 5;   // never spawn above this index
    public const int   ScorePerTier   = 50;  // score needed to unlock one more spawn type

    // ── Animation ────────────────────────────────────────────
    public const float SpawnAnim      = 0.22f;
    public const float MergeAnim      = 0.16f;
    public const float BobAmplitude   = 0.032f;
    public const float BobFreq        = 2.2f;
}