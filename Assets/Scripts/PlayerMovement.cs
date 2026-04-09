using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float maxSpeed = 12f;      // Ulaşılacak maksimum hız
    public float acceleration = 10f;  // Hızlanma katsayısı (0-max arası geçiş hızı)
    public float deceleration = 20f;  // Yavaşlama/Durma katsayısı (Tuşu bırakınca)
    public float turnSpeed = 15f;     // Ani dönüş katsayısı (Sağa giderken sola basınca)

    [Header("İnce Ayar")]
    public float velPower = 0.9f;
    [SerializeField] private float jumpForce = 10;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;

    public bool isDoubleJump = false;

    [Header("Double Jump Settings")]
    //[SerializeField] private int maxJumpCount = 2; // Kaç kez zıplanabilir? (2 = Double Jump)
    private int _remainingJumps; // Kalan zıplama hakkı

    public PlayerInputHandler _playerInputHandler;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private bool _isGrounded;

    private void Start()
    {
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }
    public void Move()
    {
        // 1. Input Al
        float moveInput = _playerInputHandler.GetMoveDirection();

        // 2. Hedef Hızı Hesapla
        // Tuşa basılıyorsa maxSpeed, basılmıyorsa 0 hedeflenir.
        float targetSpeed = moveInput * maxSpeed;

        // 3. Hız Farkını Bul (Delta)
        // Hedeflediğimiz hız ile şu anki hızımız arasındaki fark
        float speedDif = targetSpeed - _rb.linearVelocityX;

        // 4. Hangi Katsayıyı Kullanacağız? (Hızlanma mı, Yavaşlama mı, Dönüş mü?)
        float accelRate;

        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            // Hareket ediyoruz...
            // Eğer hedef yön ile şu anki hız yönü zıtsa (Dönüş yapıyoruz)
            if (Mathf.Sign(targetSpeed) != Mathf.Sign(_rb.linearVelocityX) && Mathf.Abs(_rb.linearVelocityX) > 0.1f)
            {
                accelRate = turnSpeed; // Dönüşler daha keskin olsun
            }
            else
            {
                accelRate = acceleration; // Normal hızlanma
            }
        }
        else
        {
            // Tuşa basmıyoruz, durmak istiyoruz
            accelRate = deceleration;
        }

        // 5. Kuvveti Hesapla ve Uygula
        // Fark * HızlanmaGücü formülü (P-Controller mantığı gibi çalışır)
        // Mathf.Pow kullanımı hareketin başlangıcını daha yumuşak yapar (opsiyoneldir)
        // 5. Kuvveti Hesapla ve Uygula
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        _rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        // --- YENİ EKLEME: Durma Sürtünmesi ---
        // Eğer tuşa basmıyorsak ve karakter hala hareket ediyorsa hızı hızla kes
        if (Mathf.Abs(moveInput) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(_rb.linearVelocityX), Mathf.Abs(deceleration * Time.fixedDeltaTime));
            amount *= Mathf.Sign(_rb.linearVelocityX);
            _rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        // 6. Görsel Çevirme (Flip)
        if (moveInput != 0)
        {
            _sr.flipX = moveInput < 0;
        }
    }
    public void Jump()
    {
        _rb.linearVelocityY = 0;
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);

    }

    private void OnDrawGizmos()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
    }
}
