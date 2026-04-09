using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 0.5f;
    [Range(0f, 3f)] public float pitch = 1.2f;
    public bool loop;

    [HideInInspector]
    public AudioSource source;

    // --- YENİ FONKSİYONLAR ---

    // Sesi Çal
    public Sound Play()
    {
        source.Play();
        return this; // Nesnenin kendisini döndürür (Zincirleme için)
    }

    // Tek seferlik çal (Üst üste binen efektler için)
    public Sound PlayOneShot()
    {
        source.PlayOneShot(clip);
        return this;
    }

    // Sesi Durdur
    public void Stop()
    {
        source.Stop();
    }

    // Ses Seviyesini Anlık Değiştir
    public Sound SetVolume(float vol)
    {
        source.volume = vol;
        return this;
    }

    // Ses Tonunu (Hızını) Anlık Değiştir
    public Sound SetPitch(float p)
    {
        source.pitch = p;
        return this;
    }

    // Sesin çalıp çalmadığını kontrol et
    public bool IsPlaying()
    {
        return source.isPlaying;
    }
}