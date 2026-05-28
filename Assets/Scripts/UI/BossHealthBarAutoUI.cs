using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Otomatik kurulumlu boss can barı.
///
/// Kullanım:
/// 1) Canvas altına bir RectTransform ekle, bu script'i takı.
/// 2) Inspector'da 8 frame sprite slotunu doldur (TL, T, TR, L, R, BL, B, BR).
/// 3) Boss referansını ata.
/// 4) Inspector'da "Rebuild" sağ tık → bar oluşur.
///
/// Frame parçaları otomatik konumlanır, fill içeride yer alır,
/// DOTween ile fade/shake/fill animasyonları çalışır.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class BossHealthBarAutoUI : MonoBehaviour
{
    // ─── Boss Bağlantısı ──────────────────────────────────────
    [Header("Boss Bağlantısı")]
    [SerializeField] private BossController boss;
    [SerializeField] private BossHealth bossHealth;

    // ─── Ekran Pozisyonu ──────────────────────────────────────
    public enum ScreenAnchor
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }

    [Header("Ekran Pozisyonu")]
    [SerializeField] private ScreenAnchor screenAnchor = ScreenAnchor.BottomCenter;
    [SerializeField] private Vector2 screenMargin = new Vector2(0f, 16f); // Kenardan boşluk (Scale Factor'a göre çarpılır)
    [SerializeField] private float barWidth  = 128f;
    [SerializeField] private float barHeight = 24f;

    // ─── Frame Sprite'ları ────────────────────────────────────
    [Header("Frame Sprite'ları (Multiple slice edilmiş tile'lar)")]
    [SerializeField] private Sprite topLeft;
    [SerializeField] private Sprite top;
    [SerializeField] private Sprite topRight;
    [SerializeField] private Sprite left;
    [SerializeField] private Sprite right;
    [SerializeField] private Sprite bottomLeft;
    [SerializeField] private Sprite bottom;
    [SerializeField] private Sprite bottomRight;

    // ─── Frame Ayarları ───────────────────────────────────────
    [Header("Frame Boyutları")]
    [SerializeField] private float tileSize = 16f;     // Köşe ve kenar tile'ının ekrandaki boyutu (px)
    [SerializeField] private float pixelsPerUnit = 1f; // Image PPU çarpanı (pixel art için genelde 1)

    // ─── Fill Ayarları ────────────────────────────────────────
    [Header("Fill (İç Dolum)")]
    [SerializeField] private Color fillColor = new Color(0.85f, 0.15f, 0.15f, 1f); // Kırmızı
    [SerializeField] private float fillPadding = 4f;   // Fill ile frame iç kenarı arasındaki boşluk
    [SerializeField] private Sprite fillSprite;        // Opsiyonel — boş bırakırsan düz renk

    // ─── İsim ─────────────────────────────────────────────────
    public enum NamePosition { Above, InsideCenter, Below }

    [Header("Boss Adı (opsiyonel)")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private string bossDisplayName = "Cerberus";
    [SerializeField] private NamePosition namePosition = NamePosition.InsideCenter;
    [SerializeField] private float nameOffsetY = 8f;

    // ─── Animasyon Süreleri ───────────────────────────────────
    [Header("Animasyon")]
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeOutDelay = 1.5f;
    [SerializeField] private float fillTweenDuration = 0.35f;
    [SerializeField] private Ease fillEase = Ease.OutCubic;

    // ─── Hasar Efekti ─────────────────────────────────────────
    [Header("Hasar Efekti")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 14;
    [SerializeField] private bool flashOnHit = true;
    [SerializeField] private Color flashColor = new Color(1f, 0.4f, 0.4f, 0.5f);

    // ─── Runtime referansları ─────────────────────────────────
    private CanvasGroup _canvasGroup;
    private RectTransform _rect;
    private RectTransform _fillRect;
    private Image _fillImage;
    private Image _flashImage;
    private float _maxFillWidth;

    private Tween _fillTween;
    private Tween _flashTween;
    private Sequence _shakeSeq;

    private const string FRAME_CHILD_NAME = "__FrameAuto";
    private const string FILL_CHILD_NAME  = "__FillAuto";
    private const string FLASH_CHILD_NAME = "__FlashAuto";

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (boss != null && bossHealth == null)
            bossHealth = boss.GetComponent<BossHealth>();

        if (Application.isPlaying)
        {
            Rebuild();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) return;
        if (boss != null) boss.onActivated.AddListener(HandleActivated);
        if (bossHealth != null)
        {
            bossHealth.onHealthChanged.AddListener(HandleHealthChanged);
            bossHealth.onBossDied.AddListener(HandleBossDied);
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        if (boss != null) boss.onActivated.RemoveListener(HandleActivated);
        if (bossHealth != null)
        {
            bossHealth.onHealthChanged.RemoveListener(HandleHealthChanged);
            bossHealth.onBossDied.RemoveListener(HandleBossDied);
        }
        _fillTween?.Kill();
        _flashTween?.Kill();
        _shakeSeq?.Kill();
    }

    // ─── Editor'de görsel önizleme için ────────────────────────
#if UNITY_EDITOR
    private bool _pendingRebuild;

    private void OnValidate()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_pendingRebuild) return;
        _pendingRebuild = true;
        UnityEditor.EditorApplication.delayCall += OnDelayedRebuild;
    }

    private void OnDelayedRebuild()
    {
        UnityEditor.EditorApplication.delayCall -= OnDelayedRebuild;
        _pendingRebuild = false;
        if (this == null) return;
        Rebuild();
    }

    private void OnDestroy_Editor()
    {
        UnityEditor.EditorApplication.delayCall -= OnDelayedRebuild;
    }
