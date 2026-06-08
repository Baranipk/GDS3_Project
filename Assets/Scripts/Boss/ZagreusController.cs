using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Zagreus boss — plan tabanlı combo zincir motoru.
///
/// Aksiyonlar:
///   Solo1, Solo2, Solo3 (tekil saldırılar)
///   Chain A: Attack1 → Attack2_v1
///   Chain B: Attack2_v2 → Attack3_v1
///   Chain C: Attack1 → Attack2_v2 → Attack3_v1
///   BackDash (defansif + post-combo + random)
/// </summary>
public class ZagreusController : MonoBehaviour
{
    public BossStateMachine StateMachine { get; private set; }

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public ZagreusAnimation zagAnim;
    [HideInInspector] public ZagreusHealth health;
    [HideInInspector] public Transform player;

    // ── Hareket ─────────────────────────────────────────────
    [Header("Hareket")]
    public float moveSpeed = 3f;
    public float backstepSpeed = 2f;
    public float stopDistance = 1.5f;

    [Header("Taktik Mesafe")]
    public float preferredRange = 4f;
    public float rangeBuffer = 1.0f;
    public float tooCloseDistance = 1.8f;

    [Header("Tespit")]
    public float activationRadius = 12f;
    public bool autoActivate = true;
    public bool startActive = false;

    [Header("Temas Hasarı")]
    public LayerMask playerLayer;
    public int contactDamage = 2;
    public float contactDamageCooldown = 1f;
    public float contactCheckRadius = 0.8f;
    private float _nextContactTime;

    [Header("Knockback")]
    public Vector2 knockbackForce = new Vector2(3f, 2f);
    public float hurtKnockbackMultiplier = 0.3f;
    public float deathKnockbackMultiplier = 1f;

    [Header("Loot")]
    public LootTable lootTable;
    [Range(0f, 100f)] public float dropChance = 100f;

    // ── Saldırı verileri ───────────────────────────────────
    [Header("Solo Saldırı Verileri")]
    public ZagreusAttackData attack1Data = new ZagreusAttackData { duration = 0.7f, hitDelay = 0.3f, damage = 2, hitRange = 1.5f };
    public ZagreusAttackData attack2Data = new ZagreusAttackData { duration = 0.7f, hitDelay = 0.3f, damage = 2, hitRange = 1.5f };
    public ZagreusAttackData attack3Data = new ZagreusAttackData { duration = 0.8f, hitDelay = 0.4f, damage = 3, hitRange = 1.8f };

    [Header("Chain Combo (Attack1+Attack2 birleşik animasyon)")]
    public ZagreusChainComboData chainComboData = new ZagreusChainComboData();

    // ── Combo karar tablosu ─────────────────────────────────
    [Header("Combo Karar Tablosu")]
    [Tooltip("Chase tick'inde sırayla denenir. İlk uyan tetiklenir.")]
    public List<ZagreusComboEntry> comboTable = new List<ZagreusComboEntry>
    {
        new ZagreusComboEntry { combo = ZagreusComboType.Chain,   chance = 0.35f, minDistance = 0f,   maxDistance = 2.5f },
        new ZagreusComboEntry { combo = ZagreusComboType.Solo1,   chance = 0.20f, minDistance = 0f,   maxDistance = 2.5f },
        new ZagreusComboEntry { combo = ZagreusComboType.Solo2,   chance = 0.20f, minDistance = 2f,   maxDistance = 5f },
        new ZagreusComboEntry { combo = ZagreusComboType.Solo3,   chance = 0.25f, minDistance = 3f,   maxDistance = 8f },
    };

    [Header("Combo Cooldown")]
    [Tooltip("Combo bittikten sonra yeniden saldırıya başlamadan önce minimum bekleme")]
    public float postComboRest = 0.6f;
    public float patternThinkInterval = 0.25f;
    [HideInInspector] public float lastComboEndTime;

    // ── BackDash ────────────────────────────────────────────
    [Header("BackDash")]
    public float backDashDuration = 0.35f;
    public float backDashSpeed = 9f;
    [Range(0f, 1f)] public float backDashChance_TooClose = 0.5f;  // melee cooldown'da & çok yakın
    [Range(0f, 1f)] public float backDashChance_PostCombo = 0.3f; // combo biter bitmez
    [Range(0f, 1f)] public float backDashChance_Random = 0.05f;   // chase'te random tick

    // ── Hurt ────────────────────────────────────────────────
    [Header("Hurt")]
    public float hurtDuration = 0.3f;

