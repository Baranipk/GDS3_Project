using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Kalkan Ayarları")]
    public int currentShield = 0; // Mevcut kalkan miktarı
    public int maxShield = 3;     // Oyuncunun en fazla biriktirebileceği kalkan sayısı

    [Header("I-Frame Durumu")]
    public bool isInvincible = false;

    private PlayerController _controller;
    private bool _isDead = false;

    private void Start()
    {
        _controller = GetComponent<PlayerController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead || isInvincible) return;

        // BLOK KONTROLÜ
        if (_controller.playerStateMachine.CurrentState == _controller.blockState)
        {
            damage = 0;
            _controller.ApplyKnockback(_controller.blockKnockbackMultiplier);
            return;
        }

        // --- YENİ: KALKAN KONTROLÜ ---
        if (currentShield > 0)
        {
            // Kalkanın emebileceği hasarı hesapla
            int damageToShield = Mathf.Min(currentShield, damage);

            currentShield -= damageToShield; // Kalkanı düşür
            damage -= damageToShield;        // Kalan hasarı hesapla

            Debug.Log($"Kalkan hasarı emdi! Kalan Kalkan: {currentShield}");

            // Eğer kalkan tüm hasarı emdiyse ve geriye hasar kalmadıysa, cana dokunmadan çık
            if (damage <= 0) return;
        }

        // 1. Kalan hasarı candan düşür
        currentHealth -= damage;

        // 2. ÖLÜM KONTROLÜ
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            // 3. Eğer ölmediyse Hasar Alma State'ine geç
            _controller.playerStateMachine.ChangeState(_controller.hurtState);
        }
    }

    // --- YENİ: KALKAN EKLEME METODU ---
    public void AddShield(int amount)
    {
        if (_isDead) return;
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        Debug.Log($"Kalkan alındı! Mevcut Kalkan: {currentShield}");
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
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        currentShield = 0; // Doğduğunda kalkanlar sıfırlansın
        _isDead = false;
        isInvincible = false;
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // Yeni eklenen kalbi dolu olarak veriyoruz
                                 // HealthUI scriptimiz Update içinde bu değişikliği fark edip kendini yenileyecek
    }
}