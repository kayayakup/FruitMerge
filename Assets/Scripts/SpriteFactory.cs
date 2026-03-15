// ============================================================
//  SpriteFactory.cs  —  runtime texture / sprite generation
// ============================================================
using UnityEngine;

public static class SpriteFactory
{
    /// <summary>
    /// Circle sprite whose natural world-space RADIUS = 1 unit at localScale (1,1,1).
    /// Includes a specular highlight and radial gradient.
    /// </summary>
    public static Sprite Circle(Color centre, Color edge, int size = 128)
    {
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode   = TextureWrapMode.Clamp
        };

        float half = size * 0.5f;
        float r    = half;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) - half;
                float dy = (y + 0.5f) - half;
                float d  = Mathf.Sqrt(dx * dx + dy * dy);

                if (d >= r) { tex.SetPixel(x, y, Color.clear); continue; }

                // Radial gradient
                float t   = d / r;
                Color col = Color.Lerp(Brighten(centre, 0.25f), edge, t * t);

                // Small specular highlight top-left
                float dxH = (x + 0.5f) - half * 0.62f;
                float dyH = (y + 0.5f) - half * 1.38f;
                float dH  = Mathf.Sqrt(dxH * dxH + dyH * dyH);
                float sr2 = r * 0.28f;
                if (dH < sr2)
                    col = Color.Lerp(col, Color.white, 0.55f * (1f - dH / sr2));

                float alpha = d < r - 1.5f ? 1f : Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, new Color(col.r, col.g, col.b, alpha));
            }
        }
        tex.Apply();
        // PPU = size/2 → radius = 1 world unit
        return Sprite.Create(tex, new Rect(0, 0, size, size),
                             new Vector2(0.5f, 0.5f), size * 0.5f);
    }

    /// <summary>Solid 1×1 world-unit rectangle sprite.</summary>
    public static Sprite Rect(Color color)
    {
        var tex = new Texture2D(4, 4, TextureFormat.ARGB32, false)
            { filterMode = FilterMode.Point };
        Color[] px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = color;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    /// <summary>Vertical-gradient 1×1 world-unit sprite.</summary>
    public static Sprite GradientRect(Color bottom, Color top, int w = 4, int h = 64)
    {
        var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
        for (int y = 0; y < h; y++)
        {
            Color c = Color.Lerp(bottom, top, (float)y / (h - 1));
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, c);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h),
                             new Vector2(0.5f, 0.5f), Mathf.Min(w, h));
    }

    private static Color Brighten(Color c, float a) =>
        new Color(Mathf.Clamp01(c.r + a * (1f - c.r)),
                  Mathf.Clamp01(c.g + a * (1f - c.g)),
                  Mathf.Clamp01(c.b + a * (1f - c.b)), c.a);
}