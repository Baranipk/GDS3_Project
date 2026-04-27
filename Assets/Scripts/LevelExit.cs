using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [Header("Geçiş Ayarları")]
    [Tooltip("Eğer işaretliyse, Build Settings'teki bir sonraki sahneye (Level 1 -> Level 2) geçer.")]
    public bool loadNextLevelAutomatically = true;

    [Tooltip("Eğer üstteki kutucuk kapalıysa, buraya yazdığın isimdeki sahneye geçer.")]
    public string specificLevelName;

    private bool _hasTriggered = false; // Karakterin kapıya iki kere çarpıp bug yaratmasını engeller

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Çarpan obje "Player" etiketine sahipse ve kapı daha önce tetiklenmediyse
        if (collision.CompareTag("Player") && !_hasTriggered)
        {
            _hasTriggered = true; // Kapıyı kilitliyoruz ki aynı anda iki kere sahne yüklemeye çalışmasın

            Debug.Log("Level bitti! Yeni sahne yükleniyor...");

            // Eğer otomatik geçiş seçiliyse LevelManager'daki sıradaki sahne fonksiyonunu çağır
            if (loadNextLevelAutomatically)
            {
                LevelManager.Instance.LoadNextScene();
            }
            // Değilse, özel ismini yazdığın sahneyi yükle
            else
            {
                LevelManager.Instance.LoadScene(specificLevelName);
            }
        }
    }
}