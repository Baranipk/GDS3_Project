using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    // ── Statik Referans & Eventler ─────────────────────────────
    public static PlayerHealth Instance { get; private set; }

    /// PlayerHealth.Start() bitince tetiklenir — HealthUI ilk kurulumu için
    public static event Action<PlayerHealth> OnPlayerReady;

    /// Her can/kalkan değişiminde tetiklenir — HealthUI güncellemesi için
    /// Update() polling yerine bu event kullanılır → güvenilir ve anlık
    public static event Action<PlayerHealth> OnHealthChanged;

    [Header("Can Ayarları")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Kalkan Ayarları")]
    public int currentShield = 0;
    public int maxShield = 3;

    [Header("I-Frame Durumu")]
    public bool isInvincible = false;

    [Header("Veri Kaydı")]
    public PlayerData data;

    private PlayerController _controller;
    private bool _isDead = false;

    private bool _hasInitialized = false;

    // ─────────────────────────────────────────────────────────── 
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnEnable()
    {
        // Eğer obje Start() ile daha önceden kurulduysa (örn. ana menüden oyuna dönüldüyse)
        // UI'ın kendini tekrar kurabilmesi için hazır olduğumuzu bildiriyoruz.
        if (_hasInitialized)
        {
            OnPlayerReady?.Invoke(this);
        }
    }

    private void Start()
    {
        _controller = GetComponent<PlayerController>();

        if (data != null)
        {
            maxHealth = data.maxHealth;
            currentHealth = maxHealth;
            currentShield = 0;
            UpdateData();
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] PlayerData atanmamış!");
            currentHealth = maxHealth;
            currentShield = 0;
        }

        _hasInitialized = true;

        // Veriler yüklendi — UI'ı kur
        OnPlayerReady?.Invoke(this);
    }

    // ─────────────────────────────────────────────────────────── 
    public void TakeDamage(int damage)
    {
        if (_isDead || isInvincible) return;

        if (_controller.playerStateMachine.CurrentState == _controller.blockState)
        {
            SoundManager.Instance?.Get("BlockHit")?.PlayOneShot();
            _controller.ApplyKnockback(_controller.blockKnockbackMultiplier);
            return;
        }

        if (currentShield > 0)
        {
            int damageToShield = Mathf.Min(currentShield, damage);
            currentShield -= damageToShield;
            damage -= damageToShield;
            UpdateData();

            if (damageToShield > 0)
                VFXManager.Instance?.PlayDamage(transform.position, damageToShield, "Shield");

            // Kalkan kırıldıysa özel ses
            if (currentShield == 0)
                SoundManager.Instance?.TryPlayOneShot("ShieldBreak");

            // Kalkan değişti — UI'ı bildir
            OnHealthChanged?.Invoke(this);

            if (damage <= 0) return;
        }

        currentHealth -= damage;
        UpdateData();

        VFXManager.Instance?.PlayDamage(transform.position, damage);

        // Can değişti — UI'ı bildir
        OnHealthChanged?.Invoke(this);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateData();
            Die();
        }
        else
        {
            _controller.playerStateMachine.ChangeState(_controller.hurtState);
        }
    }

    public void AddShield(int amount)
    {
        if (_isDead) return;
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        UpdateData();

        VFXManager.Instance?.PlayShield(transform.position, amount);
        SoundManager.Instance?.TryPlayOneShot("PlayerShieldUp");
        OnHealthChanged?.Invoke(this);
    }

    public void Heal(int amount)
    {
        if (_isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateData();

        VFXManager.Instance?.PlayHeal(transform.position, amount);
        SoundManager.Instance?.TryPlayOneShot("PlayerHeal");
        OnHealthChanged?.Invoke(this);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        currentShield = 0;
        _isDead = false;
        isInvincible = false;
        UpdateData();

        // Respawn sonrası UI'ı zorla güncelle
        OnHealthChanged?.Invoke(this);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateData();

        SoundManager.Instance?.TryPlayOneShot("PlayerMaxHpUp");
        OnHealthChanged?.Invoke(this);
    }

    public void InstantKill()
    {
        if (_isDead) return;
        currentShield = 0;
        currentHealth = 0;
        UpdateData();
        OnHealthChanged?.Invoke(this);
        Die();
    }

    public bool HasCollectedUpgrade(string id)
    {
        if (data != null) return data.collectedHealthUpgrades.Contains(id);
        return false;
    }

    public void RecordUpgrade(string id)
    {
        if (data != null && !data.collectedHealthUpgrades.Contains(id))
            data.collectedHealthUpgrades.Add(id);
    }

    private void Die()
    {
        _isDead = true;
        // Ölüm anında güçlü geri tepme — dramatize
        _controller.ApplyKnockback(_controller.deathKnockbackMultiplier);
        _controller.playerStateMachine.ChangeState(_controller.deathState);
    }

    private void UpdateData()
    {
        if (data != null)
        {
            data.maxHealth = maxHealth;
            data.currentHealth = currentHealth;
            data.currentShield = currentShield;
        }
    }
}