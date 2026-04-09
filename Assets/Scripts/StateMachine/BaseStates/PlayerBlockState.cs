using UnityEngine;

public class PlayerBlockState : IplayerState
{
    PlayerController controller;
    Rigidbody2D rb;
    PlayerAnimation pAnim;

    public PlayerBlockState(PlayerController controller)
    {
        this.controller = controller;

        this.rb = controller.GetComponent<Rigidbody2D>();
        this.pAnim = controller.GetComponent<PlayerAnimation>();
    }

    public void Enter()
    {
        // Blok başladığında karakteri durdur (Saldırıdaki gibi)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Blok başlama animasyonunu tetikle
        pAnim.PlayBlockStart();
    }

    public void Update()
    {
        // Blok boyunca hareket edilmesini engellemek için hızı sıfır tutabilirsin
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void FixedUpdate() { }

    public void Exit()
    {
        // Bloktan çıkarken animasyonu kapat
        pAnim.StopBlock();
    }
}
