using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class BossHealth : MonoBehaviour
{
    [Header("Sağlık Ayarları")]
    public int maxHealth = 200;
    private int currentHealth;

    [Header("Sesler (boş bırakılırsa ses çalmaz)")]
    public string hurtSoundName = "";
    public string deathSoundName = "";

    [Header("Eventler (UI / Cutscene)")]
    public UnityEvent<int, int> onHealthChanged; // (current, max)
    public UnityEvent onBossDied;

    private BossController _controller;
    private bool _isDead = false;

    public bool IsDead => _isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        _controller = GetComponent<BossController>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage, Vector2? sourcePos = null)
    {
        if (_isDead || _controller == null) return;
        if (_controller.IsInvulnerable) return;

        currentHealth -= damage;

        VFXManager.Instance?.PlayHitSpark(transform.position);
        SoundManager.Instance?.TryPlayOneShot(hurtSoundName);

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(sourcePos);
        }
        else
        {
            // Hafif knockback (boss ağır, az itilir)
            if (sourcePos.HasValue)
                _controller.ApplyKnockback(sourcePos.Value, _controller.hurtKnockbackMultiplier);

            // Scale punch
            transform.DOKill(true);
            transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.18f, 6, 0.5f);

            _controller.StateMachine.ChangeState(_controller.hurtState);
        }
    }

    private void Die(Vector2? sourcePos = null)
    {
        if (_isDead) return;
        _isDead = true;

        SoundManager.Instance?.TryPlayOneShot(deathSoundName);

        if (sourcePos.HasValue)
            _controller.ApplyKnockback(sourcePos.Value, _controller.deathKnockbackMultiplier);

        // Boss ölüm punch — daha dramatik
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.4f, 4, 0.7f);

        _controller.DropLoot();
        _controller.StateMachine.ChangeState(_controller.deathState);

        onBossDied?.Invoke();
    }
}
