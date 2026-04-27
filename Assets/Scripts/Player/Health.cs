using UnityEngine;
using System;

public class Health : MonoBehaviour
{
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

    private void Start()
    {
        _controller = GetComponent<PlayerController>();

        if (data != null)
        {
            // 1. Maksimum canı veri dosyasından oku
            maxHealth = data.maxHealth;

            // 2. KRİTİK ADIM: Mevcut canı datadan okumak yerine direkt maksimuma eşitle
            currentHealth = maxHealth;

            // Kalkanı istersen datadan (kaldığı yerden) yüklemeye devam edebilirsin
            currentShield = data.currentShield;

            // 3. Veriyi hemen güncelle ki UI ve diğer sistemler dolu canı görsün
            UpdateData();
        }
        else
        {
            Debug.LogWarning("PlayerData atanmadı! Inspector üzerinden dosyayı sürüklemeyi unutmayın.");
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDead || isInvincible) return;

        // --- BLOK KONTROLÜ ---
        if (_controller.playerStateMachine.CurrentState == _controller.blockState)
        {
            damage = 0;

            // YENİ: Blok yaparken darbe alınca BlockHit sesini çal
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

            // İSTEĞE BAĞLI: Kalkan darbe emdiğinde bir ses çalmak istersen:
            // SoundManager.Instance?.Get("ShieldHit")?.PlayOneShot();

            if (damage <= 0) return;
        }

        // 1. Kalan hasarı candan düşür
        currentHealth -= damage;
        UpdateData();

        // 2. ÖLÜM KONTROLÜ
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateData();
            Die();
        }
        else
        {
            // 3. Hasar alma state'i (Hurt sesi zaten bu state'in Enter'ında çalıyor)
            _controller.playerStateMachine.ChangeState(_controller.hurtState);
        }
    }

    public void AddShield(int amount)
    {
        if (_isDead) return;
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        UpdateData();

        // İSTEĞE BAĞLI: Kalkan kazanma sesi
        // SoundManager.Instance?.Get("ShieldUp")?.PlayOneShot();
    }

    private void Die()
    {
        _isDead = true;
        _controller.playerStateMachine.ChangeState(_controller.deathState);
    }

    public void Heal(int amount)
    {
        if (_isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateData();

        // İSTEĞE BAĞLI: Canlanma/İksir içme sesi
        // SoundManager.Instance?.Get("Heal")?.PlayOneShot();
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