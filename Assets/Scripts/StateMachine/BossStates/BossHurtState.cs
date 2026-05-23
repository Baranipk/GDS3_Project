using UnityEngine;

public class BossHurtState : IBossState
{
    private readonly BossController boss;
    private float _exitTime;

    public BossHurtState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        if (boss.rb != null)
        {
            // Hem yatay hem düşey hızı sıfırla — boss yere kilitlensin
            boss.rb.linearVelocity = Vector2.zero;
        }
        boss.bossAnim.SetWalk(false);
        boss.bossAnim.PlayHurt();

        _exitTime = Time.time + boss.hurtDuration;
    }

    public void Update()
    {
        // Donmuş gibi dur — hareket etmesin
        if (boss.rb != null && boss.rb.bodyType == RigidbodyType2D.Dynamic)
        {
            boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        }

        if (Time.time >= _exitTime)
            boss.StateMachine.ChangeState(boss.chaseState);
    }

    public void FixedUpdate() { }

    public void Exit() { }
}
