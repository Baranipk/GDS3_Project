using UnityEngine;

public class BatChaseState : IEnemyState
{
    private BatController bat;

    public BatChaseState(BatController batController)
    {
        this.bat = batController;
    }

    public void Enter()
    {
        // Gerekirse agresif uçuş animasyonu
        // bat.anim.Play("BatFlyFast");
    }

    public void Update()
    {
        float distanceToPlayer = Vector2.Distance(bat.transform.position, bat.player.position);

        // 1. OYUNCUYU KAYBETME KONTROLÜ: Oyuncu çok uzaklaştıysa devriyeye dön
        if (distanceToPlayer > bat.loseRadius)
        {
            bat.StateMachine.ChangeState(bat.patrolState);
            return;
        }

        // 2. TAKİP HAREKETİ: Oyuncuya doğru uç
        bat.transform.position = Vector2.MoveTowards(bat.transform.position, bat.player.position, bat.chaseSpeed * Time.deltaTime);

        // Yönünü oyuncuya çevir
        bat.CheckFlip(bat.player.position.x - bat.transform.position.x);
    }

    public void FixedUpdate() { }
    public void Exit() { }
}