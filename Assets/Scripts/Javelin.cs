using UnityEngine;

public class Javelin : MonoBehaviour
{
    [Header("Javelin Ayarları")]
    [SerializeField] private int damage = 15;        // YENİ: Düşmana verilecek hasar
    [SerializeField] private float speed = 15f;      // Uçuş hızı
    [SerializeField] private float lifeTime = 3f;    // Hiçbir şeye çarpmazsa kaç saniye sonra yok olsun?

    private float _direction = 1f;

    // PlayerAttack scripti bu mızrağı oluşturduğunda yönünü belirlemek için bu metodu çağıracak
    public void Setup(float facingDirection)
    {
        _direction = facingDirection;

        // Mızrağın yönünü karakterin baktığı yöne çevir
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * _direction;
        transform.localScale = localScale;

        // Güvenlik önlemi: Haritadan çıkıp sonsuza kadar gitmemesi için belli bir süre sonra yok et
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Mızrağı her karede ileri doğru hareket ettir
        transform.Translate(Vector3.right * _direction * speed * Time.deltaTime);
    }

    // Çarpışma Algılama
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Eğer düşmana veya düşman objesinin herhangi bir alt parçasına çarparsa
        if (collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy"))
        {
            // GetComponentInParent: Alt objeye (collider) çarpsak bile gidip ana objedeki EnemyHealth scriptini bulur
            EnemyHealth enemyHealth = collision.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage); // Hasar ver
                Debug.Log($"Javelin {collision.name} objesine çarptı ve {damage} hasar verdi!");
            }

            Destroy(gameObject); // Düşmana çarptığında mızrağı yok et
        }
        // Eğer yere veya duvara çarparsa
        else if (collision.CompareTag("Ground"))
        {
            Debug.Log("Javelin duvara/yere çarptı!");
            Destroy(gameObject); // Javelin'i yok et
        }
    }
}