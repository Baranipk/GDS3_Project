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
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        pAnim.PlayBlockStart();

        // --- YENİ: BLOK BAŞLAMA SESİ ---
        SoundManager.Instance?.Get("Block")?.PlayOneShot();
    }

    // Bu metod, karakter bloktayken bir darbe aldığında Health scriptinden çağrılabilir
    public void PlayBlockHitSound()
    {
        SoundManager.Instance?.Get("BlockHit")?.PlayOneShot();
    }

    public void Update()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void FixedUpdate() { }

    public void Exit()
    {
        pAnim.StopBlock();
    }
}