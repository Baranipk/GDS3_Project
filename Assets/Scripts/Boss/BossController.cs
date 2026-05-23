using UnityEngine;
using UnityEngine.Events;

public class BossController : MonoBehaviour
{
    public BossStateMachine StateMachine { get; private set; }

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public BossAnimation bossAnim;
    [HideInInspector] public BossHealth health;
    [HideInInspector] public Transform player;

    [Header("Hareket")]
    public float moveSpeed = 2.5f;
    public float backstepSpeed = 1.5f;       // Player'dan kaçarken yürüme hızı
    public float stopDistance = 1.5f;

    [Header("Taktik Mesafe (boss hangi mesafeyi korumaya çalışır)")]
    public float preferredRange = 5f;         // Boss bu mesafeyi tutmaya çalışır
    public float rangeBuffer = 1.2f;          // Sweet spot toleransı
    public float rangedMinDistance = 3.5f;    // Bundan yakına ranged yapmaz
    public float tooCloseDistance = 1.8f;     // Bundan yakınsa (melee cooldown'da iken) kaçar

    [Header("Saldırı Örüntüsü")]
    [Range(0f, 1f)] public float comboChance = 0.35f;  // Saldırıdan sonra hemen ikinci saldırı şansı
    public float patternThinkInterval = 0.25f;          // Karar verme sıklığı (saniye)
    [Range(0f, 1f)] public float aggressiveness = 0.6f; // Yüksek = saldırı tercih, düşük = mesafe koru

    [Header("Hurt")]
    public float hurtDuration = 0.35f;        // Hasar alınca kaç sn donsun

    [Header("Tespit")]
    public float activationRadius = 12f;   // Boss bu menzilde oyuncuyu görünce intro tetiklenir
    public bool autoActivate = true;       // false ise tetikleyici (trigger/cutscene) açar

    [Header("Melee Saldırı")]
    public Transform meleeAttackPoint;
    public float meleeRange = 1.2f;
    public int meleeDamage = 2;
    public LayerMask playerLayer;
    public float meleeCooldown = 3f;
    public float meleeMaxDistance = 2.5f;  // Bu mesafenin altındaysa melee tercih edilir
    public float meleeAttackDuration = 0.8f;  // Animasyon klibinin toplam süresi
    public float meleeHitDelay = 0.3f;        // Animasyonun kaçıncı saniyesinde vuruş gerçekleşsin
    [HideInInspector] public float lastMeleeTime;

    [Header("Ranged Saldırı")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public int projectileCount = 1;        // 1 = tek, >1 = burst
    public float projectileSpread = 0f;    // derece cinsinden saçılma
    public float rangedCooldown = 4f;
    public float rangedAttackDuration = 1.0f;  // Animasyon klibinin toplam süresi
    public float rangedSpawnDelay = 0.4f;      // Animasyonun kaçıncı saniyesinde projectile çıksın
    [HideInInspector] public float lastRangedTime;

    [Header("Temas Hasarı")]
    public int contactDamage = 2;
    public float contactDamageCooldown = 1f;
    public float contactCheckRadius = 0.8f;
    private float _nextContactTime;

    [Header("Loot")]
    public LootTable lootTable;
    [Range(0f, 100f)] public float dropChance = 100f;

    [Header("Sesler (SoundManager isim) — boş bırakılırsa ses çalmaz")]
    public string introSoundName = "";
    public string meleeSoundName = "";
    public string rangedSoundName = "";
    public string footstepSoundName = "";
    public string musicSoundName = "";

    [Header("Genel")]
    public bool isFacingRight = true;
    public bool startWithIntro = true;

    // State'ler
    [HideInInspector] public BossIntroState introState;
    [HideInInspector] public BossIdleState idleState;
    [HideInInspector] public BossChaseState chaseState;
    [HideInInspector] public BossMeleeAttackState meleeState;
    [HideInInspector] public BossRangedAttackState rangedState;
    [HideInInspector] public BossHurtState hurtState;
    [HideInInspector] public BossDeathState deathState;

    [Header("Eventler")]
    public UnityEvent onActivated;            // Boss aktive olduğunda (UI bar göstermek için)

    public bool IsActivated { get; private set; }
    public bool IsInvulnerable { get; set; } // Intro/Death sırasında set edilir

    private void Awake()
    {
        StateMachine = new BossStateMachine();
        rb = GetComponent<Rigidbody2D>();
        bossAnim = GetComponent<BossAnimation>();
        health = GetComponent<BossHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform.root; // child collider tag'lenmiş olabilir, root'u al

        introState = new BossIntroState(this);
        idleState = new BossIdleState(this);
        chaseState = new BossChaseState(this);
        meleeState = new BossMeleeAttackState(this);
        rangedState = new BossRangedAttackState(this);
        hurtState = new BossHurtState(this);
        deathState = new BossDeathState(this);
    }

    private void Start()
    {
        StateMachine.Initialize(idleState);

        if (player == null)
            Debug.LogWarning($"[Boss {name}] Player bulunamadı! Player objesinin Tag'i 'Player' olmalı.", this);
        else
            Debug.Log($"[Boss {name}] Player bulundu: {player.name}", this);
    }

    private void Update()
    {
        if (!IsActivated && autoActivate && player != null && !health.IsDead)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= activationRadius)
            {
                Debug.Log($"[Boss {name}] Player menzilde ({dist:F1} <= {activationRadius}), aktive ediliyor.", this);
                Activate();
            }
        }

