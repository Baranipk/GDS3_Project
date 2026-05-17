using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerDeathState : IplayerState
{
    PlayerController controller;
    PlayerAnimation pAnim;
    PlayerMovement pMovement;
    Rigidbody2D rb;

    public PlayerDeathState(PlayerController controller)
    {
        this.controller = controller;
        this.pAnim = controller.GetComponent<PlayerAnimation>();
        this.rb = controller.GetComponent<Rigidbody2D>();
        this.pMovement = controller.GetComponent<PlayerMovement>();
    }

    public async void Enter()
    {
        // ── 1. Yatay hareketi durdur, dikey serbest (yerçekimi çalışsın) ───
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.bodyType = RigidbodyType2D.Dynamic;

        // ── 2. Sadece Ground layer ile çarpış, diğer her şeyden geç ────────
        // excludeLayers: "bu layer'larla ÇARPIŞMA" demek.
        // ~groundLayer  : ground dışındaki TÜM layer'lar → hepsini hariç tut.
        // Sonuç: karakter yalnızca zemine iner, düşman/platform/duvardan geçer.
        LayerMask originalExclude = rb.excludeLayers;
        rb.excludeLayers = ~pMovement.groundLayer;

        // ── 3. Animasyon + ses ────────────────────────────────────────────
        pAnim.Death();
        SoundManager.Instance?.Get("Death")?.PlayOneShot();

        // ── 4. Yere inene kadar bekle (max 2 saniye timeout) ─────────────
        float elapsed = 0f;
        float timeout = 2f;

        while (!pMovement.IsGrounded() && elapsed < timeout)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (controller == null || controller.gameObject == null) return;
            elapsed += Time.deltaTime;
        }

        // ── 5. Yere indi — tamamen dondur ─────────────────────────────────
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        // excludeLayers'ı eski haline getir (Static modda fark etmez
        // ama Respawn sonrası Dynamic'e dönünce önemli)
        rb.excludeLayers = originalExclude;

        // ── 6. Ölüm ekranı için bekle (animasyon bitsin) ──────────────────
        // Toplam süre: yere iniş süresi + bu 1.5sn = ~2sn hissiyatı
        await UniTask.Delay(1500);

        if (controller == null || controller.gameObject == null) return;

        if (DeathManager.Instance != null)
            DeathManager.Instance.ShowDeathScreen();
        else
            controller.Respawn();
    }

    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}