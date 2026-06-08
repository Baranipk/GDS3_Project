using UnityEngine;

public class ZagreusHurtState : IBossState
{
    private readonly ZagreusController boss;
    private float exitTime;

    public ZagreusHurtState(ZagreusController boss) { this.boss = boss; }

    public void Enter()
    {
        if (boss.rb != null) boss.rb.linearVelocity = Vector2.zero;
        boss.zagAnim?.SetWalk(false);
        boss.zagAnim?.PlayHurt();
        exitTime = Time.time + boss.hurtDuration;
    }

    public void Update()
    {
        if (boss.rb != null && boss.rb.bodyType == RigidbodyType2D.Dynamic)
            boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);

        if (Time.time >= exitTime)
            boss.StateMachine.ChangeState(boss.chaseState);
    }

    public void FixedUpdate() { }
    public void Exit() { }
}
