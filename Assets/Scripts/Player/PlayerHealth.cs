using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
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

            // 2. Mevcut canı direkt maksimuma eşitle
            currentHealth = maxHealth;

            // 3. YENİ EKLENEN/DEĞİŞEN KISIM: Kalkanı datadan okuma, her sahnede sıfırla!
            currentShield = 0;

            // 4. Veriyi hemen güncelle ki UI ve diğer sistemler dolu canı ve sıfırlanmış kalkanı görsün
            UpdateData();
        }
        else
        {
            Debug.LogWarning("PlayerData atanmadı! Inspector üzerinden dosyayı sürüklemeyi unutmayın.");
            currentHealth = maxHealth;
            currentShield = 0; // Eğer data yoksa da kalkanı sıfırla
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
    // ID daha önce alınmış mı diye kontrol eder
    public bool HasCollectedUpgrade(string id)
    {
        if (data != null)
        {
            return data.collectedHealthUpgrades.Contains(id);
        }
        return false;
    }

    // ID'yi alınmışlar listesine ekler
    public void RecordUpgrade(string id)
    {
        if (data != null && !data.collectedHealthUpgrades.Contains(id))
        {
            data.collectedHealthUpgrades.Add(id);
        }
    }

    public void InstantKill()
    {
        if (_isDead) return;

        // Kalkanı ve canı sıfırla ki UI ekranında da boş görünsün
        currentShield = 0;
        currentHealth = 0;
        UpdateData();

        // Direkt ölümü çağır (Blok veya I-Frame umursamaz)
        Die();
    }

}