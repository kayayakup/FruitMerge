// ============================================================
//  BubbleBackground.cs  —  animated colorful bubble effects
// ============================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BubbleBackground : MonoBehaviour
{
    private readonly List<GameObject> bubbles = new();
    private Sprite bubbleSprite;

    private static readonly Color[] BubbleColors =
    {
        new Color(1.00f, 0.60f, 0.70f, 0.22f),  // pink
        new Color(0.55f, 0.85f, 0.95f, 0.22f),  // turquoise
        new Color(1.00f, 0.90f, 0.50f, 0.22f),  // yellow
        new Color(0.72f, 0.62f, 0.95f, 0.22f),  // lavender
        new Color(0.60f, 0.95f, 0.72f, 0.22f),  // mint
        new Color(1.00f, 0.72f, 0.50f, 0.22f),  // peach
    };

    private const int   MaxBubbles    = 18;
    private const float SpawnInterval = 0.6f;
    private const float MinSize       = 0.4f;
    private const float MaxSize       = 1.8f;
    private const float MinSpeed      = 0.6f;
    private const float MaxSpeed      = 1.8f;
    private const float MinLife       = 5f;
    private const float MaxLife       = 12f;

    public void Initialize()
    {
        bubbleSprite = SpriteFactory.Circle(Color.white, Color.white, 64);
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (bubbles.Count < MaxBubbles)
                SpawnBubble();
            yield return new WaitForSeconds(SpawnInterval + Random.Range(0f, 0.4f));
        }
    }

    private void SpawnBubble()
    {
        float hw = GameConfig.HalfWidth + 3f;
        float x  = Random.Range(-hw, hw);
        float y  = GameConfig.BottomCenterY - 2f;

        var go = new GameObject("Bubble");
        go.transform.position = new Vector3(x, y, 1.8f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = bubbleSprite;
        sr.sortingOrder = -1;
        sr.color        = BubbleColors[Random.Range(0, BubbleColors.Length)];

        float size  = Random.Range(MinSize, MaxSize);
        float speed = Random.Range(MinSpeed, MaxSpeed);
        float life  = Random.Range(MinLife, MaxLife);
        float drift = Random.Range(-0.3f, 0.3f);
        float phase = Random.Range(0f, Mathf.PI * 2f);

        go.transform.localScale = Vector3.one * size;
        bubbles.Add(go);

        StartCoroutine(AnimateBubble(go, sr, size, speed, drift, phase, life));
    }

    private IEnumerator AnimateBubble(GameObject go, SpriteRenderer sr,
        float size, float speed, float drift, float phase, float life)
    {
        Color baseCol = sr.color;
        float elapsed = 0f;

        while (elapsed < life)
        {
            if (go == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / life;

            // Rise upward with gentle sine wave drift
            float moveY = speed * Time.deltaTime;
            float moveX = Mathf.Sin(elapsed * 1.2f + phase) * drift * Time.deltaTime;
            go.transform.position += new Vector3(moveX, moveY, 0f);

            // Pulse scale gently
            float pulse = 1f + Mathf.Sin(elapsed * 2f + phase) * 0.08f;
            go.transform.localScale = Vector3.one * size * pulse;

            // Fade out in last 30%
            float alpha = t > 0.7f ? baseCol.a * (1f - (t - 0.7f) / 0.3f) : baseCol.a;
            sr.color = new Color(baseCol.r, baseCol.g, baseCol.b, alpha);

            yield return null;
        }

        if (go != null)
        {
            bubbles.Remove(go);
            Destroy(go);
        }
    }
}
