using UnityEngine;

public class SkeletonIdleState : IEnemyState
{
    private SkeletonController skeleton;
    public SkeletonIdleState(SkeletonController skeleton) => this.skeleton = skeleton;

    public void Enter() => skeleton.enemyAnim.SetWalk(false);

    public void Update()
    {
        if (Vector2.Distance(skeleton.transform.position, skeleton.player.position) < skeleton.detectionRadius)
            skeleton.StateMachine.ChangeState(skeleton.chaseState);
    }
    public void FixedUpdate() { }
    public void Exit() { }
}