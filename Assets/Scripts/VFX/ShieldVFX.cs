using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Kalkan kazanınca karakterin etrafında beliren mavi halka/patlama VFX.
///
/// Prefab yapısı:
///   ShieldVFX (bu script burada)
///   ├── TXM          → TextMeshPro bileşeni
///   └── Particle System → halka patlama efekti
///
/// Particle System ayarları:
///   Start Speed     → 2.5
///   Start Size      → 0.15 ~ 0.25
///   Start Lifetime  → 0.3 ~ 0.5
///   Start Color     → #00AAFF
///   Gravity         → 0
///   Shape           → Circle, Radius: 0.1, Radius Thickness: 0
///   Emission Burst  → Time:0, Count:16, Cycles:1
///   Color over Lifetime → mavi → şeffaf
///   Renderer Material   → VFX_Additive
/// </summary>
public class ShieldVFX : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private ParticleSystem _particles;

    [Header("Metin Ayarları")]
    [Tooltip("Yazının etiketi — örn: 'Shield', 'Guard', 'Block'")]
    [SerializeField] private string _label = "Shield";

    [Tooltip("Yazı rengi")]
    [SerializeField] private Color _textColor = new Color(0.3f, 0.8f, 1f, 1f);

    [Tooltip("True: '+1 Shield' | False: sadece '+1'")]
    [SerializeField] private bool _showLabel = true;

    [Header("Animasyon Ayarları")]
    [SerializeField] private float _riseHeight = 1.0f;
    [SerializeField] private float _riseTime = 1.0f;
    [SerializeField] private float _scaleFrom = 0.2f;
    [SerializeField] private float _scalePeak = 1.4f;
    [SerializeField] private float _scaleTo = 1.0f;
    [SerializeField] private float _popTime = 0.12f;
    [SerializeField] private float _holdTime = 0.4f;
    [SerializeField] private float _fadeTime = 0.4f;

    private void Awake()
    {
        if (_text == null) _text = GetComponentInChildren<TextMeshPro>();
        if (_particles == null) _particles = GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// Shield VFX'i oynatır.
    /// amount: kaç kalkan kazanıldı
    /// </summary>
    public void Play(int amount = 1)
    {
        // Metni oluştur — _showLabel true ise label'ı ekle
        if (_text != null)
        {
            _text.text = _showLabel
                ? $"+{amount} {_label}"   // örn: "+1 Shield"
                : $"+{amount}";           // örn: "+1"
            _text.color = _textColor;
        }

        // Halka patlama parçacıklarını başlat
        if (_particles != null)
            _particles.Play();

        // Başlangıç scale sıfır
        transform.localScale = Vector3.one * _scaleFrom;

        Vector3 targetPos = transform.position + Vector3.up * _riseHeight;

        Sequence seq = DOTween.Sequence().SetLink(gameObject);

        // Pop animasyonu
        seq.Append(transform.DOScale(_scalePeak, _popTime).SetEase(Ease.OutBack, 3f));
        seq.Append(transform.DOScale(_scaleTo, _popTime * 0.5f).SetEase(Ease.InOutSine));

        // Yukarı yüksel
        seq.Insert(0f, transform.DOMove(targetPos, _riseTime).SetEase(Ease.OutCubic));

        // Görünür kal
        seq.AppendInterval(_holdTime);

        // Fade out
        if (_text != null)
            seq.Append(_text.DOFade(0f, _fadeTime).SetEase(Ease.InSine));

        // Yok et
        seq.OnComplete(() =>
        {
            if (gameObject != null)
                Destroy(gameObject);
        });
    }
}