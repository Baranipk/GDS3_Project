using UnityEngine;

public class Javelin : MonoBehaviour
{
    [Header("Javelin Ayarları")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 3f;

    private float _direction = 1f;

    public void Setup(float facingDirection)
    {
        _direction = facingDirection;

        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * _direction;
        transform.localScale = localScale;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * _direction * speed * Time.deltaTime);
    }

    // Çarpışma Algılama
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string hitLayerName = LayerMask.LayerToName(collision.gameObject.layer);

        // 1. DÜŞMANA ÇARPMA
        if (collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                // --- YENİ: Düşmana Çarpma Sesi ---
                // Güvenlik kontrolü ile çağırıyoruz ki SoundManager yokken hata vermesin
                SoundManager.Instance?.Get("JavelinHitEnemy")?.PlayOneShot();
            }

            Destroy(gameObject);
        }
        // 2. YERE/DUVARA ÇARPMA
        else if (hitLayerName == "Ground")
        {
            Debug.Log("Javelin 'Ground' layerına sahip bir yere çarptı!");

            // --- YENİ: Duvara/Yere Çarpma Sesi ---
            SoundManager.Instance?.Get("JavelinHitWall")?.PlayOneShot();

            Destroy(gameObject);
        }
    }
}