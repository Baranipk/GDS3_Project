using UnityEngine;

public class SatyrIdleState : IEnemyState
{
    private SatyrController satyr;
    public SatyrIdleState(SatyrController satyr) => this.satyr = satyr;

    public void Enter() => satyr.enemyAnim.SetWalk(false);

    public void Update()
    {
        float distanceToPlayer = Vector2.Distance(satyr.transform.position, satyr.player.position);

        // Eğer oyuncu hala menzildeyse
        if (distanceToPlayer < satyr.detectionRadius)
        {
            // 1. Sürekli olarak yüzünü oyuncuya dön (Böylece mermiyi doğru yöne atar)
            satyr.CheckFlip(satyr.player.position.x - satyr.transform.position.x);

            // 2. Saldırı bekleme süresi (cooldown) dolduysa saldırıya geç!
            if (Time.time >= satyr.lastAttackTime + satyr.attackCooldown)
            {
                satyr.StateMachine.ChangeState(satyr.attackState);
            }
        }
        else
        {
            // Oyuncu menzilden çıktıysa devriye gezmeye geri dön
            satyr.StateMachine.ChangeState(satyr.patrolState);
        }
    }
    public void FixedUpdate() { }
    public void Exit() { }
}