using UnityEngine;

public class ZagreusDeathState : IBossState
{
    private readonly ZagreusController boss;
    public ZagreusDeathState(ZagreusController boss) { this.boss = boss; }

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

        boss.zagAnim?.SetWalk(false);
        boss.zagAnim?.PlayDeath();
    }
    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}
