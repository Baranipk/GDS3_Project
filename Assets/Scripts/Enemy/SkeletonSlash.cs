using UnityEngine;

public class SkeletonSlash : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 0.5f;

    public void Setup(float dir)
    {
        // Animator rotation veya scale'i override edebileceğinden,
        // yön flip'i için root objenin localScale.x'ini kullanıyoruz.
        // Bu yöntem animator ile çakışmaz, animasyon düzgün oynar.

        Vector3 scale = transform.localScale;
        // Prefab'ın default yönü SOLA (-x) olduğu varsayımıyla:
        // Sola bakıyorsa (dir < 0) → scale.x pozitif bırak (default)
        // Sağa bakıyorsa (dir > 0) → scale.x negatif yap (aynala)
        scale.x = dir > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}