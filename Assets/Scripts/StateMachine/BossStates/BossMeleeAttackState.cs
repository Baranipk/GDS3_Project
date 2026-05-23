using UnityEngine;

public class BossMeleeAttackState : IBossState
{
    private readonly BossController boss;
    private float _startTime;
    private bool _hitFired;

    public BossMeleeAttackState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        boss.lastMeleeTime = Time.time;
        _startTime = Time.time;
        _hitFired = false;

        if (boss.rb != null) boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        boss.bossAnim.SetWalk(false);

        if (boss.player != null) boss.FaceTarget(boss.player.position);
        boss.bossAnim.PlayMelee();
    }

    public void Update()
    {
        float elapsed = Time.time - _startTime;

        // Vuruş zamanı geldiyse (event'e gerek yok)
        if (!_hitFired && elapsed >= boss.meleeHitDelay)
        {
            _hitFired = true;
            boss.AnimEvent_MeleeHit();
        }

        // Animasyon süresi bittiyse chase'e dön
        if (elapsed >= boss.meleeAttackDuration)
        {
            boss.StateMachine.ChangeState(boss.chaseState);
        }
    }

    public void FixedUpdate() { }
    public void Exit() { }
}
