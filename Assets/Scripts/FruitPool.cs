// ============================================================
//  FruitPool.cs  —  one pool per fruit type
// ============================================================
using UnityEngine;
using System.Collections.Generic;

public class FruitPool : MonoBehaviour
{
    public static FruitPool Instance { get; private set; }

    private Dictionary<FruitType, Queue<Fruit>>  pools;
    private Dictionary<FruitType, Sprite>        sprites;
    private Dictionary<FruitType, bool>          hasCustomSprite;
    private Dictionary<FruitType, float>         scaleFactors;
    private MergeManager                         mergeMgr;
    private PhysicsMaterial2D                    fruitMat;

    public void Initialize(Sprite[] assigned)
    {
        Instance = this;
        fruitMat = new PhysicsMaterial2D("FruitMat")
            { bounciness = GameConfig.Bounciness, friction = GameConfig.Friction };

        // Build sprite table + scale factors
        sprites       = new Dictionary<FruitType, Sprite>(FruitDatabase.Count);
        hasCustomSprite = new Dictionary<FruitType, bool>(FruitDatabase.Count);
        scaleFactors  = new Dictionary<FruitType, float>(FruitDatabase.Count);

        for (int i = 0; i < FruitDatabase.Count; i++)
        {
            var t    = (FruitType)i;
            var d    = FruitDatabase.Get(i);
            bool has = assigned != null && i < assigned.Length && assigned[i] != null;
            hasCustomSprite[t] = has;
            sprites[t] = has ? assigned[i] : SpriteFactory.Circle(d.primaryColor, d.edgeColor);

            // Calculate scale factor: desired world diameter = d.radius * 2
            // Sprite's natural size = bounds.size (at scale 1)
            if (has)
            {
                float spriteWorldSize = assigned[i].bounds.size.x; // natural width at scale 1
                float desiredDiameter = d.radius * 2f;
                scaleFactors[t] = desiredDiameter / spriteWorldSize;
            }
            else
            {
                // Procedural circle: PPU = size/2 → radius = 1 world unit at scale 1
                // So scale = desired radius
                scaleFactors[t] = d.radius;
            }
        }

        // Pre-warm pools
        pools = new Dictionary<FruitType, Queue<Fruit>>(FruitDatabase.Count);
        for (int i = 0; i < FruitDatabase.Count; i++)
        {
            var t = (FruitType)i;
            pools[t] = new Queue<Fruit>(GameConfig.PoolSize);
            for (int j = 0; j < GameConfig.PoolSize; j++)
                pools[t].Enqueue(CreateFruit(t));
        }
    }

    /// <summary>Called after MergeManager is ready to inject it into pooled fruits.</summary>
    public void InjectMergeManager(MergeManager mm)
    {
        mergeMgr = mm;
        foreach (var q in pools.Values)
            foreach (var f in q)
                f.SetMergeManager(mm);
    }

    // ── Public accessors ─────────────────────────────────────
    public Sprite GetSprite(FruitType type) => sprites[type];

    // ── Pool API ──────────────────────────────────────────────
    public Fruit Get(FruitType type)
    {
        if (pools[type].Count > 0) return pools[type].Dequeue();
        var nf = CreateFruit(type);
        nf.SetMergeManager(mergeMgr);
        return nf;
    }

    public void Return(Fruit f)
    {
        f.ReturnToPool();
        if (pools.TryGetValue(f.FruitType, out var q)) q.Enqueue(f);
    }

    // ── Factory ───────────────────────────────────────────────
    private Fruit CreateFruit(FruitType type)
    {
        var data = FruitDatabase.Get(type);
        var go   = new GameObject($"Fruit_{data.fruitName}");
        go.transform.SetParent(transform);

        // Add all components while GO is active so Awake runs immediately
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale           = GameConfig.GravityScale;
        rb.linearDamping          = GameConfig.LinearDamp;
        rb.angularDamping         = GameConfig.AngularDamp;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation          = RigidbodyInterpolation2D.Interpolate;

        // Sprite renderer (must be set BEFORE PolygonCollider2D so it can read the sprite shape)
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprites[type];
        sr.sortingOrder = 11;

        // Collider: PolygonCollider2D for custom sprites, CircleCollider2D for procedural
        if (hasCustomSprite[type])
        {
            var poly = go.AddComponent<PolygonCollider2D>();
            poly.sharedMaterial = fruitMat;
        }
        else
        {
            var col = go.AddComponent<CircleCollider2D>();
            col.radius         = 1f;   // local → world = localScale.x × 1 = scaleFactor ✓
            col.sharedMaterial = fruitMat;
        }

        var fruit = go.AddComponent<Fruit>();
        fruit.Initialise(data, scaleFactors[type]);

        go.SetActive(false);
        return fruit;
    }
}