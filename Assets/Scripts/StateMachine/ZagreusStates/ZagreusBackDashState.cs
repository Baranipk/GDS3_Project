using UnityEngine;

public class ZagreusBackDashState : IBossState
{
    private readonly ZagreusController boss;
    private float endTime;
    private float dashDir; // +1 sağa, -1 sola (player'dan uzaklaşan yön)

    public ZagreusBackDashState(ZagreusController boss) { this.boss = boss; }

    public void Enter()
    {
        // Player'a yüzü dönük kalsın
        if (boss.player != null) boss.FaceTarget(boss.player.position);

        // Geri sıçrama yönü: player'ın TERSİ
        if (boss.player != null)
            dashDir = Mathf.Sign(boss.transform.position.x - boss.player.position.x);
        else
            dashDir = boss.isFacingRight ? -1f : 1f;
        if (dashDir == 0f) dashDir = -1f;

        boss.zagAnim?.SetWalk(false);
        boss.zagAnim?.PlayBackDash();
        SoundManager.Instance?.TryPlayOneShot(boss.backDashSoundName);

        endTime = Time.time + boss.backDashDuration;
    }

    public void Update()
    {
        if (Time.time >= endTime)
            boss.StateMachine.ChangeState(boss.chaseState);
    }

    public void FixedUpdate()
    {
        if (boss.rb == null) return;
        boss.rb.linearVelocity = new Vector2(dashDir * boss.backDashSpeed, boss.rb.linearVelocity.y);
    }

    public void Exit()
    {
        if (boss.rb != null)
            boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
    }
}
