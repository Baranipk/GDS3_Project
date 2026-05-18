using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Can kazanınca karakterin üzerinde beliren yeşil "+" VFX.
///
/// Prefab yapısı:
///   HealVFX (bu script burada)
///   ├── TXM  → TextMeshPro bileşeni
///   └── Particle System → parçacık efekti
/// </summary>
public class HealVFX : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private TextMeshPro    _text;
    [SerializeField] private ParticleSystem _particles;

    [Header("Metin Rengi")]
    [SerializeField] private Color _textColor = new Color(0.3f, 1f, 0.4f, 1f);

    [Header("Animasyon Ayarları")]
    [SerializeField] private float _riseHeight = 1.2f;
    [SerializeField] private float _riseTime   = 1.0f;
    [SerializeField] private float _scaleFrom  = 0.3f;
    [SerializeField] private float _scalePeak  = 1.3f;
    [SerializeField] private float _scaleTo    = 1.0f;
    [SerializeField] private float _popTime    = 0.15f;
    [SerializeField] private float _holdTime   = 0.3f;
    [SerializeField] private float _fadeTime   = 0.4f;

    private void Awake()
    {
        if (_text      == null) _text      = GetComponentInChildren<TextMeshPro>();
        if (_particles == null) _particles = GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// VFX'i oynatır.
    /// amount: kaç can kazanıldı (+1, +2 vb.)
    /// </summary>
    public void Play(int amount = 1)
    {
        // Metni ayarla
        if (_text != null)
        {
            _text.text  = $"+{amount}";
            _text.color = _textColor;
        }

        // Parçacıkları başlat
        if (_particles != null)
            _particles.Play();

        // Başlangıç scale sıfır
        transform.localScale = Vector3.one * _scaleFrom;

        Vector3 targetPos = transform.position + Vector3.up * _riseHeight;

        Sequence seq = DOTween.Sequence().SetLink(gameObject);

        // Pop animasyonu
        seq.Append(transform.DOScale(_scalePeak, _popTime).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(_scaleTo, _popTime * 0.5f).SetEase(Ease.InOutSine));

        // Yukarı yükselme (tüm süre boyunca)
        seq.Insert(0f, transform.DOMove(targetPos, _riseTime).SetEase(Ease.OutCubic));

        // Görünür kal
        seq.AppendInterval(_holdTime);

        // Fade out
        if (_text != null)
            seq.Append(_text.DOFade(0f, _fadeTime).SetEase(Ease.InSine));

        // Animasyon bitince yok et
        seq.OnComplete(() =>
        {
            if (gameObject != null)
                Destroy(gameObject);
        });
    }
}