        StateMachine.CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState?.FixedUpdate();
    }

    private void LateUpdate()
    {
        if (!health.IsDead && IsActivated && Time.time >= _nextContactTime)
            CheckContactDamage();
    }

    /// <summary>External (trigger/cutscene) veya autoActivate tarafından çağrılır.</summary>
    public void Activate()
    {
        if (IsActivated || health.IsDead) return;
        IsActivated = true;

        SoundManager.Instance?.TryPlay(musicSoundName);

        Debug.Log($"[Boss {name}] Activate çağrıldı. startWithIntro={startWithIntro}", this);

        onActivated?.Invoke();

        if (startWithIntro)
            StateMachine.ChangeState(introState);
        else
            StateMachine.ChangeState(chaseState);
    }

    public void CheckFlip(float moveDirectionX)
    {
        if (moveDirectionX > 0 && !isFacingRight) Flip();
        else if (moveDirectionX < 0 && isFacingRight) Flip();
    }

    public void FaceTarget(Vector3 targetPos)
    {
        float dir = targetPos.x - transform.position.x;
        CheckFlip(dir);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void CheckContactDamage()
    {
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, contactCheckRadius, playerLayer);
        if (playerHit == null) return;

        PlayerHealth playerHealth = playerHit.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.isInvincible)
        {
            playerHealth.TakeDamage(contactDamage);
            _nextContactTime = Time.time + contactDamageCooldown;
        }
    }

    // ─── Animation Event Hook'ları ──────────────────────────────

    /// <summary>Melee animasyonunun vuruş frame'inde çağrılır.</summary>
    public void AnimEvent_MeleeHit()
    {
        if (health.IsDead || meleeAttackPoint == null) return;

        SoundManager.Instance?.TryPlayOneShot(meleeSoundName);

        Collider2D hit = Physics2D.OverlapCircle(meleeAttackPoint.position, meleeRange, playerLayer);
        if (hit != null)
        {
            PlayerHealth ph = hit.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(meleeDamage);
        }
    }

    /// <summary>Ranged animasyonunun spawn frame'inde çağrılır.</summary>
    public void AnimEvent_SpawnProjectile()
    {
        if (health.IsDead || projectilePrefab == null || projectileSpawnPoint == null) return;

        SoundManager.Instance?.TryPlayOneShot(rangedSoundName);

        Vector2 baseDir = isFacingRight ? Vector2.right : Vector2.left;

        int count = Mathf.Max(1, projectileCount);
        float totalSpread = projectileSpread;
        float step = (count > 1) ? totalSpread / (count - 1) : 0f;
        float startAngle = -totalSpread * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

            GameObject p = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            if (p.TryGetComponent(out BossProjectile bp))
                bp.Setup(dir);
            else if (p.TryGetComponent(out EnemyProjectile ep))
                ep.Setup(dir); // Mevcut EnemyProjectile prefab'larını da kabul et
        }
    }

    /// <summary>Animasyonun sonunda çağrılır — chase'e döner.</summary>
    // Artık state'ler kendi süresini takip ediyor — bu event no-op (geriye uyumluluk için duruyor)
    public void AnimEvent_AttackComplete() { }

    /// <summary>Intro animasyonu bittiğinde çağrılır.</summary>
    public void AnimEvent_IntroComplete()
    {
        if (StateMachine.CurrentState == introState)
            StateMachine.ChangeState(chaseState);
    }

    /// <summary>Death animasyonu bittiğinde çağrılır — opsiyonel destroy.</summary>
    public void AnimEvent_DeathComplete()
    {
        // İsteğe bağlı: Destroy(gameObject) veya disable
        gameObject.SetActive(false);
    }

    /// <summary>Yürüme adım sesi için animasyon event.</summary>
    public void AnimEvent_Footstep()
    {
        SoundManager.Instance?.TryPlayOneShot(footstepSoundName);
    }

    public void DropLoot()
    {
        if (lootTable == null) return;
        if (Random.Range(0f, 100f) > dropChance) return;

        GameObject item = lootTable.GetRandomLoot();
        if (item != null) Instantiate(item, transform.position, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, contactCheckRadius);

        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeRange);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, meleeMaxDistance);
    }
}
