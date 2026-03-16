// ============================================================
//  Bootstrap.cs  —  ATTACH THIS TO THE SINGLE EMPTY GAMEOBJECT
//  Creates every system, camera, wall, and UI element at runtime.
// ============================================================
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [Tooltip("11 sprites in order: Cherry, Strawberry, Grape, Orange, Apple, " +
             "Pear, Peach, Pineapple, Watermelon, Melon, Durian")]
    public Sprite[] fruitSprites = new Sprite[11];
    public Sprite   basketBottomSprite;

    [Tooltip("Audio Clips for Game Events")]
    public AudioClip clipGameStart;
    public AudioClip clipFruitDrop;
    public AudioClip clipFruitMerge;
    public AudioClip clipFruitCollide;
    public AudioClip clipGameOver;
    public AudioClip clipButtonClick;
    public AudioClip clipBackgroundMusic;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Physics2D.gravity = new Vector2(0f, GameConfig.GravityY);

        // ── Phase 1: instantiate all manager MonoBehaviours ──
        var audioMgr  = Spawn<AudioManager>   ("AudioManager");
        var camSetup  = Spawn<CameraSetup>    ("CameraSetup");
        var scoreMgr  = Spawn<ScoreManager>   ("ScoreManager");
        var boundary  = Spawn<BoundaryManager>("BoundaryManager");
        var bubbleBg  = Spawn<BubbleBackground>("BubbleBackground");
        var pool      = Spawn<FruitPool>      ("FruitPool");
        var mergeMgr  = Spawn<MergeManager>   ("MergeManager");
        var spawner   = Spawn<FruitSpawner>   ("FruitSpawner");
        var uiMgr     = Spawn<UIManager>      ("UIManager");
        var inputH    = Spawn<InputHandler>   ("InputHandler");
        var ssMgr     = Spawn<ScreenshotManager>("ScreenshotManager");
        var gameMgr   = Spawn<GameManager>    ("GameManager");   // last

        // ── Phase 2: initialize in dependency order ───────────
        audioMgr .Initialize(clipGameStart, clipFruitDrop, clipFruitMerge, 
                             clipFruitCollide, clipGameOver, clipButtonClick, clipBackgroundMusic);
        camSetup .Initialize();
        scoreMgr .Initialize();
        boundary .Initialize(basketBottomSprite);
        bubbleBg .Initialize();
        pool     .Initialize(fruitSprites);
        mergeMgr .Initialize(pool, scoreMgr);
        pool     .InjectMergeManager(mergeMgr);
        spawner  .Initialize(pool, scoreMgr);
        ssMgr    .Initialize(camSetup.MainCamera);
        uiMgr    .Initialize(scoreMgr, spawner);
        inputH   .Initialize(spawner);
        gameMgr  .Initialize(scoreMgr, spawner, uiMgr, ssMgr);  // starts game
    }

    private T Spawn<T>(string n) where T : MonoBehaviour
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform);
        return go.AddComponent<T>();
    }
}