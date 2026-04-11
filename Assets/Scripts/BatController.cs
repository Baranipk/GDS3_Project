using UnityEngine;

public class BatController : EnemyController
{
    [Header("Yarasa Özel Ayarları")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public Transform[] waypoints; // Yarasanın devriye gezeceği noktalar

    // State'leri tanımlıyoruz
    public BatPatrolState patrolState;
    public BatChaseState chaseState;
    public BatDeathState deathState;
    public BatHurtState hurtState;

    protected override void Awake()
    {
        base.Awake(); // Ana sınıftaki Awake'i (StateMachine, rb vb.) çalıştır

        // State'leri oluştur ve bu kontrolcüyü onlara gönder
        patrolState = new BatPatrolState(this);
        chaseState = new BatChaseState(this);
        deathState = new BatDeathState(this);
        hurtState = new BatHurtState(this);
    }

    private void Start()
    {
        // Yarasanın yer çekiminden etkilenmemesi (uçması) için
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;

        // Oyuna Devriye (Patrol) durumunda başla
        StateMachine.Initialize(patrolState);
    }

    // Temas hasarı (Yarasa oyuncuya değerse hasar verir)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Çarptığımız objenin veya onun ana(kök) objesinin Tag'i "Player" mı?
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            // GetComponentInParent: Alt objeye çarpsak bile gidip ana objedeki Health scriptini bulur!
            Health playerHealth = collision.GetComponentInParent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                Debug.Log("Yarasa oyuncuyu ısırdı! Kalan Can: " + playerHealth.currentHealth);
            }
        }
    }
}