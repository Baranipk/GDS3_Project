using UnityEngine;

public class SkeletonChaseState : IEnemyState
{
    private SkeletonController skeleton;
    public SkeletonChaseState(SkeletonController skeleton) => this.skeleton = skeleton;

    public void Enter() => skeleton.enemyAnim.SetWalk(true);

    public void Update()
    {
        float distance = Vector2.Distance(skeleton.transform.position, skeleton.player.position);

        // 1. SALDIRI MENZİLİ KONTROLÜ
        if (distance <= skeleton.stopDistance)
        {
            // Menzile girdiğinde yürüme animasyonunu durdur
            skeleton.enemyAnim.SetWalk(false);
            skeleton.rb.linearVelocity = new Vector2(0, skeleton.rb.linearVelocity.y); // Fiziksel olarak durdur

            // Eğer saldırı cooldown süresi dolmuşsa saldırıya geç
            if (Time.time > skeleton.lastAttackTime + skeleton.attackCooldown)
            {
                skeleton.StateMachine.ChangeState(skeleton.attackState);
            }
            return;
        }

        // 2. KAYBETME MENZİLİ KONTROLÜ
        if (distance > skeleton.loseRadius)
        {
            skeleton.StateMachine.ChangeState(skeleton.idleState);
            return;
        }

        // 3. TAKİP HAREKETİ (Menzil dışındaysa yürü)
        skeleton.enemyAnim.SetWalk(true);
        skeleton.transform.position = Vector2.MoveTowards(skeleton.transform.position,
            new Vector2(skeleton.player.position.x, skeleton.transform.position.y), skeleton.chaseSpeed * Time.deltaTime);

        skeleton.CheckFlip(skeleton.player.position.x - skeleton.transform.position.x);
    }

    public void FixedUpdate() { }
    public void Exit() { }
}