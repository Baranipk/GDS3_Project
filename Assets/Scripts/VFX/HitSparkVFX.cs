using UnityEngine;
using DG.Tweening;

/// <summary>
/// Düşmana vurulunca çıkan anlık kıvılcım/flash VFX.
/// Sadece Particle System — metin yok.
///
/// Prefab yapısı:
///   HitSparkVFX (bu script burada)
///   ├── Particle System  → kıvılcım parçacıkları
///   └── Flash (opsiyonel) → SpriteRenderer ile beyaz flash
///
/// Particle System Inspector ayarları:
///   Duration        → 0.2
///   Looping         → ✗
///   Start Lifetime  → 0.1 ~ 0.2
///   Start Speed     → 3 ~ 6      (hızlı dışa fırlama)
///   Start Size      → 0.05 ~ 0.15
///   Start Color     → Beyaz / Sarı / Turuncu
///   Gravity         → 0.5        (hafifçe aşağı düşsün)
///   Max Particles   → 12
///
///   Shape
///     Shape   → Circle
///     Radius  → 0.05
///     Radius Thickness → 0  (kenardan fırlasın)
///
///   Emission
///     Rate over Time → 0
///     Burst: Time 0, Count 12, Cycles 1
///
///   Color over Lifetime → Sarı/Beyaz → Şeffaf
///   Size over Lifetime  → 1 → 0  (küçülerek yok olur)
///
///   Renderer
///     Material → VFX_Additive  (parlama için)
///     Order in Layer → 2
/// </summary>
public class HitSparkVFX : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private SpriteRenderer _flashSprite; // Opsiyonel beyaz flash

    [Header("Flash Ayarları")]
    [Tooltip("True: vurulma anında beyaz sprite flash çıkar")]
    [SerializeField] private bool  _useFlash     = true;
    [SerializeField] private Color _flashColor   = new Color(1f, 1f, 0.8f, 0.9f);
    [SerializeField] private float _flashDuration = 0.08f;

    [Header("Yaşam Süresi")]
    [Tooltip("VFX objesinin kaç saniye sonra yok edileceği")]
    [SerializeField] private float _lifetime = 0.4f;

    private void Awake()
    {
        if (_particles == null)
            _particles = GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// Hit Spark VFX'i oynatır.
    /// Otomatik olarak _lifetime saniye sonra yok olur.
    /// </summary>
    public void Play()
    {
        // Kıvılcım parçacıklarını başlat
        if (_particles != null)
            _particles.Play();

        // Beyaz flash efekti
        if (_useFlash && _flashSprite != null)
            PlayFlash();

        // Belirli süre sonra yok et
        Object.Destroy(gameObject, _lifetime);
    }

    private void PlayFlash()
    {
        _flashSprite.color = _flashColor;

        // Kısa sürede tamamen şeffaflaş
        _flashSprite.DOFade(0f, _flashDuration)
                    .SetEase(Ease.OutSine)
                    .SetLink(gameObject);
    }
}
