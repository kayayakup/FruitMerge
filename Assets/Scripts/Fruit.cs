// ============================================================
//  Fruit.cs  —  individual fruit behaviour, physics, animation
// ============================================================
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Fruit : MonoBehaviour
{
    // ── Read-only state ───────────────────────────────────────
    public FruitData Data          { get; private set; }
    public FruitType FruitType     => Data.type;
    public bool      IsHeld        { get; private set; }
    public bool      IsNewlySpawned{ get; private set; }
    public bool      IsBeingMerged { get; private set; }
    public Bounds    WorldBounds   => col.bounds;

    // ── Components ────────────────────────────────────────────
    private Rigidbody2D    rb;
    private CircleCollider2D col;
    private SpriteRenderer sr;
    private MergeManager   mergeMgr;

    // ── Scale factor (set by FruitPool based on sprite size) ──
    private float _scaleFactor = 1.5f;

    // ── Private ───────────────────────────────────────────────
    private float spawnTime;
    private float bobOffset;

    // ─────────────────────────────────────────────────────────
    private void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sr  = GetComponent<SpriteRenderer>();
    }

    public void Initialise(FruitData data, float scaleFactor)
    {
        Data = data;
        _scaleFactor = scaleFactor;
        transform.localScale = Vector3.one * _scaleFactor;
    }

    public void ChangeType(FruitData data, float scaleFactor, Sprite sprite, float colliderRadius)
    {
        float currentScale = _scaleFactor > 0f
            ? transform.localScale.x / _scaleFactor
            : 1f;

        Data = data;
        _scaleFactor = scaleFactor;
        sr.sprite = sprite;
        col.radius = colliderRadius;
        transform.localScale = Vector3.one * (_scaleFactor * currentScale);
    }

    public void SetMergeManager(MergeManager mm) => mergeMgr = mm;

    // ── Pool: spawn & return ──────────────────────────────────
    public void Spawn(Vector3 pos, bool held)
    {
        gameObject.SetActive(true);
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        IsBeingMerged      = false;
        bobOffset          = Random.Range(0f, Mathf.PI * 2f);

        ApplyHeldState(held);

        // Scale-in animation
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleTo(GameConfig.SpawnAnim));

        if (!held) GameManager.Instance?.RegisterFruit(this);
    }

    public void Release()
    {
        if (!IsHeld) return;
        ApplyHeldState(false);

        AudioManager.Instance?.PlayDrop();

        GameManager.Instance?.RegisterFruit(this);
    }

    public void MarkMerging()
    {
        IsBeingMerged = true;
        StopAllCoroutines();
    }

    public void ReturnToPool()
    {
        StopAllCoroutines();
        IsBeingMerged = IsNewlySpawned = IsHeld = false;
        GameManager.Instance?.UnregisterFruit(this);
        gameObject.SetActive(false);
    }

    // ── Per-frame ─────────────────────────────────────────────
    private void Update()
    {
        // Expire merge immunity
        if (IsNewlySpawned && Time.time - spawnTime > GameConfig.MergeImmunity)
            IsNewlySpawned = false;

        // Gentle held bob
        if (IsHeld)
        {
            float bob = Mathf.Sin((Time.time + bobOffset) * GameConfig.BobFreq)
                      * GameConfig.BobAmplitude;
            transform.localScale = Vector3.one * (_scaleFactor + bob);
        }
    }

    // ── Collision → attempt merge ─────────────────────────────
    private void OnCollisionEnter2D(Collision2D hit)
    {
        TryMergeWith(hit);

        // Play collision sound if impact is strong enough
        if (!IsHeld && !IsBeingMerged)
        {
            float impact = hit.relativeVelocity.magnitude;
            if (impact > 1.5f) // Threshold to prevent tiny bumps from making sound
            {
                // Scale volume based on impact, max out at 1.0f
                float vol = Mathf.Clamp01(impact / 10f); 
                AudioManager.Instance?.PlayCollide(vol);
            }
        }
    }
    
    private void OnCollisionStay2D(Collision2D hit)  => TryMergeWith(hit);

    private void TryMergeWith(Collision2D hit)
    {
        if (IsBeingMerged || IsHeld || IsNewlySpawned || mergeMgr == null) return;
        var other = hit.gameObject.GetComponent<Fruit>();
        if (other == null || other.IsBeingMerged || other.IsHeld || other.IsNewlySpawned) return;
        if (other.FruitType != FruitType) return;
        mergeMgr.TryMerge(this, other);
    }

    // ── Public coroutines (used by MergeManager) ─────────────
    public IEnumerator ScaleOut(float duration)
    {
        Vector3 start = transform.localScale;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float p = t / duration;
            transform.localScale = Vector3.Lerp(start, Vector3.zero, p * p);
            yield return null;
        }
        transform.localScale = Vector3.zero;
    }

    // ── Private helpers ───────────────────────────────────────
    private void ApplyHeldState(bool held)
    {
        IsHeld             = held;
        rb.isKinematic     = held;
        rb.gravityScale    = held ? 0f : GameConfig.GravityScale;
        rb.linearVelocity  = Vector2.zero;
        rb.angularVelocity = 0f;
        col.enabled        = !held;
        sr.color           = held ? new Color(1f,1f,1f, GameConfig.HeldAlpha) : Color.white;

        if (!held) { IsNewlySpawned = true; spawnTime = Time.time; }
        else IsNewlySpawned = false;
    }

    private IEnumerator ScaleTo(float duration)
    {
        Vector3 start = transform.localScale;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            float e = 1f - Mathf.Pow(1f - p, 3f);   // cubic ease-out
            transform.localScale = Vector3.Lerp(start, Vector3.one * _scaleFactor, e);
            yield return null;
        }
        transform.localScale = Vector3.one * _scaleFactor;
    }
}