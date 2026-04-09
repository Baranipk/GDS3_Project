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
        // Fırlatma sırasında karakterin kaymasını engelle
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Animator'da "ThrowTrigger" adında bir trigger oluşturman gerekecek
        pAnim.PlayThrow();

        // Not: SpawnJavelin() metodunu burada çağırmıyoruz! 
        // Çünkü mızrağın animasyonun en başında değil, elin ileri gittiği an çıkmasını istiyoruz.
        // Bunu Unity Editöründe Animation Event ile yapacağız.

        // Animasyonun bitmesi için tahmini bir süre bekle (Animasyon sürene göre ayarla)
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