using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("I-Frame Durumu")]
    public bool isInvincible = false; // Hasar alınamazlık kontrolü

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
            Debug.Log("Bloklandı!");
            return;
        }

        // 1. Canı düşür
        currentHealth -= damage;

        // 2. ÖLÜM KONTROLÜ (BURASI EKSİKTİ)
        if (currentHealth <= 0)
        {
            currentHealth = 0; // Canın eksiye düşmesini engelle
            Die();             // Ölüm metodunu çağır
        }
        else
        {
            // 3. Eğer ölmediyse Hasar Alma State'ine geç
            _controller.playerStateMachine.ChangeState(_controller.hurtState);
        }
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
        _isDead = false;
        isInvincible = false; // Resetlerken hasar alınamazlığı da kapat
    }
}