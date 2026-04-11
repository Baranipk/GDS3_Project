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
        // 1. Hareketleri durdur
        rb.linearVelocity = Vector2.zero;

        rb.bodyType = RigidbodyType2D.Static;
        // 2. Ölüm animasyonunu tetikle
        pAnim.Death();


        // 3. Etkileşimi kes

        controller.GetComponentInChildren<Collider2D>().enabled = false;

        // 4. ANIMASYONUN BİTMESİNİ BEKLE (Örn: 2 saniye)
        // Ölüm animasyonun ne kadar sürüyorsa o kadar beklet
        await UniTask.Delay(2000);

        // 5. Yeniden doğuşu başlat
        controller.Respawn();
    }

    public void Update() { } // Ölüyken input almasın diye boş bırakıyoruz
    public void FixedUpdate() { }
    public void Exit() { }
}