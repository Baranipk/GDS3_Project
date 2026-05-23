using UnityEngine;

public class BossDeathState : IBossState
{
    private readonly BossController boss;

    public BossDeathState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        boss.IsInvulnerable = true;
        if (boss.rb != null)
        {
            boss.rb.linearVelocity = Vector2.zero;
            boss.rb.bodyType = RigidbodyType2D.Kinematic;
        }

        foreach (var col in boss.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        boss.bossAnim.SetWalk(false);
        boss.bossAnim.PlayDeath();
    }

    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}
