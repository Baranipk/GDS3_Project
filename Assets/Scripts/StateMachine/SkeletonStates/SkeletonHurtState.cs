using System.Threading.Tasks;
using UnityEngine;

public class SkeletonHurtState : IEnemyState
{
    private SkeletonController skeleton;
    public SkeletonHurtState(SkeletonController skeleton) => this.skeleton = skeleton;

    public async void Enter()
    {
        skeleton.rb.linearVelocity = Vector2.zero;
        skeleton.enemyAnim.PlayHurt();

        await Task.Delay(400); // Sarsılma süresi

        // DÜZELTME: Eğer karakter bu bekleme süresinde öldüyse, asla chase state'e dönme!
        if (skeleton != null && !skeleton.GetComponent<EnemyHealth>().IsDead)
        {
            skeleton.StateMachine.ChangeState(skeleton.chaseState);
        }
    }
    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}