using UnityEngine;

public class SkeletonAttackState : IEnemyState
{
    private SkeletonController skeleton;
    private float attackDuration = 1.0f; // Animasyonun yaklaşık süresi

    public SkeletonAttackState(SkeletonController skeleton) => this.skeleton = skeleton;

    public void Enter()
    {
        // Saldırı başladığında tamamen durduğundan emin ol
        skeleton.rb.linearVelocity = Vector2.zero;
        skeleton.enemyAnim.SetWalk(false);

        // Saldırı animasyonunu tetikle
        skeleton.enemyAnim.PlayAttack();

        // Son saldırı zamanını güncelle
        skeleton.lastAttackTime = Time.time;
    }

    public void Update()
    {
        // Animasyon süresi dolduğunda tekrar takip (Chase) durumuna dön
        // Bu süre içinde yürüme komutları çalışmayacağı için karakter sabit kalır
        if (Time.time > skeleton.lastAttackTime + attackDuration)
        {
            skeleton.StateMachine.ChangeState(skeleton.chaseState);
        }
    }

    public void FixedUpdate() { }
    public void Exit() { }
}