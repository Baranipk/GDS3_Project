using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public float speed = 7f;         // Uçuş hızı
    public int damage = 1;           // Vereceği hasar
    public float lifeTime = 3f;      // Kaç saniye sonra kendi kendine yok olacağı

    private Vector2 _direction;

    // Fırlatıldığı anda yönünü ayarlamak için çağıracağımız metot
    public void Setup(Vector2 moveDirection)
    {
        _direction = moveDirection.normalized;

        // Merminin ekranda sonsuza kadar gitmemesi için lifeTime süresi dolunca yok et
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Mermiyi belirlenen yönde hareket ettir
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Eğer oyuncuya çarparsa
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            SoundManager.Instance?.TryPlayOneShot("EnemyProjectileHit");
            Destroy(gameObject); // Hasar verdikten sonra yok ol
        }
        // 2. Eğer duvara veya yere çarparsa (Ground layer'ı)
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            SoundManager.Instance?.TryPlayOneShot("EnemyProjectileHit");
            Destroy(gameObject); // Duvara çarpınca da yok ol
        }
    }
}