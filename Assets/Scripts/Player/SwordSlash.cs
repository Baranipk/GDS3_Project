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
        Debug.Log($"[SwordSlash] Çarpıştı: {collision.name} (tag={collision.tag}, rootTag={collision.transform.root.tag})");

        // 1. DÜŞMAN veya BOSS ETKİLEŞİMİ
        bool isEnemy = collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy");
        bool isBoss  = collision.CompareTag("Boss")  || collision.transform.root.CompareTag("Boss");

        if (isEnemy || isBoss)
        {
            EnemyHealth enemyHealth = collision.GetComponentInParent<EnemyHealth>();
            BossHealth  bossHealth  = collision.GetComponentInParent<BossHealth>();

            Vector2 src = transform.position;
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, src);
                Debug.Log($"Sabit slash {collision.name} objesine {damage} hasar verdi!");
            }
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(damage, src);
                Debug.Log($"Sabit slash {collision.name} (Boss) objesine {damage} hasar verdi!");
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