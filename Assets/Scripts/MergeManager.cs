// ============================================================
//  MergeManager.cs  —  merge detection, animation, particles
// ============================================================
using UnityEngine;
using System.Collections;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance { get; private set; }

    private FruitPool    pool;
    private ScoreManager scoreMgr;

    public void Initialize(FruitPool fp, ScoreManager sm)
    {
        Instance = this; pool = fp; scoreMgr = sm;
    }

    // ── Called from Fruit.OnCollisionEnter2D ─────────────────
    public void TryMerge(Fruit a, Fruit b)
    {
        // Guard: mark both immediately (synchronous) to prevent double-trigger
        if (a.IsBeingMerged || b.IsBeingMerged) return;
        a.MarkMerging();
        b.MarkMerging();
        StartCoroutine(MergeRoutine(a, b));
    }

    private IEnumerator MergeRoutine(Fruit a, Fruit b)
    {
        Vector3 pos = (a.transform.position + b.transform.position) * 0.5f;
        FruitData mergedData = a.Data;
        float   dur = GameConfig.MergeAnim;

        AudioManager.Instance?.PlayMerge();

        // Scale both out simultaneously
        StartCoroutine(a.ScaleOut(dur));
        yield return StartCoroutine(b.ScaleOut(dur));

        // Award score
        scoreMgr.AddScore(mergedData.scoreValue);

        // Visual burst
        SpawnBurst(pos, mergedData.primaryColor);

        // Return to pools
        pool.Return(a);
        pool.Return(b);

        // Spawn the next tier (if any)
        if (FruitDatabase.HasNext(mergedData.type))
        {
            var next  = pool.Get(FruitDatabase.Next(mergedData.type).type);
            next.Spawn(pos, false);
        }
    }

    // ── Particle burst ────────────────────────────────────────
    private void SpawnBurst(Vector3 centre, Color col)
    {
        for (int i = 0; i < 8; i++)
        {
            float ang = i * 45f * Mathf.Deg2Rad;
            var   vel = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang))
                      * Random.Range(1.0f, 2.4f);
            StartCoroutine(ParticleRoutine(centre, vel, col));
        }
    }

    private IEnumerator ParticleRoutine(Vector3 p, Vector2 vel, Color col)
    {
        var go = new GameObject("MergePart");
        go.transform.position   = p;
        float sz = Random.Range(0.06f, 0.13f);
        go.transform.localScale = Vector3.one * sz;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Circle(col, col * 0.65f, 32); sr.sortingOrder = 10;

        float life = Random.Range(0.28f, 0.50f);
        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float a = 1f - t / life;
            sr.color              = new Color(col.r, col.g, col.b, a);
            go.transform.position += (Vector3)(vel * Time.deltaTime);
            go.transform.localScale = Vector3.one * sz * a;
            vel   *= 1f - Time.deltaTime * 3f;
            vel.y -= 2f * Time.deltaTime;
            yield return null;
        }
        Destroy(go);
    }
}