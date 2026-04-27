using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerDeathState : IplayerState
{
    PlayerController controller;
    PlayerAnimation pAnim;
    Rigidbody2D rb;

    public PlayerDeathState(PlayerController controller)
    {
        this.controller = controller;
        this.pAnim = controller.GetComponent<PlayerAnimation>();
        this.rb = controller.GetComponent<Rigidbody2D>();
    }

    public async void Enter()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        pAnim.Death();

        // --- YENİ: ÖLÜM SESİ ---
        SoundManager.Instance?.Get("Death")?.PlayOneShot();

        controller.GetComponentInChildren<Collider2D>().enabled = false;
        await UniTask.Delay(2000);
        controller.Respawn();
    }

    public void Update() { } // Ölüyken input almasın diye boş bırakıyoruz
    public void FixedUpdate() { }
    public void Exit() { }
}