using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;

/// <summary>
/// Level başında intro videosu oynatan controller.
/// Video bitince veya skip tuşuna basınca fade-out ile kaybolur ve oyun başlar.
///
/// Kurulum: Aşağıda detaylı (script dosyasının altında comment olarak).
/// </summary>
public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI skipPromptText;

    [Header("Davranış")]
    [Tooltip("Sahne yüklenir yüklenmez intro başlasın mı?")]
    [SerializeField] private bool playOnStart = true;

    [Tooltip("Sadece bir kez oynatılsın mı? Restart sonrası tekrar oynamasın diye AÇ.")]
    [SerializeField] private bool playOnlyOnce = true;

    [Tooltip("PlayerPrefs anahtarı (benzersiz olmalı — her level için farklı)")]
    [SerializeField] private string playerPrefsKey = "Level1IntroPlayed";

    [Header("Animasyon")]
    [SerializeField] private float fadeInDuration  = 0.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    [Header("Skip Mesajı")]
    [SerializeField] private string skipPromptTextValue = "Atlamak için boşluğa bas";
    [SerializeField] private float skipPromptDelay = 1.5f;  // Mesaj kaç sn sonra belirsin

    [Header("Müzik")]
    [Tooltip("Video başlarken oyun müziğini durdur — video'nun kendi sesi varsa şart")]
    [SerializeField] private bool stopGameMusicOnPlay = true;
    [SerializeField] private float musicFadeOutDuration = 0.3f;

    [Tooltip("Intro bittikten sonra çalmaya başlayacak müzik ID'si (örn 'Level1Music'). Boşsa müzik başlatılmaz.")]
    [SerializeField] private string musicAfterIntro = "";
    [SerializeField] private float musicAfterIntroFadeIn = 1.5f;

    [Header("Eventler")]
    public UnityEvent onIntroStart;
    public UnityEvent onIntroEnd;

    private bool _isPlaying;
    private Tween _skipPromptTween;
    private Tween _fadeTween;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        if (skipPromptText != null)
        {
            skipPromptText.text = skipPromptTextValue;
            Color c = skipPromptText.color;
            c.a = 0f;
            skipPromptText.color = c;
        }
    }

    private void Start()
    {
        // Daha önce oynatıldıysa atla
        if (playOnlyOnce && PlayerPrefs.GetInt(playerPrefsKey, 0) == 1)
        {
            SkipImmediate();
            return;
        }

        if (playOnStart) PlayIntro();
    }

    public void PlayIntro()
    {
        if (_isPlaying) return;
        if (videoPlayer == null)
        {
            Debug.LogError("[IntroVideoPlayer] VideoPlayer atanmamış!", this);
            EndIntro();
            return;
        }

        _isPlaying = true;

        // Fade-in overlay
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        _fadeTween?.Kill();
        _fadeTween = canvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);

        // Müziği durdur
        if (stopGameMusicOnPlay)
            SoundManager.Instance?.StopMusic(musicFadeOutDuration);

        // Video event'i + play
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived    += OnVideoError;

        Debug.Log($"[IntroVideo] Play çağrılıyor — Clip: {(videoPlayer.clip != null ? videoPlayer.clip.name : "NULL")}, " +
                  $"TargetTex: {(videoPlayer.targetTexture != null ? videoPlayer.targetTexture.name : "NULL")}, " +
                  $"RawImage Tex: {(displayImage.texture != null ? displayImage.texture.name : "NULL")}", this);

        videoPlayer.Prepare();
        videoPlayer.Play();

        // Skip prompt'u gecikmeli fade-in
        if (skipPromptText != null)
        {
            _skipPromptTween?.Kill();
            _skipPromptTween = skipPromptText.DOFade(0.8f, 0.5f)
                .SetDelay(skipPromptDelay)
                .SetUpdate(true);
        }

        onIntroStart?.Invoke();
    }

    private void Update()
    {
        if (!_isPlaying) return;
        if (IsSkipPressed()) EndIntro();
    }

    private bool IsSkipPressed()
    {
        // Klavye
        var kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame
                        || kb.escapeKey.wasPressedThisFrame
                        || kb.enterKey.wasPressedThisFrame))
            return true;

        // Mouse
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;

        // Gamepad (Xbox/PS/Switch tüm konsol kontrolcüleri)
        var pad = Gamepad.current;
        if (pad != null && (pad.buttonSouth.wasPressedThisFrame   // A / Cross
                         || pad.buttonEast.wasPressedThisFrame    // B / Circle
                         || pad.buttonNorth.wasPressedThisFrame   // Y / Triangle
                         || pad.buttonWest.wasPressedThisFrame    // X / Square
                         || pad.startButton.wasPressedThisFrame   // Start / Options
                         || pad.selectButton.wasPressedThisFrame))// Back / Share
            return true;

        return false;
    }

    private void OnVideoEnd(VideoPlayer vp) => EndIntro();

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log($"[IntroVideo] Hazır — Width: {vp.width}, Height: {vp.height}, Length: {vp.length:F2}s", this);
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[IntroVideo] HATA: {message}", this);
    }

    public void EndIntro()
    {
        if (!_isPlaying) return;
        _isPlaying = false;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.Stop();
        }

        if (playOnlyOnce)
        {
            PlayerPrefs.SetInt(playerPrefsKey, 1);
            PlayerPrefs.Save();
        }

        _skipPromptTween?.Kill();

        // Intro bittikten sonra oyun müziğini başlat
        StartPostIntroMusic(musicAfterIntroFadeIn);

        // Fade-out + objeyi disable et
        _fadeTween?.Kill();
        _fadeTween = canvasGroup.DOFade(0f, fadeOutDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                onIntroEnd?.Invoke();
                gameObject.SetActive(false);
            });
    }

    private void SkipImmediate()
    {
        _isPlaying = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        // Intro zaten oynamış (PlayerPrefs ile atlandı) — yine de oyun müziği başlasın
        StartPostIntroMusic(0.5f);

        gameObject.SetActive(false);
    }

    private void StartPostIntroMusic(float fadeIn)
    {
        if (string.IsNullOrEmpty(musicAfterIntro)) return;
        if (SoundManager.Instance == null) return;

        // Eğer zaten bu müzik çalıyorsa tekrar başlatma (LevelMusic component'i başlatmış olabilir)
        if (SoundManager.Instance.CurrentMusicName == musicAfterIntro) return;

        SoundManager.Instance.PlayMusic(musicAfterIntro, fadeIn, 0.5f);
    }

    /// <summary>Test için PlayerPrefs sıfırla (intro'yu tekrar oynat).</summary>
    [ContextMenu("Reset PlayerPrefs (Test)")]
    private void ResetIntroPlayed()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
        Debug.Log($"[IntroVideoPlayer] '{playerPrefsKey}' PlayerPrefs sıfırlandı.");
    }
}

