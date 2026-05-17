using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    // ── Statik Referans & Event ────────────────────────────────
    public static PlayerHealth Instance { get; private set; }

    /// <summary>
    /// PlayerHealth.Start() tamamlandığında, veriler yüklendikten SONRA tetiklenir.
    /// HealthUI bu event'e subscribe olarak timing sorununu tamamen çözer.
    /// OnSceneLoaded → Start()'tan önce gelir; bu event Start()'tan sonra gelir.
    /// </summary>
    public static event Action<PlayerHealth> OnPlayerReady;

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

    // ─────────────────────────────────────────────────────────── 
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
            Debug.LogWarning("[PlayerHealth] PlayerData atanmamış! Inspector üzerinden sürüklemeyi unutmayın.");
            currentHealth = maxHealth;
            currentShield = 0;
        }

        // Veriler yüklendi, UI'a hazır olduğumuzu bildir.
        // OnSceneLoaded'dan SONRA geldiği için UI doğru değerleri alır.
        OnPlayerReady?.Invoke(this);
    }

    // ─────────────────────────────────────────────────────────── 
    public void TakeDamage(int damage)
    {
        if (_isDead || isInvincible) return;

        // --- BLOK KONTROLÜ ---
        if (_controller.playerStateMachine.CurrentState == _controller.blockState)
        {
            damage = 0;
            SoundManager.Instance?.Get("BlockHit")?.PlayOneShot();
            _controller.ApplyKnockback(_controller.blockKnockbackMultiplier);
            return;
        }

        // --- KALKAN KONTROLÜ ---
        if (currentShield > 0)
        {
            int damageToShield = Mathf.Min(currentShield, damage);
            currentShield -= damageToShield;
            damage -= damageToShield;
            UpdateData();

            if (damage <= 0) return;
        }

        currentHealth -= damage;
        UpdateData();

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
    }

    public void Heal(int amount)
    {
        if (_isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateData();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        currentShield = 0;
        _isDead = false;
        isInvincible = false;
        UpdateData();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateData();
    }

    public void InstantKill()
    {
        if (_isDead) return;
        currentShield = 0;
        currentHealth = 0;
        UpdateData();
        Die();
    }

    public bool HasCollectedUpgrade(string id)
    {
        if (data != null)
            return data.collectedHealthUpgrades.Contains(id);
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