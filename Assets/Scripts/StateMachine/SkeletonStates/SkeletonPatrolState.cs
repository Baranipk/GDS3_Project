using UnityEngine;

public class SkeletonPatrolState : IEnemyState
{
    private SkeletonController skeleton;
    private Transform targetPoint;

    public SkeletonPatrolState(SkeletonController skeleton) => this.skeleton = skeleton;

    public void Enter()
    {
        skeleton.enemyAnim.SetWalk(true);
        targetPoint = skeleton.leftPoint;
    }

    public void Update()
    {
        if (Vector2.Distance(skeleton.transform.position, skeleton.player.position) < skeleton.detectionRadius)
        {
            skeleton.StateMachine.ChangeState(skeleton.chaseState);
            return;
        }

        skeleton.transform.position = Vector2.MoveTowards(skeleton.transform.position,
            new Vector2(targetPoint.position.x, skeleton.transform.position.y), skeleton.patrolSpeed * Time.deltaTime);

        skeleton.CheckFlip(targetPoint.position.x - skeleton.transform.position.x);

        if (Vector2.Distance(skeleton.transform.position, new Vector2(targetPoint.position.x, skeleton.transform.position.y)) < 0.1f)
            targetPoint = targetPoint == skeleton.leftPoint ? skeleton.rightPoint : skeleton.leftPoint;
    }
    public void FixedUpdate() { }
    public void Exit() { }
}