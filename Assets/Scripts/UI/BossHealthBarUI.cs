using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Boss can barı — bir BossHealth'i izler, aktive olunca fade-in eder,
/// hasar alınca fill tween + shake yapar, öldüğünde gecikmeli fade-out olur.
///
/// Hierarchy beklentisi:
///   Canvas
///   └── BossHealthBar (CanvasGroup + bu script)
///       ├── Frame  (Image - çerçeve)
///       ├── FillContainer (RectTransform - dolum alanının boyutu)
///       │   └── Fill (Image - Image Type: Tiled - sol-stretch anchor)
///       └── (opsiyonel) NameText (TMP)
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private BossController boss;        // Hangi bossu izleyecek
    [SerializeField] private BossHealth bossHealth;      // boss.GetComponent<BossHealth>() ile auto-bağlanır
    [SerializeField] private CanvasGroup canvasGroup;    // Fade için (yoksa otomatik eklenir)
    [SerializeField] private RectTransform fillRect;     // Genişliği animate edilecek "Fill" objesi
    [SerializeField] private RectTransform shakeTarget;  // Hasar alınca sallanacak obje (genelde bar root)
    [SerializeField] private TextMeshProUGUI nameText;   // Opsiyonel — boss adı
    [SerializeField] private string bossDisplayName = "Cerberus";

    [Header("Animasyon Süreleri")]
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeOutDelay = 1.5f;  // Boss öldükten kaç sn sonra kaybolsun
    [SerializeField] private float fillTweenDuration = 0.35f;
    [SerializeField] private Ease fillEase = Ease.OutCubic;

    [Header("Hasar Efekti")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 14;
    [SerializeField] private bool flashOnHit = true;
    [SerializeField] private Image flashImage;           // Opsiyonel — kırmızı flash için Frame'in üzerine konur
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f, 0.6f);

    private float _maxFillWidth;
    private Tween _fillTween;
    private Tween _flashTween;
    private Sequence _shakeSeq;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (boss != null && bossHealth == null)
            bossHealth = boss.GetComponent<BossHealth>();

        // Maksimum fill genişliğini cache'le (mevcut boyut maks kabul ediliyor)
        if (fillRect != null)
            _maxFillWidth = fillRect.rect.width;

        // Başlangıçta gizli
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        if (nameText != null) nameText.text = bossDisplayName;
        if (flashImage != null)
        {
            var c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }

    private void OnEnable()
    {
        if (boss != null) boss.onActivated.AddListener(HandleActivated);
        if (bossHealth != null)
        {
            bossHealth.onHealthChanged.AddListener(HandleHealthChanged);
            bossHealth.onBossDied.AddListener(HandleBossDied);
        }
    }

    private void OnDisable()
    {
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

    // ─── Event Handler'lar ─────────────────────────────────────

    private void HandleActivated()
    {
        // Fill'i doluya çek (boss başlamadan önce HP eventi gönderildi olabilir)
        if (bossHealth != null) SetFillImmediate(bossHealth.CurrentHealth, bossHealth.MaxHealth);

        canvasGroup.DOKill();
        canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (fillRect == null) return;

        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        float targetWidth = _maxFillWidth * ratio;

        _fillTween?.Kill();
        _fillTween = fillRect.DOSizeDelta(new Vector2(targetWidth, fillRect.sizeDelta.y), fillTweenDuration)
                              .SetEase(fillEase);

        // Hasar efekti — sadece bar görünürken
        if (canvasGroup.alpha > 0.1f)
        {
            PlayShake();
            if (flashOnHit && flashImage != null) PlayFlash();
        }
    }

    private void HandleBossDied()
    {
        // Fill'i 0'a tween et + bekle + fade out
        _fillTween?.Kill();
        _fillTween = fillRect.DOSizeDelta(new Vector2(0f, fillRect.sizeDelta.y), fillTweenDuration)
                              .SetEase(fillEase);

        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, fadeOutDuration).SetDelay(fadeOutDelay).SetEase(Ease.InQuad);
    }

    // ─── Yardımcılar ───────────────────────────────────────────

    private void SetFillImmediate(int current, int max)
    {
        if (fillRect == null) return;
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        fillRect.sizeDelta = new Vector2(_maxFillWidth * ratio, fillRect.sizeDelta.y);
    }

    private void PlayShake()
    {
        if (shakeTarget == null) return;
        _shakeSeq?.Kill();

        Vector3 originalPos = shakeTarget.anchoredPosition3D;
        _shakeSeq = DOTween.Sequence();
        _shakeSeq.Append(shakeTarget.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato, 90, false, true));
        _shakeSeq.OnComplete(() => shakeTarget.anchoredPosition3D = originalPos);
    }

    private void PlayFlash()
    {
        _flashTween?.Kill();
        flashImage.color = flashColor;
        _flashTween = flashImage.DOFade(0f, 0.35f).SetEase(Ease.OutQuad);
    }
}
