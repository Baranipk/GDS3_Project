using UnityEngine;

public class SkeletonController : EnemyController
{
    [Header("Hareket ve Menzil")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float stopDistance = 1.2f;

    [Header("Seçenekler")]
    public bool startAsPatrol = true;
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Saldırı Ayarları (Melee)")]
    public Transform attackPoint;      // Hasarın çıkacağı merkez nokta
    public float attackRange = 0.8f;   // Hasar verme dairesinin yarıçapı
    public LayerMask playerLayer;      // Sadece "Player" katmanına hasar vermek için
    public int attackDamage = 1;
    public float attackCooldown = 2.5f;
    [HideInInspector] public float lastAttackTime;

    [Header("Temas Hasarı Ayarları")]
    public int contactDamage = 1;            // Dokunduğunda vereceği hasar
    public float contactDamageCooldown = 1f; // Hasar verme sıklığı (saniye)
    private float nextContactDamageTime;

    // State'ler
    public SkeletonIdleState idleState;
    public SkeletonPatrolState patrolState;
    public SkeletonChaseState chaseState;
    public SkeletonAttackState attackState;
    public SkeletonHurtState hurtState;
    public SkeletonDeathState deathState;

    protected override void Awake()
    {
        base.Awake();
        idleState = new SkeletonIdleState(this);
        patrolState = new SkeletonPatrolState(this);
        chaseState = new SkeletonChaseState(this);
        attackState = new SkeletonAttackState(this);
        hurtState = new SkeletonHurtState(this);
        deathState = new SkeletonDeathState(this);
    }

    private void Start()
    {
        if (startAsPatrol)
            StateMachine.Initialize(patrolState);
        else
            StateMachine.Initialize(idleState);
    }

    // --- YENİ MELEE ATTACK METODU ---
    // Animasyon Event üzerinden bunu çağıracağız
    public void PerformMeleeAttack()
    {
        if (GetComponent<EnemyHealth>().IsDead) return;

        Debug.Log("Saldırı metodu çalıştı!"); // 1. Kontrol: Event çalışıyor mu?

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

        if (hitPlayer != null)
        {
            Debug.Log("Oyuncu tespit edildi: " + hitPlayer.name); // 2. Kontrol: Menzil ve Layer doğru mu?

            Health playerHealth = hitPlayer.GetComponentInParent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
        else
        {
            Debug.Log("Saldırı yapıldı ama kimseye değmedi.");
        }
    }

    public void FinalizeAttack()
    {
        if (GetComponent<EnemyHealth>().IsDead) return;
        if (StateMachine.CurrentState == attackState)
        {
            StateMachine.ChangeState(chaseState);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Durma/Takip Menzilleri
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // 2. SALDIRI ALANI (Vuruş Noktası) - Editörde görmek için çok önemli
        if (attackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void OnAttackComplete()
    {
        if (StateMachine.CurrentState == attackState)
        {
            StateMachine.ChangeState(chaseState);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleContactDamage(collision.collider);
    }

    // Eğer iskeletin collider'ı "Is Trigger" SE bu metod çalışır
    private void OnTriggerStay2D(Collider2D other)
    {
        HandleContactDamage(other);
    }

    private void HandleContactDamage(Collider2D other)
    {
        // 1. Çarptığımız obje Player mı? (Tag kontrolü)
        // İpucu: Player collider'ın child objede olduğu için CompareTag'i dikkatli kullanmalıyız.
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            // 2. Hasar verme zamanı geldi mi?
            if (Time.time >= nextContactDamageTime)
            {
                // 3. Health bileşenine ulaş
                Health playerHealth = other.GetComponentInParent<Health>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(contactDamage);
                    Debug.Log("İskelet temas yoluyla hasar verdi!");

                    // Bekleme süresini güncelle
                    nextContactDamageTime = Time.time + contactDamageCooldown;
                }
            }
        }
    }
}