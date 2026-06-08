using UnityEngine;

public class ZagreusIdleState : IBossState
{
    private readonly ZagreusController boss;
    public ZagreusIdleState(ZagreusController boss) { this.boss = boss; }

    public void Enter()
    {
        if (boss.rb != null) boss.rb.linearVelocity = Vector2.zero;
        boss.zagAnim?.SetWalk(false);
    }
    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}
