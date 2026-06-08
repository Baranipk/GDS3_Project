using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

/// <summary>
/// Üstüne basıldıktan belli süre sonra opacity'si azalarak kaybolan,
/// içinden geçilebilir hale gelen platform. Belli süre sonra geri gelir.
///
/// Kurulum (her kaybolan platform AYRI bir GameObject):
///   - Tilemap + TilemapRenderer (görsel)
///   - TilemapCollider2D
///   - CompositeCollider2D (Used By Effector ✅)
///   - Rigidbody2D (Static)
///   - PlatformEffector2D (tek yönlü)
///   - Bu script
/// Tek yönlü platform olduğu için çarpışma sadece üstten gelir.
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class CrumblingPlatform : MonoBehaviour
{
    private enum State { Idle, Warning, Fading, Gone, Respawning }

    [Header("Zamanlama")]
    [Tooltip("Üstüne basınca kaç sn sonra kaybolmaya başlar")]
    public float crumbleDelay = 0.8f;
    [Tooltip("Kaybolma (fade out) süresi")]
    public float fadeDuration = 0.4f;
    [Tooltip("0 = kalıcı kaybolur. >0 = bu kadar sn sonra geri gelir")]
    public float respawnDelay = 3f;
    [Tooltip("Geri gelirken fade-in süresi")]
    public float respawnFadeDuration = 0.3f;

    [Header("Tetikleme")]
    public LayerMask playerLayer;

    [Header("Uyarı: Sallanma")]
    public bool shakeOnWarning = true;
    public float shakeStrength = 0.06f;
    public int shakeVibrato = 18;

    [Header("Uyarı: Yanıp Sönme")]
    public bool flickerOnWarning = true;
    [Range(0f, 1f)] public float flickerMinAlpha = 0.4f;
    public int flickerCount = 4;

    [Header("Ses (opsiyonel)")]
    public string crumbleSoundName = "PlatformCrumble";
    public string respawnSoundName = "PlatformRespawn";

    private Tilemap _tilemap;
    private Collider2D _collider;
    private State _state = State.Idle;
    private Vector3 _startPos;
    private Color _baseColor;

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
        _collider = GetComponent<Collider2D>(); // Composite veya Tilemap collider
        _startPos = transform.position;
        _baseColor = _tilemap.color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_state != State.Idle) return;

        // Player layer kontrolü (tek yönlü platform olduğu için temas zaten üstten gelir)
        if (((1 << collision.collider.gameObject.layer) & playerLayer) == 0)
        {
            // Layer eşleşmezse tag ile de dene
            if (!collision.collider.CompareTag("Player") &&
                !collision.collider.transform.root.CompareTag("Player"))
                return;
        }

        StartCoroutine(CrumbleRoutine());
    }

    private IEnumerator CrumbleRoutine()
    {
        // ── 1. Uyarı fazı ──────────────────────────────────────
        _state = State.Warning;

        if (shakeOnWarning)
            transform.DOShakePosition(crumbleDelay, shakeStrength, shakeVibrato, 90, false, false)
                     .SetId(this);

        if (flickerOnWarning)
        {
            // crumbleDelay süresi boyunca alpha'yı yanıp söndür
            float perFlicker = crumbleDelay / (flickerCount * 2f);
            DOTween.Sequence().SetId(this)
                .Append(FadeAlphaTween(flickerMinAlpha, perFlicker))
                .Append(FadeAlphaTween(_baseColor.a, perFlicker))
                .SetLoops(flickerCount);
        }

        yield return new WaitForSeconds(crumbleDelay);

        // ── 2. Fade out + collider kapat ───────────────────────
        _state = State.Fading;
        if (!string.IsNullOrEmpty(crumbleSoundName))
            SoundManager.Instance?.TryPlayOneShot(crumbleSoundName);

        // Pozisyonu sıfırla (shake kaymış olabilir)
        transform.position = _startPos;

        // Collider'ı hemen kapat → fade olurken içinden geçilebilir
        if (_collider != null) _collider.enabled = false;

        yield return FadeAlphaTween(0f, fadeDuration).WaitForCompletion();

        _state = State.Gone;

        // ── 3. Respawn ─────────────────────────────────────────
        if (respawnDelay > 0f)
        {
            _state = State.Respawning;
            yield return new WaitForSeconds(respawnDelay);

            if (!string.IsNullOrEmpty(respawnSoundName))
                SoundManager.Instance?.TryPlayOneShot(respawnSoundName);

            yield return FadeAlphaTween(_baseColor.a, respawnFadeDuration).WaitForCompletion();

            if (_collider != null) _collider.enabled = true;
            _state = State.Idle;
        }
        // respawnDelay == 0 ise Gone'da kalır (kalıcı)
    }

    private Tween FadeAlphaTween(float targetAlpha, float duration)
    {
        return DOTween.To(
            () => _tilemap.color,
            c => _tilemap.color = c,
            new Color(_baseColor.r, _baseColor.g, _baseColor.b, targetAlpha),
            duration
        ).SetId(this).SetUpdate(false);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }
}
