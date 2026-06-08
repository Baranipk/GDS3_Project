using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// Zagreus HP — BossHealth ile aynı mantık ama ZagreusController'a bağlı.
/// </summary>
public class ZagreusHealth : MonoBehaviour
{
    [Header("Sağlık")]
    public int maxHealth = 200;
    private int currentHealth;

    [Header("Sesler (boş = sessiz)")]
    public string hurtSoundName = "";
    public string deathSoundName = "";

    [Header("Screen Shake Şiddetleri")]
    public bool enableScreenShake = true;
    [Range(0f, 2f)] public float shakeOnHurt = 0.4f;
    [Range(0f, 2f)] public float shakeOnDeath = 1.2f;

    [Header("Eventler")]
    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onBossDied;

    private ZagreusController _controller;
    private bool _isDead;

    public bool IsDead => _isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        _controller = GetComponent<ZagreusController>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage, Vector2? sourcePos = null)
    {
        if (_isDead || _controller == null || _controller.IsInvulnerable) return;

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
            if (sourcePos.HasValue)
                _controller.ApplyKnockback(sourcePos.Value, _controller.hurtKnockbackMultiplier);

            transform.DOKill(true);
            transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.18f, 6, 0.5f);
            if (enableScreenShake) ScreenShake.Instance?.Shake(shakeOnHurt);

            // Combo zincirini kes, hurt state'e geç
            _controller.ClearPlan();
            _controller.StateMachine.ChangeState(_controller.hurtState);
        }
    }

    private void Die(Vector2? sourcePos)
    {
        if (_isDead) return;
        _isDead = true;

        SoundManager.Instance?.TryPlayOneShot(deathSoundName);

        if (sourcePos.HasValue)
            _controller.ApplyKnockback(sourcePos.Value, _controller.deathKnockbackMultiplier);

        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.4f, 4, 0.7f);
        if (enableScreenShake) ScreenShake.Instance?.Shake(shakeOnDeath);

        _controller.DropLoot();
        _controller.ClearPlan();
        _controller.StateMachine.ChangeState(_controller.deathState);

        onBossDied?.Invoke();
    }
}
