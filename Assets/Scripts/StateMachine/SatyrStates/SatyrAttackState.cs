using UnityEngine;

public class SatyrAttackState : IEnemyState
{
    private SatyrController satyr;
    private float attackDuration = 1.0f; // Animasyonun yaklaşık süresi

    public SatyrAttackState(SatyrController satyr) => this.satyr = satyr;

    public void Enter()
    {
        // Fiziksel olarak durdur ve yüzünü oyuncuya dön
        satyr.rb.linearVelocity = Vector2.zero;
        satyr.enemyAnim.SetWalk(false);
        satyr.CheckFlip(satyr.player.position.x - satyr.transform.position.x);

        // Saldırı animasyonunu tetikle
        satyr.enemyAnim.PlayAttack();
        satyr.lastAttackTime = Time.time;
    }

    public void Update()
    {
        // Animasyon bitince Idle durumuna dön.
        // İstersen Skeleton'da yaptığımız gibi SatyrController içindeki OnAttackComplete() 
        // metoduyla da bu geçişi sağlayabilirsin. Şimdilik süre ile kontrol ediyoruz.
        if (Time.time > satyr.lastAttackTime + attackDuration)
        {
            satyr.StateMachine.ChangeState(satyr.idleState);
        }
    }

    public void FixedUpdate() { }
    public void Exit() { }
}