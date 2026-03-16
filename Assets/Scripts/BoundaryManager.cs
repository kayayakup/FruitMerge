// ============================================================
//  BoundaryManager.cs  —  walls, floor, background, danger line
//  3D-looking basket with gradient layers
// ============================================================
using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    private PhysicsMaterial2D wallMat;

    public void Initialize(Sprite bottomSprite = null)
    {
        wallMat = new PhysicsMaterial2D("WallMat")
            { bounciness = GameConfig.Bounciness, friction = GameConfig.Friction };

        CreateBackground();
        CreateBasket3D();
        // CreateGameOverLine(); // No longer used as a visual line

        if (bottomSprite != null)
            CreateBasketBottomAddon(bottomSprite);
    }

    // ── Background ───────────────────────────────────────────
    private void CreateBackground()
    {
        float totalWidth  = (GameConfig.HalfWidth + 3f) * 3f + 4f;
        float totalHeight = 40f;

        // Sky gradient – canlı mavi
MakeQuad("BG_Sky", new Vector3(0, 4f, 2f), new Vector2(totalWidth, totalHeight * 0.6f),
    SpriteFactory.GradientRect(
        new Color(0.42f, 0.78f, 1.00f), // #6BC5FF – parlak gök mavisi
        new Color(0.29f, 0.62f, 1.00f)  // #4A9EFF – daha koyu ama canlı mavi
    ));

// Ground gradient – taze yeşil (çimen)
MakeQuad("BG_Ground", new Vector3(0, -8f, 2f), new Vector2(totalWidth, totalHeight * 0.4f),
    SpriteFactory.GradientRect(
        new Color(0.55f, 0.76f, 0.29f), // #8BC34A – parlak çimen yeşili
        new Color(0.77f, 0.88f, 0.65f)  // #C5E1A5 – açık yeşil
    ));
    }

    // ── 3D Basket ────────────────────────────────────────────
    private void CreateBasket3D()
    {
        float hw = GameConfig.HalfWidth;
        float wt = GameConfig.WallThick;
        float bY = GameConfig.BottomCenterY;

        // Wall extends from bottom to 3 units above game-over line
        float wallTop  = GameConfig.GameOverLineY + 3f;
        float wallBot  = bY - wt * 0.5f;
        float WH       = wallTop - wallBot;
        float wallCY   = wallBot + WH * 0.5f;

        // ── 3D Colors ────────────────────────────────────────
        Color woodFront  = new Color(0.60f, 0.38f, 0.20f);
        Color woodDark   = new Color(0.38f, 0.22f, 0.10f);
        Color woodLight  = new Color(0.82f, 0.62f, 0.38f);
        Color woodShine  = new Color(0.92f, 0.78f, 0.58f);
        Color floorFront = new Color(0.55f, 0.35f, 0.18f);
        Color floorDark  = new Color(0.35f, 0.20f, 0.08f);
        Color floorLight = new Color(0.75f, 0.55f, 0.32f);

        float depth = wt * 0.35f; // 3D depth offset

        // ═══════════ LEFT WALL (3D) ═══════════════════════════
        // Inner shadow edge (dark, slightly inside)
        MakeVisual("LW_Inner", new Vector3(-(hw - depth * 0.5f), wallCY, 0.02f),
                   new Vector2(depth, WH), woodDark, 4);
        // Main front face
        MakeWall("LeftWall", new Vector3(-(hw + wt * 0.5f), wallCY, 0f),
                 new Vector2(wt, WH), woodFront, 5);
        // Outer highlight edge
        MakeVisual("LW_Outer", new Vector3(-(hw + wt + depth * 0.5f), wallCY, 0.01f),
                   new Vector2(depth, WH), woodLight, 6);
        // Top shine strip
        MakeVisual("LW_TopShine", new Vector3(-(hw + wt * 0.5f), wallTop - 0.06f, -0.01f),
                   new Vector2(wt + depth * 2f, 0.12f), woodShine, 7);

        // ═══════════ RIGHT WALL (3D) ══════════════════════════
        MakeVisual("RW_Inner", new Vector3(hw - depth * 0.5f, wallCY, 0.02f),
                   new Vector2(depth, WH), woodDark, 4);
        MakeWall("RightWall", new Vector3(hw + wt * 0.5f, wallCY, 0f),
                 new Vector2(wt, WH), woodFront, 5);
        MakeVisual("RW_Outer", new Vector3(hw + wt + depth * 0.5f, wallCY, 0.01f),
                   new Vector2(depth, WH), woodLight, 6);
        MakeVisual("RW_TopShine", new Vector3(hw + wt * 0.5f, wallTop - 0.06f, -0.01f),
                   new Vector2(wt + depth * 2f, 0.12f), woodShine, 7);

        // ═══════════ BOTTOM WALL (3D) ═════════════════════════
        float floorW = hw * 2 + wt * 2;
        // Top surface highlight
        MakeVisual("BW_Top", new Vector3(0, bY + wt * 0.5f + depth * 0.25f, 0.02f),
                   new Vector2(floorW, depth * 0.5f), floorLight, 6);
        // Main front face
        MakeWall("BottomWall", new Vector3(0, bY, 0),
                 new Vector2(floorW, wt), floorFront, 5);
        // Bottom dark edge
        MakeVisual("BW_Bottom", new Vector3(0, bY - wt * 0.5f - depth * 0.25f, 0.01f),
                   new Vector2(floorW + depth, depth * 0.5f), floorDark, 4);

        // ═══════════ INNER BACK PANEL ═════════════════════════
        // Slightly tinted background inside the basket for depth
        Color innerBg = new Color(0.96f, 0.92f, 0.86f, 0.25f);
        MakeVisual("InnerBG", new Vector3(0, wallBot + WH * 0.5f, 0.5f),
                   new Vector2(hw * 2f, WH), innerBg, 1);
    }

    // ── Physics wall ─────────────────────────────────────────
    private void MakeWall(string name, Vector3 pos, Vector2 size, Color col, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Rect(col); sr.sortingOrder = sortOrder;
        
        var bc = go.AddComponent<BoxCollider2D>();
        bc.sharedMaterial = wallMat;

        // Thicken colliders significantly to prevent tunnelling.
        // We extend the collider OUTWARDS so it doesn't affect the inner volume.
        float thickness = 10f; // 10 units of extra thickness
        if (name.Contains("Left"))
        {
            bc.size   = new Vector2(1f + thickness, 1f);
            bc.offset = new Vector2(-thickness * 0.5f, 0f);
        }
        else if (name.Contains("Right"))
        {
            bc.size   = new Vector2(1f + thickness, 1f);
            bc.offset = new Vector2(thickness * 0.5f, 0f);
        }
        else if (name.Contains("Bottom"))
        {
            bc.size   = new Vector2(1f, 1f + thickness);
            bc.offset = new Vector2(0f, -thickness * 0.5f);
        }
        else
        {
            bc.size = Vector2.one;
        }
    }

    // ── Visual-only layer ────────────────────────────────────
    private void MakeVisual(string name, Vector3 pos, Vector2 size, Color col, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Rect(col); sr.sortingOrder = sortOrder;
    }

    // ── Dashed game-over danger line ─────────────────────────
    private void CreateGameOverLine()
    {
        float y       = GameConfig.GameOverLineY;
        float hw      = GameConfig.HalfWidth;
        float dashLen = 0.35f, gapLen = 0.20f, unit = dashLen + gapLen;
        Color lineCol = new Color(1f, 0.22f, 0.22f, 0.75f);

        var parent = new GameObject("GameOverLine");
        parent.transform.position = new Vector3(0f, y, -0.5f);

        int n = Mathf.CeilToInt(hw * 2f / unit) + 2;
        for (int i = 0; i < n; i++)
        {
            float cx = -hw + i * unit + dashLen * 0.5f;
            if (cx - dashLen * 0.5f > hw) break;

            var dash = new GameObject($"D{i}");
            dash.transform.SetParent(parent.transform);
            dash.transform.localPosition = new Vector3(cx, 0f, 0f);
            dash.transform.localScale    = new Vector3(dashLen, 0.08f, 1f);
            var sr = dash.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Rect(lineCol); sr.sortingOrder = 8;
        }
    }

    // ── Basket Bottom Visual Addon ──────────────────────────
    private void CreateBasketBottomAddon(Sprite sp)
    {
        float hw = GameConfig.HalfWidth;
        float wt = GameConfig.WallThick;
        float bY = GameConfig.BottomCenterY;

        var go = new GameObject("BasketBottomAddon");
        // Place it slightly in front of the bottom wall (Z = -0.05f)
        go.transform.position = new Vector3(0, bY, -0.05f);
        
        // Scale it to cover the width of the basket (including walls)
        float totalW = (hw * 2f) + (wt * 2f);
        // We'll maintain the sprite's aspect ratio or just stretch it if "panoramic" implies full width
        go.transform.localScale = new Vector3(totalW, 1.5f, 1f); // 1.5f height for the "small panoramic" look

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = 10; // In front of everything else in the basket
    }

    // ── Helper ────────────────────────────────────────────────
    private void MakeQuad(string name, Vector3 pos, Vector2 size, Sprite sp)
    {
        var go = new GameObject(name);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.sortingOrder = -3;
    }
}