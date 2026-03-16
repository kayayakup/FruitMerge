// ============================================================
//  UIManager.cs  —  premium HUD + game-over screen + Safe Area
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ── Dependencies ─────────────────────────────────────────
    private ScoreManager scoreMgr;
    private FruitSpawner spawner;

    // ── HUD ──────────────────────────────────────────────────
    private TMP_Text txtScore, txtBest;

    // ── Next preview ─────────────────────────────────────────
    private Image    nextImg;

    // ── Game-over panel ──────────────────────────────────────
    private GameObject   goPanel;
    private TMP_Text     goScore, goBest;
    private RawImage     goShot;

    // ── Canvas ────────────────────────────────────────────────
    private Canvas       canvas;
    private RectTransform safeAreaRect;

    // ─────────────────────────────────────────────────────────
    public void Initialize(ScoreManager sm, FruitSpawner sp)
    {
        Instance = this; scoreMgr = sm; spawner = sp;
        BuildCanvas();
        BuildSafeArea();
        BuildHUD();
        BuildNextPreview();
        BuildGameOverPanel();
        scoreMgr.OnScoreChanged += (cur, best) =>
        {
            txtScore.text = cur.ToString();
            txtBest .text = best.ToString();
        };
        // Initial paint
        txtScore.text = "0";
        txtBest .text = sm.BestScore.ToString();
    }

    // ── Public API ────────────────────────────────────────────
    public void RefreshNextPreview(int typeIdx)
    {
        if (nextImg == null) return;
        var pool = FruitPool.Instance;
        if (pool != null)
        {
            nextImg.sprite = pool.GetSprite((FruitType)typeIdx);
            nextImg.color  = Color.white;
        }
        else
        {
            var d = FruitDatabase.Get(typeIdx);
            nextImg.sprite = SpriteFactory.Circle(d.primaryColor, d.edgeColor);
            nextImg.color  = Color.white;
        }
    }

    public void ShowGameOver(Texture2D shot, int score, int best)
    {
        goShot.texture = shot;
        goScore.text   = score.ToString();
        goBest.text    = best.ToString();
        goPanel.SetActive(true);
    }

    public void HideGameOver() => goPanel?.SetActive(false);

    // ── Canvas ───────────────────────────────────────────────
    private void BuildCanvas()
    {
        var go = new GameObject("UICanvas"); go.transform.SetParent(transform);
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(540, 960);
        scaler.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
    }

    // ── Safe Area Container ──────────────────────────────────
    private void BuildSafeArea()
    {
        var saGO = MakeRectGO("SafeArea", canvas.transform);
        safeAreaRect = (RectTransform)saGO.transform;
        FullStretch(safeAreaRect);
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        var sa = Screen.safeArea;
        var canvasRect = (RectTransform)canvas.transform;
        Vector2 anchorMin = sa.position / new Vector2(Screen.width, Screen.height);
        Vector2 anchorMax = (sa.position + sa.size) / new Vector2(Screen.width, Screen.height);
        safeAreaRect.anchorMin = anchorMin;
        safeAreaRect.anchorMax = anchorMax;
        safeAreaRect.offsetMin = safeAreaRect.offsetMax = Vector2.zero;
    }

    // ── Premium HUD ──────────────────────────────────────────
    private void BuildHUD()
    {
        // ═══ SCORE CARD — top-right ════════════════════════════
        var scoreCard = MakeRectGO("ScoreCard", safeAreaRect);
        var scRt = (RectTransform)scoreCard.transform;
        scRt.anchorMin = scRt.anchorMax = new Vector2(1, 1);
        scRt.pivot     = new Vector2(1, 1);
        scRt.anchoredPosition = new Vector2(-12, -65);
        scRt.sizeDelta = new Vector2(130, 65);
        // Card background
        var scBg = scoreCard.AddComponent<Image>();
        scBg.color = new Color(0.18f, 0.18f, 0.28f, 0.85f);

        // "SCORE" label
        var scoreLbl = MakeLabel("ScoreLbl", Vector2.zero, Vector2.zero,
                                  TextAnchor.UpperCenter, 12, scoreCard.transform);
        scoreLbl.text  = "SCORE";
        scoreLbl.color = new Color(0.75f, 0.75f, 0.85f);
        scoreLbl.fontStyle = FontStyles.Bold;
        var slRt = (RectTransform)scoreLbl.transform;
        slRt.anchorMin = new Vector2(0, 0.55f); slRt.anchorMax = Vector2.one;
        slRt.offsetMin = new Vector2(8, 0); slRt.offsetMax = new Vector2(-8, -6);

        // Score value
        txtScore = MakeLabel("ScoreVal", Vector2.zero, Vector2.zero,
                              TextAnchor.MiddleCenter, 24, scoreCard.transform);
        txtScore.color = Color.white;
        txtScore.fontStyle = FontStyles.Bold;
        var svRt = (RectTransform)txtScore.transform;
        svRt.anchorMin = Vector2.zero; svRt.anchorMax = new Vector2(1, 0.55f);
        svRt.offsetMin = new Vector2(8, 4); svRt.offsetMax = new Vector2(-8, 0);
        txtScore.alignment = TextAlignmentOptions.Center;

        // ═══ BEST CARD — below score ═══════════════════════════
        var bestCard = MakeRectGO("BestCard", safeAreaRect);
        var bcRt = (RectTransform)bestCard.transform;
        bcRt.anchorMin = bcRt.anchorMax = new Vector2(1, 1);
        bcRt.pivot     = new Vector2(1, 1);
        bcRt.anchoredPosition = new Vector2(-12, -138);
        bcRt.sizeDelta = new Vector2(130, 65);
        var bcBg = bestCard.AddComponent<Image>();
        bcBg.color = new Color(0.28f, 0.18f, 0.35f, 0.85f);

        var bestLbl = MakeLabel("BestLbl", Vector2.zero, Vector2.zero,
                                 TextAnchor.UpperCenter, 12, bestCard.transform);
        bestLbl.text  = "BEST";
        bestLbl.color = new Color(1f, 0.85f, 0.55f);
        bestLbl.fontStyle = FontStyles.Bold;
        var blRt = (RectTransform)bestLbl.transform;
        blRt.anchorMin = new Vector2(0, 0.55f); blRt.anchorMax = Vector2.one;
        blRt.offsetMin = new Vector2(8, 0); blRt.offsetMax = new Vector2(-8, -6);

        txtBest = MakeLabel("BestVal", Vector2.zero, Vector2.zero,
                             TextAnchor.MiddleCenter, 24, bestCard.transform);
        txtBest.color = new Color(1f, 0.92f, 0.65f);
        txtBest.fontStyle = FontStyles.Bold;
        var bvRt = (RectTransform)txtBest.transform;
        bvRt.anchorMin = Vector2.zero; bvRt.anchorMax = new Vector2(1, 0.55f);
        bvRt.offsetMin = new Vector2(8, 4); bvRt.offsetMax = new Vector2(-8, 0);
        txtBest.alignment = TextAlignmentOptions.Center;
    }

    // ── Next Preview (top-left) ──────────────────────────────
    private void BuildNextPreview()
    {
        var container = MakeRectGO("NextContainer", safeAreaRect);
        var rt = (RectTransform)container.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot     = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(12, -65);
        rt.sizeDelta = new Vector2(80, 95);

        // Background card
        var bg = container.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.18f, 0.28f, 0.85f);

        // "NEXT" label
        var label = MakeLabel("NextTxt", Vector2.zero, Vector2.zero,
                              TextAnchor.UpperCenter, 12, container.transform);
        label.text  = "NEXT";
        label.color = new Color(0.75f, 0.75f, 0.85f);
        label.fontStyle = FontStyles.Bold;
        var lRt = (RectTransform)label.transform;
        lRt.anchorMin = new Vector2(0, 0.72f); lRt.anchorMax = Vector2.one;
        lRt.offsetMin = new Vector2(4, 0); lRt.offsetMax = new Vector2(-4, -6);

        // Fruit image
        var imgGO = MakeRectGO("NextImg", container.transform);
        nextImg   = imgGO.AddComponent<Image>();
        nextImg.color = Color.white;
        nextImg.preserveAspect = true;
        var ri = (RectTransform)imgGO.transform;
        ri.anchorMin = new Vector2(0.1f, 0.05f);
        ri.anchorMax = new Vector2(0.9f, 0.70f);
        ri.offsetMin = ri.offsetMax = Vector2.zero;
    }

    // ── Premium Game-over Panel ──────────────────────────────
    private void BuildGameOverPanel()
    {
        goPanel = MakeRectGO("GameOverPanel", canvas.transform);
        FullStretch((RectTransform)goPanel.transform);

        var cg = goPanel.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // ── Dimmed backdrop ──
        var dim = goPanel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.70f);

        // ── Main card ──
        var card = MakeRectGO("Card", goPanel.transform);
        var cardRt = (RectTransform)card.transform;
        cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(420, 600);
        var cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.14f, 0.12f, 0.22f, 0.95f);

        // ── "GAME OVER" title (artık biraz daha aşağıda) ──
        var title = MakeLabel("GOTitle", Vector2.zero, Vector2.zero,
                              TextAnchor.MiddleCenter, 42, card.transform);
        title.text = "GAME OVER";
        title.color = new Color(1f, 0.35f, 0.30f);
        title.fontStyle = FontStyles.Bold;
        title.enableWordWrapping = false;
        var tRt = (RectTransform)title.transform;
        tRt.anchorMin = new Vector2(0, 0.80f); tRt.anchorMax = new Vector2(1, 0.90f); // yukarıdan biraz boşluk bırak
        tRt.offsetMin = tRt.offsetMax = Vector2.zero;
        title.alignment = TextAlignmentOptions.Center;

        // ── Screenshot (preserve aspect) ──
        var shotGO = MakeRectGO("Shot", card.transform);
        goShot = shotGO.AddComponent<RawImage>();
        var sRt = (RectTransform)shotGO.transform;
        sRt.anchorMin = new Vector2(0.05f, 0.32f);
        sRt.anchorMax = new Vector2(0.95f, 0.86f);
        sRt.offsetMin = sRt.offsetMax = Vector2.zero;
        var arf = shotGO.AddComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        arf.aspectRatio = 9f / 16f;

        // ── YENİ: Score değeri (sol üst köşe) ──
        var scoreCont = MakeRectGO("ScoreContainer", card.transform);
        var scRt = scoreCont.GetComponent<RectTransform>();
        scRt.anchorMin = new Vector2(0, 1);          // sol üst
        scRt.anchorMax = new Vector2(0, 1);
        scRt.pivot = new Vector2(0, 1);
        scRt.anchoredPosition = new Vector2(100, -20);
        scRt.sizeDelta = new Vector2(150, 80);

        // Dikey layout – etiket üstte, değer altta
        var vlg = scoreCont.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.spacing = 5;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var scoreLbl = MakeLabel("ScoreLabel", Vector2.zero, Vector2.zero,
                                 TextAnchor.MiddleLeft, 18, scoreCont.transform);
        scoreLbl.text = "SCORE";
        scoreLbl.color = Color.white;
        scoreLbl.fontStyle = FontStyles.Bold;
        var slRt = scoreLbl.GetComponent<RectTransform>();
        slRt.anchorMin = new Vector2(0, 1); slRt.anchorMax = new Vector2(1, 1);
        slRt.pivot = new Vector2(0, 1);
        slRt.sizeDelta = new Vector2(0, 30);   // sabit yükseklik

        goScore = MakeLabel("GOScore", Vector2.zero, Vector2.zero,
                            TextAnchor.MiddleLeft, 32, scoreCont.transform);
        goScore.color = Color.white;
        goScore.fontStyle = FontStyles.Bold;
        var gsRt = goScore.GetComponent<RectTransform>();
        gsRt.anchorMin = new Vector2(0, 0); gsRt.anchorMax = new Vector2(1, 0);
        gsRt.pivot = new Vector2(0, 0);
        gsRt.sizeDelta = new Vector2(0, 40);   // sabit yükseklik

        // ── YENİ: Best değeri (sağ üst köşe) ──
        var bestCont = MakeRectGO("BestContainer", card.transform);
        var bcRt = bestCont.GetComponent<RectTransform>();
        bcRt.anchorMin = new Vector2(1, 1);          // sağ üst
        bcRt.anchorMax = new Vector2(1, 1);
        bcRt.pivot = new Vector2(1, 1);
        bcRt.anchoredPosition = new Vector2(-25, -20);
        bcRt.sizeDelta = new Vector2(150, 80);

        var vlgBest = bestCont.AddComponent<VerticalLayoutGroup>();
        vlgBest.childAlignment = TextAnchor.UpperRight;
        vlgBest.spacing = 5;
        vlgBest.childForceExpandWidth = true;
        vlgBest.childForceExpandHeight = false;

        var bestLbl = MakeLabel("BestLabel", Vector2.zero, Vector2.zero,
                                TextAnchor.MiddleRight, 18, bestCont.transform);
        bestLbl.text = "BEST";
        bestLbl.color = Color.white;
        bestLbl.fontStyle = FontStyles.Bold;
        var blRt = bestLbl.GetComponent<RectTransform>();
        blRt.anchorMin = new Vector2(0, 1); blRt.anchorMax = new Vector2(1, 1);
        blRt.pivot = new Vector2(1, 1);
        blRt.sizeDelta = new Vector2(0, 30);

        goBest = MakeLabel("GOBest", Vector2.zero, Vector2.zero,
                           TextAnchor.MiddleRight, 32, bestCont.transform);
        goBest.color = Color.white;
        goBest.fontStyle = FontStyles.Bold;
        var gbRt = goBest.GetComponent<RectTransform>();
        gbRt.anchorMin = new Vector2(0, 0); gbRt.anchorMax = new Vector2(1, 0);
        gbRt.pivot = new Vector2(1, 0);
        gbRt.sizeDelta = new Vector2(0, 40);

        // ── Play Again button (aynı kalır) ──
        var btnGO = MakeRectGO("RestartBtn", card.transform);
        var btnRt = (RectTransform)btnGO.transform;
        btnRt.anchorMin = new Vector2(0.12f, 0.04f); btnRt.anchorMax = new Vector2(0.88f, 0.14f);
        btnRt.offsetMin = btnRt.offsetMax = Vector2.zero;
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.95f, 0.35f, 0.20f);
        btnImg.raycastTarget = true;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(OnPlayAgainClicked);

        var btnLabel = MakeLabel("RestartTxt", Vector2.zero, Vector2.zero,
                                 TextAnchor.MiddleCenter, 24, btnGO.transform);
        btnLabel.text  = "▶  PLAY AGAIN";
        btnLabel.color = Color.white;
        btnLabel.fontStyle = FontStyles.Bold;
        btnLabel.raycastTarget = false;
        FullStretch((RectTransform)btnLabel.transform);
        btnLabel.alignment = TextAlignmentOptions.Center;

        goPanel.SetActive(false);
    }

    private void OnPlayAgainClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        // Scene reload — guaranteed clean state
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ── UI Helpers ───────────────────────────────────────────
    private TMP_Text MakeLabel(string name, Vector2 pos, Vector2 size,
                               TextAnchor align, int fontSize, Transform parent)
    {
        var go = MakeRectGO(name, parent);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = fontSize;
        t.text     = name;
        t.color    = Color.white;
        return t;
    }

    private GameObject MakeRectGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void FullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}