using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerHurtState : IplayerState
{
    PlayerController controller;
    PlayerAnimation pAnim;
    Rigidbody2D rb;
    PlayerHealth health;

    public PlayerHurtState(PlayerController controller)
    {
        this.controller = controller;
        this.pAnim = controller.GetComponent<PlayerAnimation>();
        this.rb = controller.GetComponent<Rigidbody2D>();
        this.health = controller.GetComponent<PlayerHealth>();
    }

    public async void Enter()
    {
        health.isInvincible = true;
        pAnim.PlayHurt();

        // --- YENİ: HASAR ALMA SESİ ---
        SoundManager.Instance?.Get("Hurt")?.PlayOneShot();

        controller.ApplyKnockback();

        await UniTask.Delay(300);

        if (health.currentHealth > 0)
        {
            controller.playerStateMachine.ChangeState(controller.idleState);
            await UniTask.Delay(500);
            health.isInvincible = false;
        }
    }

    // Hasar yediği ve sersemlediği sırada Input almasını engellemek için Update metodları boş bırakılır
    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}