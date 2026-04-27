using System;
using UnityEngine;
using UnityEngine.Audio; // AudioMixer için gerekli

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Ses Ayarları")]
    public AudioMixer mainMixer; // Unity'deki Mixer dosyamız
    public Sound[] sounds;

    void Awake()
    {
        // 1. Singleton ve Kalıcılık (Sahneler arası geçişte yok olmaz)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        // Ana menü müziğini başlatmak için (İsmini "Theme" yaptığını varsayıyorum)
        Get("Theme")?.Play();
    }

    // --- YENİ: SES SEVİYESİ KAYDETME VE DEĞİŞTİRME ---

    public void SetVolume(string parameterName, float sliderValue)
    {
        // Slider değeri (0 ile 1 arası) Logaritmik desibel (dB) değerine çevrilir (-80dB ile 0dB arası)
        // Log10(0) tanımsız olduğu için minimum 0.0001f veriyoruz
        float val = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float dbValue = Mathf.Log10(val) * 20f;

        mainMixer.SetFloat(parameterName, dbValue); // Anlık olarak sesi değiştir
        PlayerPrefs.SetFloat(parameterName, sliderValue); // Değeri hafızaya (PC'ye) kaydet
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
}