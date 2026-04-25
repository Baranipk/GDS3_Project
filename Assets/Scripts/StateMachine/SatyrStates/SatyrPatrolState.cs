using UnityEngine;

public class SatyrPatrolState : IEnemyState
{
    private SatyrController satyr;
    private float targetX; // Artık Transform yerine gidilecek X koordinatını tutuyoruz

    public SatyrPatrolState(SatyrController satyr) => this.satyr = satyr;

    public void Enter()
    {
        satyr.enemyAnim.SetWalk(true);
        targetX = satyr.leftBoundary; // İlk olarak sol sınıra doğru yürümeye başla
    }

    public void Update()
    {
        // Oyuncuyu fark ederse (Menzile girerse), devriyeyi bırak ve dur (Idle'a geç)
        if (Vector2.Distance(satyr.transform.position, satyr.player.position) < satyr.detectionRadius)
        {
            satyr.StateMachine.ChangeState(satyr.idleState);
            return;
        }

        // Hedef X koordinatına doğru hareket et (Y ve Z ekseni sabit kalır)
        Vector2 targetPos = new Vector2(targetX, satyr.transform.position.y);
        satyr.transform.position = Vector2.MoveTowards(satyr.transform.position, targetPos, satyr.patrolSpeed * Time.deltaTime);

        // Yönü kontrol et (Sola mı sağa mı bakmalı?)
        satyr.CheckFlip(targetX - satyr.transform.position.x);

        // Hedef noktaya (sınıra) ulaşıp ulaşmadığını kontrol et
        if (Mathf.Abs(satyr.transform.position.x - targetX) < 0.1f)
        {
            // Sola ulaştıysa hedefi sağ yap, sağa ulaştıysa hedefi sol yap
            targetX = targetX == satyr.leftBoundary ? satyr.rightBoundary : satyr.leftBoundary;
        }
    }
    public void FixedUpdate() { }
    public void Exit() { }
}