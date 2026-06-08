using UnityEngine;

/// <summary>
/// Tek bir animasyon içinde Attack1 + Attack2'yi birleşik oynatır.
/// Animasyon içinde IKI HIT noktası vardır:
///   hit1Delay'de Attack1'in vuruşu
///   hit2Delay'de Attack2'nin vuruşu
/// Süresi (duration) bitince Chase'e döner.
/// </summary>
public class ZagreusChainComboState : IBossState
{
    private readonly ZagreusController boss;
    private float startTime;
    private bool hit1Fired;
    private bool hit2Fired;

    public ZagreusChainComboState(ZagreusController boss) { this.boss = boss; }

    private ZagreusChainComboData Data => boss.chainComboData;

    // Animation Event'lerden çağrılır (varsa). Timer'dan önce çalışırsa timer atlanır.
    public void PerformHit1()
    {
        if (hit1Fired) return;
        hit1Fired = true;
        DoHit(Data.damage1, Data.hit1Offset);
    }

    public void PerformHit2()
    {
        if (hit2Fired) return;
        hit2Fired = true;
        DoHit(Data.damage2, Data.hit2Offset);
        SoundManager.Instance?.TryPlayOneShot(boss.chainCombo2ndHitSoundName);
    }

    public void Enter()
    {
        startTime = Time.time;
        hit1Fired = false;
        hit2Fired = false;

        if (boss.rb != null) boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        boss.zagAnim?.SetWalk(false);

        if (boss.player != null) boss.FaceTarget(boss.player.position);

        boss.zagAnim?.PlayChainCombo();
        SoundManager.Instance?.TryPlayOneShot(boss.chainComboSoundName);
    }

    public void Update()
    {
        float elapsed = Time.time - startTime;

        // Timer fallback — event yoksa devreye girer
        if (!hit1Fired && elapsed >= Data.hit1Delay) PerformHit1();
        if (!hit2Fired && elapsed >= Data.hit2Delay) PerformHit2();

        if (elapsed >= Data.duration)
        {
            boss.OnAttackCompleted();
        }
    }

    private void DoHit(int damage, Vector2 offset)
    {
        Vector2 facingMul = new Vector2(boss.isFacingRight ? 1f : -1f, 1f);
        Vector2 origin = (Vector2)boss.transform.position + Vector2.Scale(offset, facingMul);

        Collider2D hit = Physics2D.OverlapCircle(origin, Data.hitRange, boss.playerLayer);
        if (hit == null) return;

        PlayerHealth ph = hit.GetComponentInParent<PlayerHealth>();
        if (ph != null && !ph.isInvincible) ph.TakeDamage(damage);
    }

    public void FixedUpdate() { }
    public void Exit() { }
}
