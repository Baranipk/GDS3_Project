using UnityEngine;

public class BossIdleState : IBossState
{
    private readonly BossController boss;

    public BossIdleState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        if (boss.rb != null) boss.rb.linearVelocity = Vector2.zero;
        boss.bossAnim.SetWalk(false);
    }

    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}
