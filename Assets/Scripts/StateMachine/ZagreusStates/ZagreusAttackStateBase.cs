using UnityEngine;

/// <summary>
/// Tüm Zagreus saldırı state'lerinin paylaştığı abstract taban.
/// Subclass sadece veriyi (data) ve animasyon çağrısını override eder.
/// </summary>
public abstract class ZagreusAttackStateBase : IBossState
{
    protected readonly ZagreusController boss;
    protected float startTime;
    protected bool hitFired;

    protected ZagreusAttackStateBase(ZagreusController boss) { this.boss = boss; }

    protected abstract ZagreusAttackData Data { get; }
    protected abstract void PlayAnimation();
    protected abstract void PlaySound();

    public virtual void Enter()
    {
        startTime = Time.time;
        hitFired = false;

        if (boss.rb != null) boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        boss.zagAnim?.SetWalk(false);

        if (boss.player != null) boss.FaceTarget(boss.player.position);

        if (boss.zagAnim != null) PlayAnimation();
        PlaySound();
    }

    public virtual void Update()
    {
        float elapsed = Time.time - startTime;

        if (!hitFired && elapsed >= Data.hitDelay)
        {
            hitFired = true;
            DoHit();
        }

        if (elapsed >= Data.duration)
        {
            boss.OnAttackCompleted();
        }
    }

    protected virtual void DoHit()
    {
        Vector2 facingMul = new Vector2(boss.isFacingRight ? 1f : -1f, 1f);
        Vector2 origin = (Vector2)boss.transform.position + Vector2.Scale(Data.hitOffset, facingMul);

        Collider2D hit = Physics2D.OverlapCircle(origin, Data.hitRange, boss.playerLayer);
        if (hit == null) return;

        PlayerHealth ph = hit.GetComponentInParent<PlayerHealth>();
        if (ph != null && !ph.isInvincible) ph.TakeDamage(Data.damage);
    }

    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
