using UnityEngine;

public class SwordSlash : MonoBehaviour
{
    [Header("Saldırı Ayarları")]
    [SerializeField] private int damage = 20; // Düşmana verilecek hasar
    [SerializeField] private float lifeTime = 0.3f; // Efektin sahnede kalma süresi

    public void Setup(float characterDirection)
    {
        // 1. Karakterin baktığı yöne göre slash efektini çevir
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * characterDirection;
        transform.localScale = newScale;

        // 2. Belirli bir süre sonra efekti yok et
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. DÜŞMAN ETKİLEŞİMİ (Mevcut kodun)
        if (collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Sabit slash {collision.name} objesine {damage} hasar verdi!");
            }
        }

        // 2. YENİ: ŞALTER (LEVER) ETKİLEŞİMİ
        // 2. YENİ: ŞALTER (LEVER) ETKİLEŞİMİ
        if (collision.CompareTag("Lever"))
        {
            ElevatorLever lever = collision.GetComponent<ElevatorLever>();
            if (lever != null)
            {
                lever.Interact();
                // Not: Kılıç darbesi şaltere değdiğinde yok olmasını istemiyorsan
                // Destroy(gameObject) eklemene gerek yok, lifeTime dolana kadar sahnede kalır.
            }
        }
    }
}