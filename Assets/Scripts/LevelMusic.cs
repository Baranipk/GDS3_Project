using UnityEngine;

/// <summary>
/// Sahnedeki herhangi bir objeye eklenir — sahne yüklendiğinde
/// belirtilen müziği SoundManager.PlayMusic ile başlatır.
///
/// Her level scene'inde bir tane bulunması yeterli.
/// Boss arenalarında: boss aktive olunca BossController'ın kendi müziği
/// (CerberusMusic/ZagreusMusic) bunun üzerine crossfade olur.
/// </summary>
public class LevelMusic : MonoBehaviour
{
    [Header("Müzik")]
    [Tooltip("SoundManager'da tanımlı müzik ID'si (örn 'Level1Music', 'CerberusMusic')")]
    public string musicName = "";

    [Header("Davranış")]
    [Tooltip("Sahne yüklendiğinde otomatik başlasın mı?")]
    public bool playOnStart = true;

    [Tooltip("Önceden başka müzik çalıyorsa crossfade uygula — kapatırsan anında geçer")]
    public bool useFade = true;

    [Tooltip("Bu obje yok olunca müzik dursun mu? (genelde KAPALI bırak — sahne değişiminde yeni LevelMusic devralır)")]
    public bool stopOnDestroy = false;

    [Header("Fade Süreleri (kullanılırsa)")]
    public float fadeInDuration  = 1.0f;
    public float fadeOutDuration = 1.0f;

    private void Start()
    {
        if (playOnStart) PlayThisMusic();
    }

    /// <summary>Manuel tetikleme için public (örn cutscene sonu).</summary>
    public void PlayThisMusic()
    {
        if (string.IsNullOrEmpty(musicName))
        {
            Debug.LogWarning($"[LevelMusic] '{gameObject.name}' objesinde Music Name boş — müzik çalmayacak.", this);
            return;
        }

        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[LevelMusic] SoundManager.Instance bulunamadı.", this);
            return;
        }

        if (useFade)
            SoundManager.Instance.PlayMusic(musicName, fadeInDuration, fadeOutDuration);
        else
            SoundManager.Instance.PlayMusic(musicName, 0f, 0f);
    }

    private void OnDestroy()
    {
        if (stopOnDestroy && SoundManager.Instance != null)
            SoundManager.Instance.StopMusic(useFade ? fadeOutDuration : 0f);
    }
}
