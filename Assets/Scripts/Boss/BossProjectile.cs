using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public float speed = 8f;
    public int damage = 2;
    public float lifeTime = 4f;
    public bool rotateToDirection = true;

    [Header("VFX (opsiyonel)")]
    public GameObject impactVFXPrefab;
    public float impactVFXDuration = 1f; // VFX kaç saniye sonra yok edilecek

    private Vector2 _direction;

    private void Awake()
    {
        // Güvenlik: Setup çağrılmasa bile mermi kendini imha etsin
        Destroy(gameObject, lifeTime);
    }

    public void Setup(Vector2 moveDirection)
    {
        _direction = moveDirection.normalized;

        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void Update()
    {
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            PlayerHealth ph = collision.GetComponentInParent<PlayerHealth>();
            if (ph != null && !ph.isInvincible) ph.TakeDamage(damage);
            SpawnImpact();
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            SpawnImpact();
            Destroy(gameObject);
        }
    }

    private void SpawnImpact()
    {
        if (impactVFXPrefab == null) return;
        GameObject vfx = Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
        Destroy(vfx, impactVFXDuration); // VFX otomatik temizlenir
    }
}
