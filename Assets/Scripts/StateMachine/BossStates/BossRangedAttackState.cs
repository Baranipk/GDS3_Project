using UnityEngine;

public class BossRangedAttackState : IBossState
{
    private readonly BossController boss;
    private float _startTime;
    private bool _projectileSpawned;

    public BossRangedAttackState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        boss.lastRangedTime = Time.time;
        _startTime = Time.time;
        _projectileSpawned = false;

        if (boss.rb != null) boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        boss.bossAnim.SetWalk(false);

        if (boss.player != null) boss.FaceTarget(boss.player.position);
        boss.bossAnim.PlayRanged();
    }

    public void Update()
    {
        float elapsed = Time.time - _startTime;

        // Projectile spawn zamanı geldiyse
        if (!_projectileSpawned && elapsed >= boss.rangedSpawnDelay)
        {
            _projectileSpawned = true;
            boss.AnimEvent_SpawnProjectile();
        }

        // Animasyon süresi bittiyse chase'e dön
        if (elapsed >= boss.rangedAttackDuration)
        {
            boss.StateMachine.ChangeState(boss.chaseState);
        }
    }

    public void FixedUpdate() { }
    public void Exit() { }
}