#endif

    // ─── Frame'i ve fill'i oluştur ────────────────────────────
    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        if (_rect == null) _rect = GetComponent<RectTransform>();

        // Layout (boyut + ekran pozisyonu) uygula
        ApplyLayout();

        // Eski auto child'ları temizle
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c.name == FRAME_CHILD_NAME || c.name == FILL_CHILD_NAME || c.name == FLASH_CHILD_NAME)
            {
                if (Application.isPlaying) Destroy(c.gameObject);
                else DestroyImmediate(c.gameObject);
            }
        }

        // FillContainer (frame'in altında, gerideki katman)
        RectTransform fillContainer = CreateChild(FILL_CHILD_NAME, _rect, 0);
        SetStretchAll(fillContainer, fillPadding, fillPadding, fillPadding, fillPadding);

        // Fill Image (sol-stretch, width animate edilir)
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(fillContainer, false);
        _fillRect = fillGO.GetComponent<RectTransform>();
        _fillRect.anchorMin = new Vector2(0f, 0f);
        _fillRect.anchorMax = new Vector2(0f, 1f);
        _fillRect.pivot     = new Vector2(0f, 0.5f);
        _fillRect.anchoredPosition = Vector2.zero;
        _fillRect.sizeDelta = new Vector2(fillContainer.rect.width, 0f);

        _fillImage = fillGO.GetComponent<Image>();
        _fillImage.color = fillColor;
        _fillImage.sprite = fillSprite;
        _fillImage.type = fillSprite != null ? Image.Type.Tiled : Image.Type.Simple;
        _fillImage.pixelsPerUnitMultiplier = pixelsPerUnit;
        _fillImage.raycastTarget = false;

        _maxFillWidth = fillContainer.rect.width;

        // Frame parçaları
        RectTransform frameRoot = CreateChild(FRAME_CHILD_NAME, _rect, 1);
        SetStretchAll(frameRoot, 0, 0, 0, 0);

        // Köşeler — sabit boyutlu, parent'ın köşesine pin'lenir
        AddCorner(frameRoot, "TL", topLeft,    new Vector2(0, 1));
        AddCorner(frameRoot, "TR", topRight,   new Vector2(1, 1));
        AddCorner(frameRoot, "BL", bottomLeft, new Vector2(0, 0));
        AddCorner(frameRoot, "BR", bottomRight,new Vector2(1, 0));

        // Kenarlar — bir eksende stretch, tiled
        AddEdge(frameRoot, "Top",    top,    EdgeSide.Top);
        AddEdge(frameRoot, "Bottom", bottom, EdgeSide.Bottom);
        AddEdge(frameRoot, "Left",   left,   EdgeSide.Left);
        AddEdge(frameRoot, "Right",  right,  EdgeSide.Right);

        // Flash overlay (frame'in üstünde)
        if (flashOnHit)
        {
            GameObject flashGO = new GameObject(FLASH_CHILD_NAME, typeof(RectTransform), typeof(Image));
            flashGO.transform.SetParent(_rect, false);
            RectTransform fr = flashGO.GetComponent<RectTransform>();
            SetStretchAll(fr, fillPadding, fillPadding, fillPadding, fillPadding);
            _flashImage = flashGO.GetComponent<Image>();
            _flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            _flashImage.raycastTarget = false;
        }

        // Boss adı (opsiyonel — yoksa atla)
        if (nameText != null)
        {
            nameText.text = bossDisplayName;
            RectTransform nt = nameText.rectTransform;

            switch (namePosition)
            {
                case NamePosition.Above:
                    // Bar'ın üstünde
                    nt.anchorMin = new Vector2(0.5f, 1f);
                    nt.anchorMax = new Vector2(0.5f, 1f);
                    nt.pivot     = new Vector2(0.5f, 0f);
                    nt.anchoredPosition = new Vector2(0f, nameOffsetY);
                    break;

                case NamePosition.InsideCenter:
                    // Bar'ın tam ortasında — taşma yok
                    nt.anchorMin = new Vector2(0.5f, 0.5f);
                    nt.anchorMax = new Vector2(0.5f, 0.5f);
                    nt.pivot     = new Vector2(0.5f, 0.5f);
                    nt.anchoredPosition = Vector2.zero;
                    break;

                case NamePosition.Below:
                    // Bar'ın altında
                    nt.anchorMin = new Vector2(0.5f, 0f);
                    nt.anchorMax = new Vector2(0.5f, 0f);
                    nt.pivot     = new Vector2(0.5f, 1f);
                    nt.anchoredPosition = new Vector2(0f, -nameOffsetY);
                    break;
            }

            // Text frame'in üstünde render edilsin (her zaman görünür)
            nameText.transform.SetAsLastSibling();
            nameText.raycastTarget = false;
        }
    }

    // ─── Layout (ekran pozisyonu + boyut) ─────────────────────
    [ContextMenu("Apply Layout")]
    public void ApplyLayout()
    {
        if (_rect == null) _rect = GetComponent<RectTransform>();

        Vector2 anchor;
        Vector2 pivot;
        Vector2 pos;

        switch (screenAnchor)
        {
            case ScreenAnchor.TopLeft:
                anchor = new Vector2(0f, 1f);  pivot = new Vector2(0f, 1f);
                pos = new Vector2(screenMargin.x, -screenMargin.y);
                break;
            case ScreenAnchor.TopCenter:
                anchor = new Vector2(0.5f, 1f); pivot = new Vector2(0.5f, 1f);
                pos = new Vector2(0f, -screenMargin.y);
                break;
            case ScreenAnchor.TopRight:
                anchor = new Vector2(1f, 1f);  pivot = new Vector2(1f, 1f);
                pos = new Vector2(-screenMargin.x, -screenMargin.y);
                break;
            case ScreenAnchor.MiddleLeft:
                anchor = new Vector2(0f, 0.5f); pivot = new Vector2(0f, 0.5f);
                pos = new Vector2(screenMargin.x, 0f);
                break;
            case ScreenAnchor.MiddleCenter:
                anchor = new Vector2(0.5f, 0.5f); pivot = new Vector2(0.5f, 0.5f);
                pos = Vector2.zero;
                break;
            case ScreenAnchor.MiddleRight:
                anchor = new Vector2(1f, 0.5f); pivot = new Vector2(1f, 0.5f);
                pos = new Vector2(-screenMargin.x, 0f);
                break;
            case ScreenAnchor.BottomLeft:
                anchor = new Vector2(0f, 0f);  pivot = new Vector2(0f, 0f);
                pos = new Vector2(screenMargin.x, screenMargin.y);
                break;
            case ScreenAnchor.BottomCenter:
                anchor = new Vector2(0.5f, 0f); pivot = new Vector2(0.5f, 0f);
                pos = new Vector2(0f, screenMargin.y);
                break;
            case ScreenAnchor.BottomRight:
                anchor = new Vector2(1f, 0f); pivot = new Vector2(1f, 0f);
                pos = new Vector2(-screenMargin.x, screenMargin.y);
                break;
            default:
                anchor = new Vector2(0.5f, 0.5f); pivot = new Vector2(0.5f, 0.5f); pos = Vector2.zero;
                break;
        }

        _rect.anchorMin = anchor;
        _rect.anchorMax = anchor;
        _rect.pivot = pivot;
        _rect.anchoredPosition = pos;
        _rect.sizeDelta = new Vector2(barWidth, barHeight);
    }

    // ─── Frame builder helpers ────────────────────────────────

    private enum EdgeSide { Top, Bottom, Left, Right }

    private RectTransform CreateChild(string n, RectTransform parent, int siblingIdx)
    {
        GameObject go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        if (siblingIdx >= 0) go.transform.SetSiblingIndex(siblingIdx);
        return go.GetComponent<RectTransform>();
    }

    private void SetStretchAll(RectTransform r, float left, float right, float top, float bottom)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.pivot     = new Vector2(0.5f, 0.5f);
        r.offsetMin = new Vector2(left, bottom);
        r.offsetMax = new Vector2(-right, -top);
    }

    private void AddCorner(RectTransform parent, string n, Sprite spr, Vector2 anchor)
    {
        GameObject go = new GameObject(n, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(tileSize, tileSize);

        Image img = go.GetComponent<Image>();
        img.sprite = spr;
        img.type = Image.Type.Simple;
        img.pixelsPerUnitMultiplier = pixelsPerUnit;
        img.raycastTarget = false;
        img.enabled = spr != null;
    }

    private void AddEdge(RectTransform parent, string n, Sprite spr, EdgeSide side)
    {
        GameObject go = new GameObject(n, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();

        switch (side)
        {
            case EdgeSide.Top:
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot     = new Vector2(0.5f, 1f);
                rt.offsetMin = new Vector2(tileSize, -tileSize);
                rt.offsetMax = new Vector2(-tileSize, 0f);
                break;
            case EdgeSide.Bottom:
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot     = new Vector2(0.5f, 0f);
                rt.offsetMin = new Vector2(tileSize, 0f);
                rt.offsetMax = new Vector2(-tileSize, tileSize);
                break;
            case EdgeSide.Left:
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot     = new Vector2(0f, 0.5f);
                rt.offsetMin = new Vector2(0f, tileSize);
                rt.offsetMax = new Vector2(tileSize, -tileSize);
                break;
            case EdgeSide.Right:
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot     = new Vector2(1f, 0.5f);
                rt.offsetMin = new Vector2(-tileSize, tileSize);
                rt.offsetMax = new Vector2(0f, -tileSize);
                break;
        }

        Image img = go.GetComponent<Image>();
        img.sprite = spr;
        img.type = Image.Type.Tiled;
        img.pixelsPerUnitMultiplier = pixelsPerUnit;
        img.raycastTarget = false;
        img.enabled = spr != null;
    }

    // ─── Event Handler'lar ─────────────────────────────────────

    private void HandleActivated()
    {
        if (bossHealth != null) SetFillImmediate(bossHealth.CurrentHealth, bossHealth.MaxHealth);

        _canvasGroup.DOKill();
        _canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);

        SoundManager.Instance?.TryPlayOneShot("BossUIAppear");
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (_fillRect == null) return;

        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        float targetWidth = _maxFillWidth * ratio;

        _fillTween?.Kill();
        _fillTween = _fillRect.DOSizeDelta(new Vector2(targetWidth, _fillRect.sizeDelta.y), fillTweenDuration)
                              .SetEase(fillEase);

        if (_canvasGroup.alpha > 0.1f)
        {
            PlayShake();
            if (flashOnHit && _flashImage != null) PlayFlash();
        }
    }

    private void HandleBossDied()
    {
        _fillTween?.Kill();
        _fillTween = _fillRect.DOSizeDelta(new Vector2(0f, _fillRect.sizeDelta.y), fillTweenDuration)
                              .SetEase(fillEase);

        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, fadeOutDuration).SetDelay(fadeOutDelay).SetEase(Ease.InQuad);

        // Fade-out tamamlanırken disappear sesi çal
        DOVirtual.DelayedCall(fadeOutDelay, () => SoundManager.Instance?.TryPlayOneShot("BossUIDisappear"));
    }

    private void SetFillImmediate(int current, int max)
    {
        if (_fillRect == null) return;
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        _fillRect.sizeDelta = new Vector2(_maxFillWidth * ratio, _fillRect.sizeDelta.y);
    }

    private void PlayShake()
    {
        _shakeSeq?.Kill();
        Vector3 originalPos = _rect.anchoredPosition3D;
        _shakeSeq = DOTween.Sequence();
        _shakeSeq.Append(_rect.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato, 90, false, true));
        _shakeSeq.OnComplete(() => _rect.anchoredPosition3D = originalPos);
    }

    private void PlayFlash()
    {
        _flashTween?.Kill();
        _flashImage.color = flashColor;
        _flashTween = _flashImage.DOFade(0f, 0.35f).SetEase(Ease.OutQuad);
    }
}
