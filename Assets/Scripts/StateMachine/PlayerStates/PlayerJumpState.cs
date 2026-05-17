using UnityEngine;

public class PlayerJumpState : IplayerState
{
    PlayerController controller;
    PlayerMovement playerMovement;
    Rigidbody2D rigidbody;
    PlayerAnimation playerAnimation;

    private bool isFalling = false;
    private float _landingTimer = 0f;

    // Ayak altından aşağıya atılan raycast mesafesi
    // Bu değeri Inspector'dan görmek istersen PlayerMovement'a taşıyabilirsin
    private const float RaycastDistance = 0.4f;

    // Platforma snap kuvveti
    private const float SnapDownForce = 3f;

    // Yakın platform tespit edilince kaç saniye sonra iniş sayılsın
    private const float LandingGrace = 0.08f;

    public PlayerJumpState(PlayerController controller)
    {
        this.controller = controller;
        playerMovement = controller.gameObject.GetComponent<PlayerMovement>();
        rigidbody = controller.gameObject.GetComponent<Rigidbody2D>();
        playerAnimation = controller.gameObject.GetComponent<PlayerAnimation>();
    }

    public void Enter()
    {
        isFalling = false;
        _landingTimer = 0f;

        if (playerMovement.IsGrounded() || playerMovement.isDoubleJump)
        {
            var jumpSound = SoundManager.Instance?.Get("Jump");
            if (jumpSound != null) jumpSound.Play();

            if (playerMovement.isDoubleJump && !playerMovement.IsGrounded())
                playerMovement.isDoubleJump = false;

            playerMovement.Jump();
            playerAnimation.SetAnimationJump();
        }
    }

    public void Exit() { }

    public void FixedUpdate()
    {
        playerMovement.Move();
    }

    public void Update()
    {
        float velY = rigidbody.linearVelocityY;

        // ── Zirve / düşüş tespiti ──────────────────────────────
        if (velY > 0.1f)
        {
            isFalling = false;
            _landingTimer = 0f;
        }
        else
        {
            isFalling = true;
            playerAnimation.SetAnimationFall();
        }

        if (!isFalling) return;

        // ── 1. Normal iniş ─────────────────────────────────────
        if (playerMovement.IsGrounded())
        {
            Land();
            return;
        }

        // ── 2. Raycast ile platform kenarı tespiti ─────────────
        // Raycast OverlapCircle'dan daha güvenilir:
        // dar bir çizgi boyunca aşağıya bakar, platform kenarını yakalar.
        if (IsGroundBelow())
        {
            // Platforma doğru bastır — havada asılı kalmayı önler
            if (rigidbody.linearVelocityY > -SnapDownForce)
                rigidbody.linearVelocity = new Vector2(
                    rigidbody.linearVelocity.x, -SnapDownForce);

            _landingTimer += Time.deltaTime;
            if (_landingTimer >= LandingGrace)
            {
                Land();
                return;
            }
        }
        else
        {
            _landingTimer = 0f;
        }
    }

    // ─────────────────────────────────────────────────────────── 
    /// <summary>
    /// Karakterin ayak noktasından aşağıya RaycastDistance kadar ışın atar.
    /// Platform effector dahil her türlü ground layer'ı yakalar.
    /// OverlapCircle'dan farklı olarak kenar noktalarda da güvenilir çalışır.
    /// </summary>
    private bool IsGroundBelow()
    {
        // Raycast başlangıç noktası: karakterin merkezi
        Vector2 origin = rigidbody.position;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            RaycastDistance,
            playerMovement.groundLayer);

        // Debug: sahne görünümünde ışını görmek için (isteğe bağlı)
        // Debug.DrawRay(origin, Vector2.down * RaycastDistance, hit ? Color.green : Color.red);

        return hit.collider != null;
    }

    private void Land()
    {
        _landingTimer = 0f;
        SoundManager.Instance?.Get("Fall")?.Play();
        controller.playerStateMachine.ChangeState(controller.idleState);
    }
}