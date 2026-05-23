using UnityEngine;

[System.Serializable]
public class LootItem
{
    public GameObject itemPrefab; // Düşecek Collectible (Can, Kalkan vb.) prefabı
    [Range(0f, 100f)]
    public float dropChance;      // % kaç ihtimalle düşecek (Örn: 25 = %25)
}

public abstract class EnemyController : MonoBehaviour
{
    public EnemyStateMachine StateMachine { get; private set; }

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public EnemyAnimation enemyAnim;
    [HideInInspector] public Transform player;

    [Header("Ortak AI Ayarları")]
    public float detectionRadius = 6f;
    public float loseRadius = 10f;

    [Header("Ortak Temas Hasarı Ayarları")]
    public int contactDamage = 1;            // Dokunduğunda vereceği hasar
    public float contactDamageCooldown = 1f; // Hasar verme sıklığı (saniye)
    private float _nextContactDamageTime;    // Bir sonraki hasar zamanını tutar

    [Header("Loot (Ganimet) Ayarları")]
    public LootTable lootTable;
    [Range(0f, 100f)]
    public float dropChance = 100f; // Varsayılan olarak %100

    [Header("Knockback")]
    public Vector2 knockbackForce = new Vector2(6f, 4f);  // Hasar alınca temel itme
    public float deathKnockbackMultiplier = 2.5f;          // Ölüm anında çarpan (daha güçlü)
    public float hurtKnockbackMultiplier = 1f;             // Normal hasar çarpanı

    public bool isFacingRight = true;

    protected virtual void Awake()
    {
        StateMachine = new EnemyStateMachine();
        rb = GetComponent<Rigidbody2D>();
        enemyAnim = GetComponent<EnemyAnimation>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    protected virtual void Update()
    {
        StateMachine.CurrentState?.Update();
    }

    protected virtual void FixedUpdate()
    {
        StateMachine.CurrentState?.FixedUpdate();
    }

    public void CheckFlip(float moveDirectionX)
    {
        if (moveDirectionX > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveDirectionX < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    

    private void LateUpdate()
    {
        // Sadece ölü değilse ve zamanı geldiyse temas hasarını kontrol et
        if (Time.time >= _nextContactDamageTime && !GetComponent<EnemyHealth>().IsDead)
        {
            CheckContactDamage();
        }
    }

    private void CheckContactDamage()
    {
        // Karakterin merkezinde küçük bir daire oluşturup "Player" arıyoruz
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, 0.5f); // 0.5f boyutunu karakterine göre ayarlayabilirsin

        if (playerHit != null && (playerHit.CompareTag("Player") || playerHit.transform.root.CompareTag("Player")))
        {
            PlayerHealth playerHealth = playerHit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null && !playerHealth.isInvincible) // isInvincible (I-Frame) kontrolü
            {
                playerHealth.TakeDamage(contactDamage);
                SoundManager.Instance?.TryPlayOneShot("EnemyContactHit");
                Debug.Log($"{gameObject.name} (Ortak Sistem) temas hasarı verdi!");

                // Bekleme süresini güncelle
                _nextContactDamageTime = Time.time + contactDamageCooldown;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        // Temas hasarı menzilini göster
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    /// <summary>
    /// Düşmanı verilen kaynak pozisyonundan uzaklaştıracak yönde iter.
    /// sourcePos atayanın (silahın/player'ın) pozisyonu.
    /// </summary>
    public void ApplyKnockback(Vector2 sourcePos, float multiplier = 1f)
    {
        if (rb == null) return;

        float dirX = Mathf.Sign(transform.position.x - sourcePos.x);
        if (dirX == 0f) dirX = isFacingRight ? -1f : 1f; // Aynı x'teyse arkaya at

        rb.linearVelocity = Vector2.zero;
        Vector2 force = new Vector2(dirX * knockbackForce.x, knockbackForce.y) * multiplier;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void DropLoot()
    {
        if (lootTable == null) return;

        // 1. ÖNCE ŞANS KONTROLÜ: Düşman loot verecek mi vermeyecek mi?
        float roll = Random.Range(0f, 100f);

        // Eğer atılan zar (Örn: 65), bizim şansımızdan (Örn: 30) büyükse hiçbir şey düşürme
        if (roll > dropChance)
        {
            return;
        }

        // 2. Şans tuttuysa tablodan ağırlıklı rastgele bir eşya çek
        GameObject itemToDrop = lootTable.GetRandomLoot();

        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.identity);
        }
    }
}