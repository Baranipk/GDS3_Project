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

        SoundManager.Instance?.Get("Death")?.PlayOneShot();

        controller.GetComponentInChildren<Collider2D>().enabled = false;

        // Karakter öldükten 1.5 - 2 saniye sonra ekran gelsin (animasyon bitsin diye)
        await UniTask.Delay(2000);

        if (controller == null || controller.gameObject == null) return;

        // ARTIK OTOMATİK RESPAWN YOK! Ölüm ekranını çağırıyoruz.
        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.ShowDeathScreen();
        }
        else
        {
            // Eğer DeathManager yoksa oyun takılmasın diye eski usul devam et (Güvenlik önlemi)
            controller.Respawn();
        }
    }

    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}