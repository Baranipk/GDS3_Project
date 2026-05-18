using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Hasar alınca karakterin üzerinde beliren kırmızı "-X HP" veya "-X Shield" yazısı.
/// Play() metoduna labelOverride geçilirse varsayılan label yerine o kullanılır.
/// </summary>
public class DamageVFX : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private TextMeshPro _text;

    [Header("Metin Ayarları")]
    [Tooltip("Varsayılan etiket — örn: 'HP'")]
    [SerializeField] private string _label = "HP";
    [SerializeField] private bool _showLabel = true;
    [SerializeField] private Color _textColor = new Color(1f, 0.25f, 0.2f, 1f);

    [Header("Animasyon Ayarları")]
    [SerializeField] private float _fallDistance = 0.4f;
    [SerializeField] private float _fallTime = 0.5f;
    [SerializeField] private float _scalePeak = 1.2f;
    [SerializeField] private float _popTime = 0.08f;
    [SerializeField] private float _fadeTime = 0.3f;

    private void Awake()
    {
        if (_text == null)
            _text = GetComponentInChildren<TextMeshPro>();
    }

    /// <summary>
    /// Damage VFX'i oynatır.
    /// damage: alınan hasar miktarı
    /// labelOverride: null ise Inspector'daki _label kullanılır
    ///                "Shield" geçilirse "-1 Shield" yazar
    /// </summary>
    public void Play(int damage = 1, string labelOverride = null)
    {
        if (_text != null)
        {
            // Override varsa onu kullan, yoksa Inspector'daki label
            string activeLabel = string.IsNullOrEmpty(labelOverride) ? _label : labelOverride;
            _text.text = _showLabel ? $"-{damage} {activeLabel}" : $"-{damage}";
            _text.color = _textColor;
        }

        transform.localScale = Vector3.zero;

        Vector3 targetPos = transform.position + Vector3.down * _fallDistance;

        Sequence seq = DOTween.Sequence().SetLink(gameObject);

        // Pop
        seq.Append(transform.DOScale(_scalePeak, _popTime).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(1f, _popTime * 0.5f).SetEase(Ease.InOutSine));

        // Aşağı düş + fade
        seq.Append(transform.DOMove(targetPos, _fallTime).SetEase(Ease.InQuad));
        seq.Join(_text.DOFade(0f, _fadeTime).SetEase(Ease.InSine));

        // Yok et
        seq.OnComplete(() =>
        {
            if (gameObject != null) Destroy(gameObject);
        });
    }
}