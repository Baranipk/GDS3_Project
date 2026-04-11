using UnityEngine;

// abstract yapıyoruz çünkü bu scripti direkt bir objeye atmayacağız,
// BatController veya SkeletonController bunu miras alacak.
public abstract class EnemyController : MonoBehaviour
{
    public EnemyStateMachine StateMachine { get; private set; }

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public EnemyAnimation enemyAnim;
    [HideInInspector] public Transform player;

    [Header("Ortak AI Ayarları")]
    public float detectionRadius = 6f; // Oyuncuyu fark etme menzili
    public float loseRadius = 10f;     // Takibi bırakma menzili

    public bool isFacingRight = true;

    protected virtual void Awake()
    {
        StateMachine = new EnemyStateMachine();
        rb = GetComponent<Rigidbody2D>();
        enemyAnim = GetComponent<EnemyAnimation>();// Animator genelde modelde olur

        // Oyuncuyu bul (Tag'inin "Player" olduğundan emin ol)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    protected virtual void Update()
    {
        StateMachine.CurrentState?.Update();
    }

    protected virtual void FixedUpdate()
    {
        StateMachine.CurrentState?.FixedUpdate();
    }

    // Düşmanın baktığı yönü ayarlayan ortak metot
    public void CheckFlip(float moveDirectionX)
    {
        if (moveDirectionX > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveDirectionX < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f); // Objenin yönünü çevirir
    }

    // Menzili sahnede çizerek görmek için (Sadece Editörde çalışır)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}