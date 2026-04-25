using UnityEngine;

public class BatController : EnemyController
{
    [Header("Patrol Limit Ayarları")]
    public float leftLimitX; // Sol limit (Dünya koordinatı)
    public float rightLimitX; // Sağ limit (Dünya koordinatı)

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
}