    // ── Sesler ──────────────────────────────────────────────
    [Header("Sesler (boş = sessiz)")]
    public string activateSoundName = "";
    public string attack1SoundName = "";
    public string attack2SoundName = "";
    public string attack3SoundName = "";
    public string chainComboSoundName = "";        // İlk vuruşta çalar
    public string chainCombo2ndHitSoundName = "";  // İkinci vuruşta çalar
    public string backDashSoundName = "";
    public string musicSoundName = "";
    public string footstepSoundName = "";          // Yürürken her adımda çalar

    [Header("Footstep Ayarları")]
    [Tooltip("Adımlar arası süre (saniye). Yürüme hızıyla uyumlu olmalı.")]
    public float footstepInterval = 0.4f;
    [Tooltip("Pitch varyasyonu (her adımda doğal hissiyat için)")]
    public Vector2 footstepPitchRange = new Vector2(0.85f, 1.15f);
    [Tooltip("Volume varyasyonu")]
    public Vector2 footstepVolumeRange = new Vector2(0.8f, 1.0f);

    [Header("Genel")]
    public bool isFacingRight = true;

    [Header("Eventler")]
    public UnityEvent onActivated;

    // ── State referansları ──────────────────────────────────
    [HideInInspector] public ZagreusIdleState idleState;
    [HideInInspector] public ZagreusChaseState chaseState;
    [HideInInspector] public ZagreusHurtState hurtState;
    [HideInInspector] public ZagreusDeathState deathState;
    [HideInInspector] public ZagreusBackDashState backDashState;
    [HideInInspector] public ZagreusAttack1State attack1State;
    [HideInInspector] public ZagreusAttack2State attack2State;
    [HideInInspector] public ZagreusAttack3State attack3State;
    [HideInInspector] public ZagreusChainComboState chainComboState;

    public bool IsActivated { get; private set; }
    public bool IsInvulnerable { get; set; }

    private readonly Queue<IBossState> _currentPlan = new Queue<IBossState>();

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        StateMachine = new BossStateMachine();
        rb = GetComponent<Rigidbody2D>();
        zagAnim = GetComponent<ZagreusAnimation>();
        health = GetComponent<ZagreusHealth>();

        if (zagAnim == null)
            Debug.LogError($"[Zagreus] ZagreusAnimation component EKSİK! '{name}' objesine 'Add Component → Zagreus Animation' ekle.", this);
        if (health == null)
            Debug.LogError($"[Zagreus] ZagreusHealth component EKSİK! '{name}' objesine 'Add Component → Zagreus Health' ekle.", this);
        if (rb == null)
            Debug.LogError($"[Zagreus] Rigidbody2D EKSİK! '{name}' objesine ekle.", this);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform.root;

