using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxHealth = 5;
    public int currentHealth;

    private PlayerController _controller;
    private bool _isDead = false;

    private void Start()
    {
        _controller = GetComponent<PlayerController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return; // Zaten ölüyse hasar almasın

        // BLOK KONTROLÜ (Opsiyonel ama çok iyi olur)
        if (_controller.playerStateMachine.CurrentState == _controller.blockState)
        {
            damage = 0; // Blok yapıyorsa hasarı yarıya indir
            Debug.Log("Bloklandığı için hasar alınmadı!");
        }

        currentHealth -= damage;
        Debug.Log($"Hasar alındı! Kalan Can: {currentHealth}");

        // Hasar efekti veya sesini burada tetikleyebilirsin

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        _controller.playerStateMachine.ChangeState(_controller.deathState);
    }

    // Can iksiri vb. için iyileşme metodu
    public void Heal(int amount)
    {
        if (_isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        _isDead = false;
    }


}