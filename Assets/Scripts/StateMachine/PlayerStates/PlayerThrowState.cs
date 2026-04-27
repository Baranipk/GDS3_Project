using UnityEngine;
using Cysharp.Threading.Tasks; // Delay için

public class PlayerThrowState : IplayerState
{
    PlayerController controller;
    PlayerAnimation pAnim;
    Rigidbody2D rb;

    public PlayerThrowState(PlayerController controller)
    {
        this.controller = controller;
        this.pAnim = controller.GetComponent<PlayerAnimation>();
        this.rb = controller.GetComponent<Rigidbody2D>();
    }

    public async void Enter()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        pAnim.PlayThrow();

        // --- YENİ: FIRLATMA SESİ ---
        SoundManager.Instance?.Get("Throw")?.PlayOneShot();

        await UniTask.Delay(400);
        controller.playerStateMachine.ChangeState(controller.idleState);
    }

    public void Update()
    {
        // Fırlatma sırasında hareket iptali
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void FixedUpdate() { }

    public void Exit() { }
}