        idleState        = new ZagreusIdleState(this);
        chaseState       = new ZagreusChaseState(this);
        hurtState        = new ZagreusHurtState(this);
        deathState       = new ZagreusDeathState(this);
        backDashState    = new ZagreusBackDashState(this);
        attack1State     = new ZagreusAttack1State(this);
        attack2State     = new ZagreusAttack2State(this);
        attack3State     = new ZagreusAttack3State(this);
        chainComboState  = new ZagreusChainComboState(this);
    }

    private void Start()
    {
        StateMachine.Initialize(idleState);
        if (startActive) Activate();
    }

    private void Update()
    {
        if (!IsActivated && autoActivate && player != null && health != null && !health.IsDead)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= activationRadius) Activate();
        }
        StateMachine.CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState?.FixedUpdate();
    }

    private void LateUpdate()
    {
        if (health == null || health.IsDead || !IsActivated) return;
        if (Time.time >= _nextContactTime) CheckContactDamage();
    }

    public void Activate()
    {
        if (IsActivated || (health != null && health.IsDead)) return;
        IsActivated = true;

        SoundManager.Instance?.TryPlayOneShot(activateSoundName);
        SoundManager.Instance?.PlayMusic(musicSoundName);  // Theme fade-out, boss music fade-in
        onActivated?.Invoke();

        StateMachine.ChangeState(chaseState);
    }

    // ── Combo Plan Motoru ───────────────────────────────────

    /// <summary>Chase'ten saldırı zinciri başlatır.</summary>
    public void StartCombo(ZagreusComboType type)
    {
        _currentPlan.Clear();
        switch (type)
        {
            case ZagreusComboType.Solo1: _currentPlan.Enqueue(attack1State); break;
            case ZagreusComboType.Solo2: _currentPlan.Enqueue(attack2State); break;
            case ZagreusComboType.Solo3: _currentPlan.Enqueue(attack3State); break;
            case ZagreusComboType.Chain: _currentPlan.Enqueue(chainComboState); break; // tek state, içinde 2 hit
        }
        AdvancePlan();
    }

    /// <summary>Bir saldırı state'i tamamlanınca çağrılır. Sıradakini çalıştırır veya Chase'e döner.</summary>
    public void OnAttackCompleted() => AdvancePlan();

    private void AdvancePlan()
    {
        if (health != null && health.IsDead) return;

        if (_currentPlan.Count > 0)
        {
            var next = _currentPlan.Dequeue();
            StateMachine.ChangeState(next);
        }
        else
        {
            lastComboEndTime = Time.time;
            StateMachine.ChangeState(chaseState);
        }
    }

    /// <summary>Hurt/Death çağırır: planı temizle, zincir kesilsin.</summary>
    public void ClearPlan() => _currentPlan.Clear();

    public bool HasPlanQueued => _currentPlan.Count > 0;

    // ── Animation Event Yönlendiricileri (opsiyonel) ────────
    // Animator clip'lerinden frame'e event ekleyince çağrılır.

    public void AnimEvent_ChainHit1()
    {
        if (StateMachine.CurrentState is ZagreusChainComboState chain) chain.PerformHit1();
    }

    public void AnimEvent_ChainHit2()
    {
        if (StateMachine.CurrentState is ZagreusChainComboState chain) chain.PerformHit2();
    }

    // ── Yardımcılar ─────────────────────────────────────────

    public void FaceTarget(Vector3 targetPos)
    {
        float dir = targetPos.x - transform.position.x;
        if (dir > 0 && !isFacingRight) Flip();
        else if (dir < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void CheckContactDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, contactCheckRadius, playerLayer);
        if (hit == null) return;
        PlayerHealth ph = hit.GetComponentInParent<PlayerHealth>();
        if (ph != null && !ph.isInvincible)
        {
            ph.TakeDamage(contactDamage);
            _nextContactTime = Time.time + contactDamageCooldown;
        }
    }

    /// <summary>Adım sesi çalar — pitch ve volume varyasyonuyla doğal hissiyat.</summary>
    public void PlayFootstep()
    {
        if (string.IsNullOrEmpty(footstepSoundName)) return;

        Sound s = SoundManager.Instance?.TryGet(footstepSoundName);
        if (s == null) return;

        float pitch  = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
        float volume = Random.Range(footstepVolumeRange.x, footstepVolumeRange.y);

        s.SetPitch(pitch).SetVolume(volume).PlayOneShot();
    }

    public void ApplyKnockback(Vector2 sourcePos, float multiplier)
    {
        if (rb == null) return;
        float dirX = Mathf.Sign(transform.position.x - sourcePos.x);
        if (dirX == 0f) dirX = isFacingRight ? -1f : 1f;
        rb.linearVelocity = Vector2.zero;
        Vector2 force = new Vector2(dirX * knockbackForce.x, knockbackForce.y) * multiplier;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void DropLoot()
    {
        if (lootTable == null) return;
        if (Random.Range(0f, 100f) > dropChance) return;
        GameObject item = lootTable.GetRandomLoot();
        if (item != null) Instantiate(item, transform.position, Quaternion.identity);
    }

    // ── Gizmos ──────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, contactCheckRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tooCloseDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, preferredRange);
    }
}

// ────────────────────────────────────────────────────────────
// Veri tipleri
// ────────────────────────────────────────────────────────────

public enum ZagreusComboType { Solo1, Solo2, Solo3, Chain }

[System.Serializable]
public class ZagreusAttackData
{
    public float duration  = 0.8f;
    public float hitDelay  = 0.3f;
    public int   damage    = 2;
    public float hitRange  = 1.5f;
    public Vector2 hitOffset = new Vector2(1f, 0f);  // boss önünden hangi offset'te hit dairesi
}

[System.Serializable]
public class ZagreusChainComboData
{
    [Tooltip("Tüm chain animasyonunun toplam süresi (saniye)")]
    public float duration = 1.6f;
    [Tooltip("İlk vuruş (Attack1 hit frame'i) zamanı")]
    public float hit1Delay = 0.35f;
    [Tooltip("İkinci vuruş (Attack2 hit frame'i) zamanı")]
    public float hit2Delay = 1.0f;
    public int damage1 = 2;
    public int damage2 = 3;
    public float hitRange = 1.5f;
    public Vector2 hit1Offset = new Vector2(1f, 0f);
    public Vector2 hit2Offset = new Vector2(1.2f, 0f);
}

[System.Serializable]
public class ZagreusComboEntry
{
    public ZagreusComboType combo;
    [Range(0f, 1f)] public float chance = 0.3f;
    public float minDistance = 0f;
    public float maxDistance = 10f;
}
