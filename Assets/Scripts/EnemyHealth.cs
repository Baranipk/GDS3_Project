using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Sağlık Ayarları")]
    public int maxHealth = 30;
    private int currentHealth;

    private EnemyController _controller;
    private bool _isDead = false;

    // KRİTİK SATIR: Dışarıdaki scriptlerin (HurtState gibi) ölümü kontrol etmesini sağlar
    public bool IsDead => _isDead;

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} hasar aldı! Kalan Can: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            // Ölmediyse sarsılma (Hurt) durumuna geç
            if (_controller is BatController bat)
            {
                bat.StateMachine.ChangeState(bat.hurtState);
            }
            else if (_controller is SkeletonController skeleton)
            {
                skeleton.StateMachine.ChangeState(skeleton.hurtState);
            }
            else if (_controller is SatyrController satyr) // BUNU EKLEDİK
            {
                satyr.StateMachine.ChangeState(satyr.hurtState);
            }
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log($"{gameObject.name} öldü!");

        if (_controller is BatController bat)
        {
            bat.StateMachine.ChangeState(bat.deathState);
        }
        else if (_controller is SkeletonController skeleton)
        {
            skeleton.StateMachine.ChangeState(skeleton.deathState);
        }
        else if (_controller is SatyrController satyr) // BUNU EKLEDİK
        {
            satyr.StateMachine.ChangeState(satyr.deathState);
        }
    }
}