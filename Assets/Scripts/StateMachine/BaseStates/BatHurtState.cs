using UnityEngine;
using System.Threading.Tasks;

public class BatHurtState : IEnemyState
{
    private BatController bat;
    private float hurtDuration = 0.4f; // Hasar alıp donma süresi

    public BatHurtState(BatController batController)
    {
        this.bat = batController;
    }

    public async void Enter()
    {
        // 1. Hareketi tamamen durdur
        bat.rb.linearVelocity = Vector2.zero;

        // 2. Hasar animasyonunu oynat
        if (bat.enemyAnim != null)
        {
            bat.enemyAnim.PlayHurt();
        }

        // Vuruşun yönünü hesapla (Oyuncudan dışarı doğru)
        // Vector2 knockbackDirection = (bat.transform.position - bat.player.position).normalized;
       // bat.rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);

        // 3. Belirlenen süre kadar bekle (Stun etkisi)
        await Task.Delay((int)(hurtDuration * 600));

        // 4. Eğer hala hayattaysa (ölmediyse) oyuncuyu kovalamaya geri dön
        // (Burada 'bat' objesinin hala var olup olmadığını kontrol etmek güvenli olur)
        if (bat != null && bat.StateMachine.CurrentState == this)
        {
            bat.StateMachine.ChangeState(bat.chaseState);
        }
    }

    public void Update() { } // Hasar alırken hiçbir şey yapma (Patrol/Chase kodları çalışmaz)
    public void FixedUpdate() { }
    public void Exit() { }
}