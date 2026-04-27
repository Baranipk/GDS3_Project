using UnityEngine;

public class PlayerJumpState : IplayerState
{
    PlayerController controller;
    PlayerMovement playerMovement;
    Rigidbody2D rigidbody;
    PlayerAnimation playerAnimation;

    AudioSource audioSource;

    private bool isFalling = false;

    public PlayerJumpState(PlayerController controller)
    {
        this.controller = controller;
        playerMovement = controller.gameObject.GetComponent<PlayerMovement>();
        rigidbody = controller.gameObject.GetComponent<Rigidbody2D>();
        playerAnimation = controller.gameObject.GetComponent<PlayerAnimation>();

    }
    public void Enter()
    {
        if (playerMovement.IsGrounded() || playerMovement.isDoubleJump)
        {
            // SoundManager.Instance?.Get("Jump")?.Play(); şeklinde güvenli çağrı
            var jumpSound = SoundManager.Instance?.Get("Jump");
            if (jumpSound != null) jumpSound.Play();

            if (playerMovement.isDoubleJump && !playerMovement.IsGrounded())
            {
                playerMovement.isDoubleJump = false;
            }
            playerMovement.Jump();
            playerAnimation.SetAnimationJump();
        }
        isFalling = false;
    }
    public void Exit() { }
    public void FixedUpdate() { playerMovement.Move(); }
    public void Update()
    {

        if (rigidbody.linearVelocityY > 0)
        {
            // Debug.Log("Jump");

        }
        else
        {
            isFalling = true;
            playerAnimation.SetAnimationFall();

        }

        if (playerMovement.IsGrounded() && isFalling)
        {
            SoundManager.Instance.Get("Fall")?.Play();
            controller.playerStateMachine.ChangeState(controller.idleState);
        }
    }
}
