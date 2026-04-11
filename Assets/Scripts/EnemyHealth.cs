using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Sağlık Ayarları")]
    public int maxHealth = 30;
    private int currentHealth;

    private EnemyController _controller;
    private bool _isDead = false;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
        currentHealth = maxHealth;
    }

    // EnemyHealth.cs içindeki TakeDamage metodunu güncelle:
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        currentHealth -= damage;

        // Eğer düşman bir Bat (Yarasa) ise onu HurtState'e sok
        if (_controller is BatController bat)
        {
            bat.StateMachine.ChangeState(bat.hurtState);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        // Düşmanı ölüm state'ine sokuyoruz
        // BatController içindeki deathState'e erişeceğiz
        if (_controller is BatController bat)
        {
            bat.StateMachine.ChangeState(bat.deathState);
        }
        // Not: İleride diğer düşmanlar için burayı genişleteceğiz
    }
}