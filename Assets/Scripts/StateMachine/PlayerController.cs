using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine playerStateMachine { get; set; }
    public float FacingDirection { get; private set; } = 1f; // Başlangıçta sağa bakıyor
    private PlayerInputHandler _inputHandler;

    public PlayerIdleState idleState;
    public PlayerMoveState moveState;
    public PlayerJumpState jumpState;
    public PlayerDeathState deathState;
    public PlayerBumperState bumperState;
    public PlayerAttackState attackState;
    public PlayerBlockState blockState;
    public PlayerThrowState throwState;
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
    }

    private void Start()
    {
        gameObject.transform.position = GameObject.FindGameObjectWithTag("StartPosition").transform.position;
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

    private void Flip(float newDirection)
    {
        FacingDirection = newDirection;
        Vector3 scale = transform.localScale;
        scale.x = newDirection; // Karakteri görsel olarak aynala
        transform.localScale = scale;
    }
}



