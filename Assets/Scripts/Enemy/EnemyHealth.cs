using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Sağlık Ayarları")]
    public int maxHealth = 30;
    private int currentHealth;

    private EnemyController _controller;
    private bool _isDead = false;

    public bool IsDead => _isDead;
    public bool malmal = false;
    

    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        currentHealth -= damage;

        // Hit Spark VFX — her hasar alışta düşmanın üzerinde çıkar
        VFXManager.Instance?.PlayHitSpark(transform.position);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            // Hurt state'e geç
            if (_controller is BatController bat)
                bat.StateMachine.ChangeState(bat.hurtState);
            else if (_controller is SkeletonController skeleton)
                skeleton.StateMachine.ChangeState(skeleton.hurtState);
            else if (_controller is SatyrController satyr)
                satyr.StateMachine.ChangeState(satyr.hurtState);
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (_controller != null)
            _controller.DropLoot();

        if (_controller is BatController bat)
            bat.StateMachine.ChangeState(bat.deathState);
        else if (_controller is SkeletonController skeleton)
            skeleton.StateMachine.ChangeState(skeleton.deathState);
        else if (_controller is SatyrController satyr)
            satyr.StateMachine.ChangeState(satyr.deathState);
    }
}