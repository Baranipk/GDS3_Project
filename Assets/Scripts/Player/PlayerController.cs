using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine playerStateMachine { get; set; }
    public float FacingDirection { get; private set; } = 1f; // Başlangıçta sağa bakıyor
    private PlayerInputHandler _inputHandler;

    [Header("Knockback Ayarları")]
    public Vector2 knockbackForce = new Vector2(5f, 5f); // Normal hasar geri tepmesi
    public float blockKnockbackMultiplier = 0.4f; // Blok anındaki geri tepme oranı (%40)
    public float deathKnockbackMultiplier = 2.2f; // Ölüm anında güçlü itme

    public PlayerIdleState idleState;
    public PlayerMoveState moveState;
    public PlayerJumpState jumpState;
    public PlayerDeathState deathState;
    public PlayerBumperState bumperState;
    public PlayerAttackState attackState;
    public PlayerBlockState blockState;
    public PlayerThrowState throwState;
    public PlayerHurtState hurtState;
    private void Awake()
    {
        playerStateMachine = new PlayerStateMachine();
        _inputHandler = GetComponent<PlayerInputHandler>();

        idleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);
        jumpState = new PlayerJumpState(this);
        deathState = new PlayerDeathState(this);
        bumperState = new PlayerBumperState(this);
        attackState = new PlayerAttackState(this);
        blockState = new PlayerBlockState(this);
        throwState = new PlayerThrowState(this);
        hurtState = new PlayerHurtState(this);
    }

    private void Start()
    {
        GameObject startPoint = GameObject.FindGameObjectWithTag("StartPosition");

        if (startPoint != null)
        {
            transform.position = startPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("Sahnede 'StartPosition' tagine sahip bir obje bulunamadı!");
        }

        playerStateMachine.Initialize(idleState);
    }

    private void Update()
    {
        playerStateMachine.CurrentState.Update();
        HandleFlip();

    }
            
    private void FixedUpdate()
    {
        playerStateMachine.ExecuteFixedUpdate();
        // Debug.Log($"CurrentState: {playerStateMachine.CurrentState}");
    }

    private void HandleFlip()
    {
        float moveInput = _inputHandler.GetMoveDirection();

        // Eğer bir input varsa ve baktığımız yönden farklıysa karakteri çevir
        if (moveInput > 0 && FacingDirection != 1f)
        {
            Flip(1f);
        }
        else if (moveInput < 0 && FacingDirection != -1f)
        {
            Flip(-1f);
        }
    }

    public void ApplyKnockback(float multiplier = 1.0f)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float knockbackDir = -FacingDirection;

        rb.linearVelocity = Vector2.zero;

        // Küçük bir "Y" zıplaması eklemek her zaman iyidir (Y ekseni kuvvetini artır)
        // Bu, karakterin yere sürtünüp durmasını engeller ve geriye kaymasını sağlar
        float verticalForce = knockbackForce.y * 1.2f;

        rb.AddForce(new Vector2(knockbackDir * knockbackForce.x * multiplier, verticalForce * multiplier), ForceMode2D.Impulse);
    }

    private void Flip(float newDirection)
    {
        FacingDirection = newDirection;
        Vector3 scale = transform.localScale;
        scale.x = newDirection; // Karakteri görsel olarak aynala
        transform.localScale = scale;
    }

    // PlayerController.cs içine ekle

    [SerializeField] private Transform spawnPoint; // Karakterin doğacağı boş bir obje

    public void Respawn()
    {
        if (this == null || gameObject == null) return;
        // 1. Pozisyonu başlangıca çek
        transform.position = spawnPoint.position;

        // 2. Rigidbody tipini tekrar Dynamic yap (Yer çekimi geri gelir)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 3. Collider'ı geri aç
        GetComponentInChildren<Collider2D>().enabled = true;

        GetComponent<PlayerAnimation>().PlayIdleForce();

        // 4. Canı ve State'i sıfırla
        GetComponent<PlayerHealth>().ResetHealth();
        playerStateMachine.ChangeState(idleState);     
    }
}



