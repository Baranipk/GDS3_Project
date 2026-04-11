using UnityEngine;

public class BatDeathState : IEnemyState
{
    private BatController bat;

    public BatDeathState(BatController batController)
    {
        this.bat = batController;
    }

    public void Enter()
    {
        // 1. Ölüm animasyonunu oynat
        if (bat.enemyAnim != null) bat.enemyAnim.PlayDeath();

        // 2. YER ÇEKİMİNİ AÇ
        // Yarasa normalde uçtuğu için gravityScale 0'dır. 
        // Bunu 3 veya 4 yaparak hızlıca yere düşmesini sağlıyoruz.
        bat.rb.gravityScale = 3.5f;

        // 3. HIZI SIFIRLA
        // İleri doğru uçuşunu durdur ki direkt aşağı düşsün.
        bat.rb.linearVelocity = new Vector2(0, bat.rb.linearVelocity.y);

        // 4. COLLIDER'I KATI YAP (Trigger'ı Kapat)
        // Yarasa hayattayken oyuncunun içinden geçebilmesi için muhtemelen isTrigger = true idi.
        // Yere çarpıp durabilmesi için bunu false (katı) yapıyoruz.
        Collider2D batCollider = bat.GetComponent<Collider2D>();
        if (batCollider != null)
        {
            batCollider.isTrigger = false;
        }

        // 5. LAYER DEĞİŞTİR (Oyuncuyu engellemesin)
        // Eğer oyuncu ölü yarasanın üzerinden geçebilsin istiyorsan Layer'ı değiştiriyoruz.
        bat.gameObject.layer = LayerMask.NameToLayer("IgnorePlayer");

        // 6. OBJEYİ SİL
        // 4 saniye sonra ceset yok olur.
        Object.Destroy(bat.gameObject, 2f);
    }

    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
}