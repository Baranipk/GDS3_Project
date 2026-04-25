using UnityEngine;

public class SatyrController : EnemyController
{
    [Header("Patriyo (Devriye) Ayarları")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f; // Karakterin doğduğu noktadan sağa ve sola kaç birim gideceği

    // Bu değerleri hesaplayıp State içinde kullanacağız (Editörde gizli)
    [HideInInspector] public float leftBoundary;
    [HideInInspector] public float rightBoundary;

    [Header("Menzilli Saldırı Ayarları")]
    public GameObject projectilePrefab; // Fırlatılacak obje (Ok/Büyü vs.)
    public Transform throwPoint;        // Objenin nereden çıkacağı
    public float attackCooldown = 2f;   // İki saldırı arası bekleme süresi
    [HideInInspector] public float lastAttackTime;

    // State'ler
    public SatyrIdleState idleState;
    public SatyrPatrolState patrolState;
    public SatyrAttackState attackState;
    public SatyrHurtState hurtState;
    public SatyrDeathState deathState;

    protected override void Awake()
    {
        base.Awake();

        // State'leri oluştur
        idleState = new SatyrIdleState(this);
        patrolState = new SatyrPatrolState(this);
        attackState = new SatyrAttackState(this);
        hurtState = new SatyrHurtState(this);
        deathState = new SatyrDeathState(this);
    }

    private void Start()
    {
        // 1. Karakterin başlangıç pozisyonuna göre sınırları hesapla
        leftBoundary = transform.position.x - patrolDistance;
        rightBoundary = transform.position.x + patrolDistance;

        // 2. Başlangıçta devriye gezmeye başla
        StateMachine.Initialize(patrolState);
    }

    // --- SALDIRI METODU (Animation Event ile çağrılacak) ---
    public void ShootProjectile()
    {
        if (GetComponent<EnemyHealth>().IsDead) return;

        if (projectilePrefab != null && throwPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, throwPoint.rotation);

            Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            if (projectile.TryGetComponent(out EnemyProjectile projScript))
            {
                projScript.Setup(shootDirection);
            }
        }
    }

    public void OnAttackComplete()
    {
        if (StateMachine.CurrentState == attackState)
        {
            StateMachine.ChangeState(idleState);
        }
    }

    // --- GÖRSELLEŞTİRME (Sadece Unity Editöründe Çalışır) ---
    private void OnDrawGizmosSelected()
    {
        // Devriye menzilini yeşil bir çizgi olarak göster
        Gizmos.color = Color.green;
        Vector3 leftBound = new Vector3(transform.position.x - patrolDistance, transform.position.y, transform.position.z);
        Vector3 rightBound = new Vector3(transform.position.x + patrolDistance, transform.position.y, transform.position.z);

        Gizmos.DrawLine(leftBound, rightBound);
        Gizmos.DrawWireSphere(leftBound, 0.2f);
        Gizmos.DrawWireSphere(rightBound, 0.2f);
    }
}