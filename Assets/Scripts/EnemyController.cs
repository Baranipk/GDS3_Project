using UnityEngine;

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
            Health playerHealth = playerHit.GetComponentInParent<Health>();

            if (playerHealth != null && !playerHealth.isInvincible) // isInvincible (I-Frame) kontrolü
            {
                playerHealth.TakeDamage(contactDamage);
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
}