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
        // Çarptığımız obje veya ebeveyni "Enemy" tag'ine mi sahip?
        if (collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy"))
        {
            // GetComponentInParent: Alt objeye (collider) çarpsak bile 
            // gidip ana objedeki EnemyHealth scriptini bulur.
            EnemyHealth enemyHealth = collision.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Sabit slash {collision.name} objesine {damage} hasar verdi!");

                // Sabit duran slash'larda genellikle çarptığında yok etmeyiz,
                // animasyonun (0.3s) tamamlanması daha profesyonel görünür.
            }
        }
    }
}