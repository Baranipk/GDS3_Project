using System.Threading.Tasks;
using UnityEngine;

public class SatyrHurtState : IEnemyState
{
    private SatyrController satyr;
    public SatyrHurtState(SatyrController satyr) => this.satyr = satyr;

    public async void Enter()
    {
        satyr.rb.linearVelocity = Vector2.zero;
        satyr.enemyAnim.PlayHurt();

        await Task.Delay(400); // Sarsılma süresi

        // Eğer karakter ölmediyse bekleme durumuna dön
        if (satyr != null && !satyr.GetComponent<EnemyHealth>().IsDead)
        {
            satyr.StateMachine.ChangeState(satyr.idleState);
        }
    }
    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}