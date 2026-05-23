using UnityEngine;
using DG.Tweening;

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

    public void TakeDamage(int damage, Vector2? sourcePos = null)
    {
        if (_isDead) return;

        currentHealth -= damage;

        VFXManager.Instance?.PlayHitSpark(transform.position);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(sourcePos);
        }
        else
        {
            SoundManager.Instance?.TryPlayOneShot("EnemyHurt");

            // Knockback uygula (hafif)
            if (_controller != null && sourcePos.HasValue)
                _controller.ApplyKnockback(sourcePos.Value, _controller.hurtKnockbackMultiplier);

            // Küçük scale punch — vuruşun ağırlık hissi
            transform.DOKill(true);
            transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.18f, 8, 0.6f);

            if (_controller is BatController bat)
                bat.StateMachine.ChangeState(bat.hurtState);
            else if (_controller is SkeletonController skeleton)
                skeleton.StateMachine.ChangeState(skeleton.hurtState);
            else if (_controller is SatyrController satyr)
                satyr.StateMachine.ChangeState(satyr.hurtState);
        }
    }

    private void Die(Vector2? sourcePos = null)
    {
        if (_isDead) return;
        _isDead = true;

        SoundManager.Instance?.TryPlayOneShot("EnemyDeath");

        // Ölüm knockback'i — çok daha güçlü, yukarı doğru fırlatma
        if (_controller != null && sourcePos.HasValue)
            _controller.ApplyKnockback(sourcePos.Value, _controller.deathKnockbackMultiplier);

        // Büyük scale punch — ölüm dramatize
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 0.3f, 6, 0.7f);

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