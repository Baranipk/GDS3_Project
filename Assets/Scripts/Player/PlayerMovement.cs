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
    public float fallMultiplier = 2.5f;
    [Tooltip("Açıksa: tuş erken bırakıldığında yukarı hız kesilir (variable jump height). Kapalıysa: her zıplama tam yükseklik.")]
    public bool enableVariableJumpHeight = true;
    [Tooltip("Tuş erken bırakıldığında yukarı hız bu oranla kesilir (0.4-0.6 önerilir)")]
    public float jumpCutMultiplier = 0.5f;
    private float defaultGravity;

    [Header("Apex Hang Time (Zirvede hafif yerçekimi)")]
    [Tooltip("velY mutlak değeri bu eşiğin altındaysa karakter apex'te sayılır")]
    public float apexThreshold = 1.8f;
    [Tooltip("Apex'te yerçekimi çarpanı — küçük = uzun süzülme")]
    public float apexGravityMultiplier = 0.5f;

    [Header("Platformer Asistanları (Affedicilik)")]
    public float coyoteTime = 0.15f;
    [HideInInspector] public float coyoteTimeCounter;

    public float jumpBufferTime = 0.15f;
    [HideInInspector] public float jumpBufferCounter;

    [Header("Çevre Kontrol Ayarları")]
    [SerializeField] private float groundCheckRadius = 0.05f; // Box yüksekliği için kullanılır
    [SerializeField] private float groundCheckWidth = 0.4f;  // Karakter genişliğine göre ayarla
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private Transform wallCheckTransform;
    [SerializeField] private float wallCheckDistance = 0.2f;

    // PUBLIC — PlayerDeathState ve PlayerJumpState erişebilsin
    [SerializeField] public LayerMask groundLayer;

    public bool isDoubleJump = false;

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
        if (IsGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float absVelY = Mathf.Abs(_rb.linearVelocityY);

        if (_rb.linearVelocityY < 0)
        {
            // Düşüş — daha ağır
            _rb.gravityScale = defaultGravity * fallMultiplier;
        }
        else if (absVelY < apexThreshold && !IsGrounded())
        {
            // Zirvede süzülme — hafif yerçekimi
            _rb.gravityScale = defaultGravity * apexGravityMultiplier;
        }
        else
        {
            _rb.gravityScale = defaultGravity;
        }
    }

    /// <summary>
    /// Zıplama tuşu erken bırakıldığında çağrılır — yukarı hızı keser.
    /// Sonuç: kısa basış = küçük zıplama, uzun basış = tam zıplama.
    /// </summary>
    public void CutJump()
    {
        if (!enableVariableJumpHeight) return;
        if (_rb.linearVelocityY > 0f)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * jumpCutMultiplier);
    }

    public void Move()
    {
        float moveInput = _playerInputHandler.GetMoveDirection();
        float targetSpeed = moveInput * maxSpeed;
        float speedDif = targetSpeed - _rb.linearVelocityX;
        float accelRate;

        if (Mathf.Abs(targetSpeed) > 0.01f)
            accelRate = (Mathf.Sign(targetSpeed) != Mathf.Sign(_rb.linearVelocityX)
                         && Mathf.Abs(_rb.linearVelocityX) > 0.1f)
                        ? turnSpeed : acceleration;
        else
            accelRate = deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        _rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (Mathf.Abs(moveInput) < 0.01f)
            ApplyFriction();
    }

    public void ApplyFriction()
    {
        float amount = Mathf.Min(Mathf.Abs(_rb.linearVelocityX),
                                 Mathf.Abs(deceleration * Time.fixedDeltaTime));
        amount *= Mathf.Sign(_rb.linearVelocityX);
        _rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
    }

    public void Jump()
    {
        _rb.linearVelocityY = 0;
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpBufferCounter = 0f;
    }

    /// <summary>
    /// OverlapBox ile zemin kontrolü — OverlapCircle'dan daha geniş alan tarar.
    /// Karakter platform kenarına oturduğunda da doğru sonuç verir.
    ///
    /// groundCheckWidth: karakterin collider genişliğiyle eşleştir (Inspector'dan ayarla)
    /// groundCheckRadius: kutunun yarı yüksekliği (ince tutulmalı)
    /// </summary>
    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(
            groundCheckTransform.position,
            new Vector2(groundCheckWidth, groundCheckRadius * 2f),
            0f,
            groundLayer);
    }

    /// <summary>
    /// Ground check transform'un dünya pozisyonunu döndürür.
    /// PlayerJumpState'in raycast kontrolü için kullanılır.
    /// </summary>
    public Vector2 GroundCheckPosition()
        => groundCheckTransform.position;

    public bool IsTouchingWall(float facingDirection)
        => Physics2D.Raycast(
            wallCheckTransform.position,
            Vector2.right * facingDirection,
            wallCheckDistance,
            groundLayer);

    private void OnDrawGizmos()
    {
        if (groundCheckTransform != null)
        {
            // Box gizmo — sahne görünümünde tarama alanını gösterir
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                groundCheckTransform.position,
                new Vector3(groundCheckWidth, groundCheckRadius * 2f, 0f));
        }

        if (wallCheckTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                wallCheckTransform.position,
                wallCheckTransform.position + (Vector3.right * wallCheckDistance));
        }
    }
}