using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    [Header("Eşya Ayarları")]
    public int shieldAmount = 1; // Bu eşya kaç kalkan verecek?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Çarpan obje Player mı?
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponentInParent<Health>();

            if (playerHealth != null)
            {
                // Eğer oyuncunun kalkan sınırı dolmamışsa kalkanı ver ve objeyi yok et
                if (playerHealth.currentShield < playerHealth.maxShield)
                {
                    playerHealth.AddShield(shieldAmount);

                    // İsteğe bağlı: Burada bir ses efekti çalabilirsin!
                    // SoundManager.Instance.Get("ShieldPickup").source.Play();

                    Destroy(gameObject); // Yerden eşyayı sil
                }
            }
        }
    }
}