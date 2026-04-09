using System;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public Sound[] sounds;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // AudioSource'ları oluşturma kısmı aynı
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // ARTIK NESNE DÖNDÜRÜYORUZ
    public Sound Get(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Ses bulunamadı: " + name);
            return null; // Hata durumunda null döner
        }

        return s; // Bulunan Sound nesnesini olduğu gibi ver
    }

    [ContextMenu("Sounds Settings Set Default")]
    public void ApplyCodeDefaults()
    {
        // 1. Koddaki "public float volume = 0.5f" gibi güncel değerleri taşıyan
        //    geçici, boş bir referans nesnesi oluşturuyoruz.
        Sound referansSes = new Sound();

        foreach (Sound s in sounds)
        {
            // 2. Listedeki her sesin AYARLARINI bu referanstan alıyoruz.
            //    DİKKAT: Name ve Clip'i eşitlemiyoruz, onlar özel kalmalı.

            s.volume = referansSes.volume; // Kodda 0.5f yazdıysan buraya 0.5f gelir
            s.pitch = referansSes.pitch;   // Kodda 1.2f yazdıysan buraya 1.2f gelir

            // Eğer loop varsayılanını da değiştirdiysen:
            // s.loop = referansSes.loop; 
        }

        Debug.Log($"Tüm sesler koddaki varsayılan değerlere (Vol: {referansSes.volume}, Pitch: {referansSes.pitch}) güncellendi!");
    }
}
