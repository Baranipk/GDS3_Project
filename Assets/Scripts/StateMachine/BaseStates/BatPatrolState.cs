using UnityEngine;

public class BatPatrolState : IEnemyState
{
    private BatController bat;
    private bool movingRight = true;

    public BatPatrolState(BatController batController)
    {
        this.bat = batController;
    }

    public void Enter()
    {
        // Başlangıçta hangi yöne bakıyorsa o yöne gitmeye başla (opsiyonel)
        movingRight = bat.transform.localScale.x > 0;
    }

    public void Update()
    {
        // 1. OYUNCU KONTROLÜ: Oyuncu menzile girdi mi?
        float distanceToPlayer = Vector2.Distance(bat.transform.position, bat.player.position);
        if (distanceToPlayer <= bat.detectionRadius)
        {
            bat.StateMachine.ChangeState(bat.chaseState);
            return;
        }

        // 2. DEVRİYE HAREKETİ (X Ekseni Sınır Kontrolü)
        PatrolMovement();
    }

    private void PatrolMovement()
    {
        // Hedef X koordinatını belirle
        float targetX = movingRight ? bat.rightLimitX : bat.leftLimitX;

        // Sadece X ekseninde hareket oluştur (Y sabit kalır)
        Vector2 targetPosition = new Vector2(targetX, bat.transform.position.y);

        // Karakteri hareket ettir
        bat.transform.position = Vector2.MoveTowards(
            bat.transform.position,
            targetPosition,
            bat.patrolSpeed * Time.deltaTime
        );

        // Görsel yönü ayarla (CheckFlip metodunu kullanarak)
        float moveDirection = targetX - bat.transform.position.x;
        bat.CheckFlip(moveDirection);

        // Sınıra ulaştı mı kontrol et (0.05f küçük bir tolerans payıdır)
        if (Mathf.Abs(bat.transform.position.x - targetX) < 0.05f)
        {
            movingRight = !movingRight; // Yön değiştir
        }
    }

    public void FixedUpdate() { }
    public void Exit() { }
}