using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using DG.Tweening;

/// <summary>
/// Otomatik kurulumlu screen shake singleton (Cinemachine Perlin Noise tabanlı).
/// Sahnede manuel olarak eklemeye gerek yok — Unity başlar başlamaz kendini oluşturur.
/// Her sahne yüklendiğinde aktif vcam'a Perlin Noise extension ekler.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    private CinemachineVirtualCamera _vcam;
    private CinemachineBasicMultiChannelPerlin _noise;
    private NoiseSettings _runtimeProfile;
    private Tween _shakeTween;

    private const bool DEBUG = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("[ScreenShake]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<ScreenShake>();
        if (DEBUG) Debug.Log("[ScreenShake] Singleton oluşturuldu");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
        BindToActiveVirtualCamera();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Bir frame bekle, sahne ojesi init olsun
        Invoke(nameof(BindToActiveVirtualCamera), 0.05f);
    }

    /// <summary>
    /// Aktif vcam'i bulup Perlin Noise extension ekler (yoksa).
    /// </summary>
    private void BindToActiveVirtualCamera()
    {
        _vcam = FindAnyObjectByType<CinemachineVirtualCamera>();
        if (_vcam == null)
        {
            if (DEBUG) Debug.LogWarning("[ScreenShake] CinemachineVirtualCamera bulunamadı");
            return;
        }

        // Noise extension al, yoksa ekle
        _noise = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (_noise == null)
        {
            _noise = _vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (DEBUG) Debug.Log($"[ScreenShake] Perlin Noise eklendi: {_vcam.name}");
        }

        // Runtime profile yoksa oluştur
        if (_runtimeProfile == null)
            _runtimeProfile = CreateRuntimeNoiseProfile();

        // Noise'a profili ata (her seferinde, çünkü vcam değişebilir)
        _noise.m_NoiseProfile = _runtimeProfile;
        _noise.m_AmplitudeGain = 0f;
        _noise.m_FrequencyGain = 1f;
    }

    /// <summary>
    /// Runtime'da NoiseSettings ScriptableObject oluşturur.
    /// 2D pos shake (X, Y), Z rotation hafif.
    /// </summary>
    private NoiseSettings CreateRuntimeNoiseProfile()
    {
        var profile = ScriptableObject.CreateInstance<NoiseSettings>();
        profile.name = "RuntimeShakeProfile";

        // Position shake (XY) — 2D oyun için
        var posChannel = new NoiseSettings.TransformNoiseParams
        {
            X = new NoiseSettings.NoiseParams { Amplitude = 1f, Frequency = 1f },
            Y = new NoiseSettings.NoiseParams { Amplitude = 1f, Frequency = 1.3f },
            Z = new NoiseSettings.NoiseParams { Amplitude = 0f, Frequency = 0f }
        };
        profile.PositionNoise = new NoiseSettings.TransformNoiseParams[] { posChannel };

        // Rotation shake — sadece Z ekseni (2D için doğal)
        var rotChannel = new NoiseSettings.TransformNoiseParams
        {
            X = new NoiseSettings.NoiseParams { Amplitude = 0f, Frequency = 0f },
            Y = new NoiseSettings.NoiseParams { Amplitude = 0f, Frequency = 0f },
            Z = new NoiseSettings.NoiseParams { Amplitude = 0.3f, Frequency = 1.5f }
        };
        profile.OrientationNoise = new NoiseSettings.TransformNoiseParams[] { rotChannel };

        if (DEBUG) Debug.Log("[ScreenShake] Runtime noise profili oluşturuldu");
        return profile;
    }

    // ─── Public API ─────────────────────────────────────────

    /// <summary>
    /// Shake tetikler.
    /// intensity 0.3 = hafif, 0.6 = orta, 1.0+ = sert.
    /// duration belirtmezsen intensity'e göre otomatik hesaplanır.
    /// </summary>
    public void Shake(float intensity = 0.5f, float duration = -1f)
    {
        if (_noise == null) BindToActiveVirtualCamera();
        if (_noise == null)
        {
            if (DEBUG) Debug.LogWarning("[ScreenShake] Noise bulunamadı, shake atlandı");
            return;
        }

        if (duration < 0f) duration = 0.15f + intensity * 0.25f; // 0.18-0.5 arası

        // Mevcut shake'i durdur, yeni shake amplitude'ü
        _shakeTween?.Kill();

        // Intensity'i amplitude değerine çevir — Perlin için 1-4 arası iyi görünür
        float amplitude = intensity * 4f;
        _noise.m_AmplitudeGain = amplitude;
        _noise.m_FrequencyGain = 2f + intensity * 2f;

        // Amplitude'ü süre boyunca 0'a düşür
        _shakeTween = DOTween.To(
            () => _noise.m_AmplitudeGain,
            x => _noise.m_AmplitudeGain = x,
            0f,
            duration
        ).SetEase(Ease.OutQuad);

        if (DEBUG) Debug.Log($"[ScreenShake] Shake: amp={amplitude:F2}, dur={duration:F2}");
    }

    public void ShakeLight()  => Shake(0.3f);
    public void ShakeMedium() => Shake(0.6f);
    public void ShakeHeavy()  => Shake(1.0f);

    /// <summary>Mevcut shake'i anında durdur.</summary>
    public void StopShake()
    {
        _shakeTween?.Kill();
        if (_noise != null) _noise.m_AmplitudeGain = 0f;
    }
}
