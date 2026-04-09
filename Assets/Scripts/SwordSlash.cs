using UnityEngine;

public class SwordSlash : MonoBehaviour
{
    // Hız (speed) değişkenine artık gerek kalmadı, sildik.

    // ÖNEMLİ: Bu objenin yok olması için hala bir yönteme ihtiyacı var.
    // Önceki mesajdaki "Yöntem 1 (Animation Event)" veya "Yöntem 2 (Timer)"ı uyguladığından emin ol.
    // Aşağıdaki örnekte basitlik adına timer kullanıyoruz:
    [SerializeField] private float lifeTime = 0.3f;

    public void Setup(float characterDirection)
    {
        // Karakterin baktığı yöne göre slash efektini çevir
        Vector3 newScale = transform.localScale;

        // Mathf.Abs(newScale.x) kılıcın orijinal genişliğini korur,
        // characterDirection (1 veya -1) ise onu sağa veya sola çevirir.
        newScale.x = Mathf.Abs(newScale.x) * characterDirection;
        transform.localScale = newScale;

        // Belirli bir süre sonra yok et (Eğer Animation Event kullanmıyorsan)
        Destroy(gameObject, lifeTime);
    }

    // --- Update() METODUNU TAMAMEN SİLDİK ---
    // Böylece obje olduğu yerde sabit kalacak.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Düşmana hasar verme mantığı buraya...
            Debug.Log("Sabit slash düşmana değdi!");

            // Sabit bir efekti düşmana değince yok etmek istemeyebilirsin,
            // (animasyonun tamamlanması daha iyi görünür). 
            // Bu yüzden Destroy(gameObject) satırını buraya eklemiyoruz.
        }
    }
}