// ─────────────────────────────────────────────────────────────
// KURULUM TALİMATI (Unity Editor):
//
// 1) Video dosyasını import et
//    - Video dosyanı (.mp4) Assets/Videos/ klasörüne sürükle (klasör yoksa oluştur)
//    - Inspector'da video clip'in ayarları otomatik gelir (genelde dokunmaya gerek yok)
//
// 2) RenderTexture oluştur
//    - Project → sağ tık → Create → Render Texture
//    - İsim: IntroVideoRT
//    - Size: 1920x1080 (veya video çözünürlüğü ile aynı)
//    - Color Format: R8G8B8A8_UNORM (default genelde uygun)
//
// 3) Canvas + UI hierarchy kur (Level 1 sahnesinde)
//    Canvas (Screen Space Overlay, Sort Order: 1000 — en üstte)
//    └── IntroOverlay (Empty + RectTransform stretched + CanvasGroup + IntroVideoPlayer script)
//        ├── VideoDisplay (UI > Raw Image, stretched full screen)
//        │     Texture: IntroVideoRT
//        ├── SkipPrompt (UI > TextMeshPro, alt-orta, "Atlamak için boşluğa bas")
//        └── VideoPlayerObj (Empty + Video Player component)
//              Video Clip: senin video'n
//              Render Mode: Render Texture
//              Target Texture: IntroVideoRT
//              Audio Output Mode: Direct (veya Audio Source)
//              Play On Awake: ❌ (script kontrol edecek)
//
// 4) IntroVideoPlayer inspector'ında alanları bağla:
//    - Video Player → VideoPlayerObj'deki Video Player component
//    - Display Image → VideoDisplay RawImage
//    - Canvas Group → IntroOverlay'in CanvasGroup
//    - Skip Prompt Text → SkipPrompt TMP text
//
// 5) Play On Start ✅ (default)
//    Play Only Once → sadece bir kez oynatılmasını istersen ✅
//
// 6) Test:
//    - Component sağ tık → "Reset PlayerPrefs (Test)" ile intro'yu tekrar tetikleyebilirsin
//    - Play → intro başlar, boşluk/escape/enter/sol tık ile skip
// ─────────────────────────────────────────────────────────────
