using System;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

    [Header("Ses Ayarları")]
    public AudioMixer mainMixer; // Unity'deki Mixer dosyamız
    public Sound[] sounds;

    void Awake()
    {
        // 1. Singleton ve Kalıcılık (Sahneler arası geçişte yok olmaz)
        if (Instance == null)
        {
            Instance = this;
            
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. Sesleri Oluştur
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            // Eğer bir gruba (Music/SFX) atandıysa onu bağla
            if (s.mixerGroup != null)
                s.source.outputAudioMixerGroup = s.mixerGroup;
        }
    }

    void Start()
    {
        // Oyun başladığında kaydedilmiş ses ayarlarını yükle
        LoadVolumeSettings();

        // Ana menü müziğini PlayMusic ile başlat (fade-in olur, sonraki müzik geçişlerinde crossfade çalışır)
        PlayMusic("Theme");
    }

    // --- YENİ: SES SEVİYESİ KAYDETME VE DEĞİŞTİRME ---

    public void SetVolume(string parameterName, float sliderValue)
    {
        float val = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float dbValue = Mathf.Log10(val) * 20f;

        mainMixer.SetFloat(parameterName, dbValue);
        PlayerPrefs.SetFloat(parameterName, sliderValue);
    }

    private void LoadVolumeSettings()
    {
        // Daha önce kaydedilmiş bir ayar yoksa varsayılan olarak 1 (Maksimum) al
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetVolume("MusicVolume", musicVol);
        SetVolume("SFXVolume", sfxVol);
    }

    // ... (Get ve ApplyCodeDefaults fonksiyonların aynı kalacak) ...
    public Sound Get(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) Debug.LogWarning("Ses bulunamadı: " + name);
        return s;
    }

    /// <summary>
    /// İsimle ses arar, bulamazsa sessizce null döner (warning yok).
    /// Opsiyonel sesler için kullan.
    /// </summary>
    public Sound TryGet(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return Array.Find(sounds, sound => sound.name == name);
    }

    /// <summary>Sesi varsa one-shot olarak çalar, yoksa hiçbir şey yapmaz.</summary>
    public void TryPlayOneShot(string name)
    {
        Sound s = TryGet(name);
        if (s != null && s.source != null && s.clip != null)
            s.source.PlayOneShot(s.clip);
    }

    /// <summary>Sesi varsa Play() ile başlatır, yoksa hiçbir şey yapmaz.</summary>
    public void TryPlay(string name)
    {
        Sound s = TryGet(name);
        if (s != null) s.source?.Play();
    }

    /// <summary>Loop'taki sesi varsa durdurur, yoksa hiçbir şey yapmaz.</summary>
    public void TryStop(string name)
    {
        Sound s = TryGet(name);
        if (s != null && s.source != null && s.source.isPlaying)
            s.source.Stop();
    }

    // ── Müzik Yöneticisi ──────────────────────────────────────
    // Aynı anda tek müzik çalar. PlayMusic çağrıldığında eski müzik fade-out olur,
    // yeni müzik fade-in olarak başlar.

    private Sound _currentMusic;
    private Tween _musicFadeInTween;
    private Tween _musicFadeOutTween;

    [Header("Müzik Geçiş Süreleri")]
    public float musicFadeInDuration  = 1.0f;
    public float musicFadeOutDuration = 1.0f;

    /// <summary>
    /// Yeni bir müzik başlatır. Eski müzik varsa crossfade olur.
    /// Hedef Sound'un Loop ✅ olması önerilir.
    /// </summary>
    public void PlayMusic(string name, float fadeIn = -1f, float fadeOut = -1f)
    {
        if (string.IsNullOrEmpty(name)) return;
        Sound target = TryGet(name);
        if (target == null || target.source == null || target.clip == null) return;

        // Zaten çalıyorsa hiçbir şey yapma
        if (_currentMusic == target && target.source.isPlaying) return;

        if (fadeIn  < 0f) fadeIn  = musicFadeInDuration;
        if (fadeOut < 0f) fadeOut = musicFadeOutDuration;

        // Eski müziği fade-out et
        FadeOutCurrentMusic(fadeOut);

        // Yeni müziği fade-in başlat
        _currentMusic = target;
        target.source.volume = 0f;
        target.source.loop = true;
        if (!target.source.isPlaying) target.source.Play();

        _musicFadeInTween?.Kill();
        _musicFadeInTween = DOTween.To(
            () => target.source.volume,
            v => target.source.volume = v,
            target.volume,
            fadeIn
        ).SetUpdate(true); // Pause sırasında bile çalışsın
    }

    /// <summary>Aktif müziği fade-out ile durdurur.</summary>
    public void StopMusic(float fadeOut = -1f)
    {
        if (fadeOut < 0f) fadeOut = musicFadeOutDuration;
        FadeOutCurrentMusic(fadeOut);
        _currentMusic = null;
    }

    /// <summary>Aktif müziğin adını döner (debug için).</summary>
    public string CurrentMusicName => _currentMusic?.name;

    private void FadeOutCurrentMusic(float fadeOut)
    {
        if (_currentMusic == null || _currentMusic.source == null) return;
        Sound oldMusic = _currentMusic;

        _musicFadeOutTween?.Kill();
        _musicFadeOutTween = DOTween.To(
            () => oldMusic.source.volume,
            v => oldMusic.source.volume = v,
            0f,
            fadeOut
        ).SetUpdate(true)
         .OnComplete(() =>
         {
             if (oldMusic.source != null && oldMusic.source.isPlaying)
                 oldMusic.source.Stop();
             // Volume'u orijinal değere geri çek (sonraki Play için)
             if (oldMusic.source != null) oldMusic.source.volume = oldMusic.volume;
         });
    }

    public void ToggleMute(string parameterName, bool isMuted, float lastSliderValue)
    {
        if (isMuted)
        {
            mainMixer.SetFloat(parameterName, -80f); // Tamamen sustur
        }
        else
        {
            // Susturma kalkınca slider'daki değere geri dön
            SetVolume(parameterName, lastSliderValue);
        }
    }


}