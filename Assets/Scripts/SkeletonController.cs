using UnityEngine;

public class SkeletonController : EnemyController
{
    [Header("Hareket ve Menzil")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float stopDistance = 1.2f; // Saldırıya başlama mesafesi

    [Header("Seçenekler")]
    public bool startAsPatrol = true; // Inspector üzerinden değiştirilebilir
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Saldırı")]
    public GameObject slashProjectilePrefab;
    public float attackCooldown = 2.5f;
    [HideInInspector] public float lastAttackTime;

    [Header("Slash Spawn Offset")]
    [Tooltip("İskeletin merkezinden sağa/sola olan yatay mesafe")]
    public float spawnOffsetX = 0.5f;
    [Tooltip("İskeletin merkezinden yukarı/aşağı olan dikey mesafe")]
    public float spawnOffsetY = 0f;

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

        // State Instance'larını oluştur
        idleState = new SkeletonIdleState(this);
        patrolState = new SkeletonPatrolState(this);
        chaseState = new SkeletonChaseState(this);
        attackState = new SkeletonAttackState(this);
        hurtState = new SkeletonHurtState(this);
        deathState = new SkeletonDeathState(this);
    }

    private void Start()
    {
        // Başlangıç moduna göre State Machine'i başlat
        if (startAsPatrol)
            StateMachine.Initialize(patrolState);
        else
            StateMachine.Initialize(idleState);
    }

    // Animation Event tarafından çağrılacak
    public void SpawnSlash()
    {
        if (GetComponent<EnemyHealth>().IsDead) return;
        if (slashProjectilePrefab == null) return;

        // İskeletin baktığı yönü hesapla (+1 sağ, -1 sol)
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;

        // Spawn pozisyonunu inspector offset'lerine göre hesapla
        Vector3 spawnPos = transform.position + new Vector3(spawnOffsetX * facingDir, spawnOffsetY, 0f);

        // Efekti doğru pozisyonda oluştur
        GameObject slash = Instantiate(slashProjectilePrefab, spawnPos, Quaternion.identity);

        // Yönü mermiye gönder
        if (slash.TryGetComponent(out SkeletonSlash projectile))
        {
            projectile.Setup(facingDir);
        }
    }

    public void FinalizeAttack()
    {
        // Eğer iskelet çoktan öldüyse, animasyon bitse bile durumu değiştirme
        if (GetComponent<EnemyHealth>().IsDead) return;

        if (StateMachine.CurrentState == attackState)
        {
            StateMachine.ChangeState(chaseState);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Saldırı Menzili (Stop Distance) - Kırmızı
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // 2. Fark Etme Menzili (Detection Radius) - Mavi
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 3. Kaybetme Menzili (Lose Radius) - Sarı
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        // 4. Slash Spawn Noktası - Cyan (her iki yön için)
        Gizmos.color = Color.cyan;
        Vector3 rightSpawn = transform.position + new Vector3(spawnOffsetX, spawnOffsetY, 0f);
        Vector3 leftSpawn = transform.position + new Vector3(-spawnOffsetX, spawnOffsetY, 0f);
        Gizmos.DrawWireSphere(rightSpawn, 0.15f);
        Gizmos.DrawWireSphere(leftSpawn, 0.15f);
    }

    // Saldırı animasyonu tamamen bittiğinde state değiştirmek için çağıracağız
    public void OnAttackComplete()
    {
        if (StateMachine.CurrentState == attackState)
        {
            StateMachine.ChangeState(chaseState);
        }
    }
}