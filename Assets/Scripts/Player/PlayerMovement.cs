using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float maxSpeed = 12f;
    public float acceleration = 10f;
    public float deceleration = 20f;
    public float turnSpeed = 15f;
    public float velPower = 0.9f;

    [Header("Zıplama & Yerçekimi (Game Feel)")]
    [SerializeField] private float jumpForce = 10f;
    public float fallMultiplier = 2.5f; // Düşerken yerçekimini artırır (Ayda yürüme hissini yok eder)
    private float defaultGravity;

    [Header("Platformer Asistanları (Affedicilik)")]
    public float coyoteTime = 0.15f;    // Platformdan düştükten sonra zıplanabilen süre
    [HideInInspector] public float coyoteTimeCounter;

    public float jumpBufferTime = 0.15f; // Yere inmeden basılan zıplamayı hafızada tutma süresi
    [HideInInspector] public float jumpBufferCounter;

    [Header("Çevre Kontrol Ayarları")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private Transform wallCheckTransform; // YENİ: Duvar kontrol noktası
    [SerializeField] private float wallCheckDistance = 0.2f; // YENİ: Duvara olan mesafe
    [SerializeField] private LayerMask groundLayer;

    public bool isDoubleJump = false;
    private int _remainingJumps;

    public PlayerInputHandler _playerInputHandler;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        defaultGravity = _rb.gravityScale;
    }

    private void Update()
    {
        // --- COYOTE TIME SAYACI ---
        if (IsGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // --- JUMP BUFFER SAYACI ---
        // Tuşa basıldığı an PlayerInputHandler bu değeri yükseltecek.
        // Biz burada sadece zamanla bu süreyi sıfırlıyoruz.
        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // --- AĞIR ÇEKİM DÜŞÜŞ (FALL MULTIPLIER) ---
        // Karakter zirveye ulaşıp aşağı düşmeye başladığında yerçekimini artır
        if (_rb.linearVelocityY < 0)
            _rb.gravityScale = defaultGravity * fallMultiplier;
        else
            _rb.gravityScale = defaultGravity;
    }

    public void Move()
    {
        float moveInput = _playerInputHandler.GetMoveDirection();
        float targetSpeed = moveInput * maxSpeed;
        float speedDif = targetSpeed - _rb.linearVelocityX;
        float accelRate;

        if (Mathf.Abs(targetSpeed) > 0.01f)
            accelRate = (Mathf.Sign(targetSpeed) != Mathf.Sign(_rb.linearVelocityX) && Mathf.Abs(_rb.linearVelocityX) > 0.1f) ? turnSpeed : acceleration;
        else
            accelRate = deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        _rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (Mathf.Abs(moveInput) < 0.01f)
            ApplyFriction();
    }

    // YENİ: Kaymayı önleyen asıl sürtünme / durdurma metodu
    public void ApplyFriction()
    {
        float amount = Mathf.Min(Mathf.Abs(_rb.linearVelocityX), Mathf.Abs(deceleration * Time.fixedDeltaTime));
        amount *= Mathf.Sign(_rb.linearVelocityX);
        _rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
    }

    public void Jump()
    {
        _rb.linearVelocityY = 0; // Önceki dikey hızı sıfırla ki zıplama her zaman tutarlı olsun
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpBufferCounter = 0f; // Zıpladık, hafızadaki tuşu temizle
    }

    public bool IsGrounded() => Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);

    // YENİ: Duvara dayanıp dayanmadığımızı kontrol eder
    public bool IsTouchingWall(float facingDirection)
    {
        return Physics2D.Raycast(wallCheckTransform.position, Vector2.right * facingDirection, wallCheckDistance, groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
        if (wallCheckTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheckTransform.position, wallCheckTransform.position + (Vector3.right * wallCheckDistance));
        }
    }
}