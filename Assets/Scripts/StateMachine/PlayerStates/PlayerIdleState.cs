using UnityEngine;

public class PlayerIdleState : IplayerState
{
    Animator animator;
    PlayerController controller;
    PlayerInputHandler inputHandler;
    PlayerAnimation playerAnimation;
    PlayerMovement _playerMovement;

    public PlayerIdleState(PlayerController controller)
    {
        this.controller = controller;

        animator = controller.gameObject.GetComponent<Animator>();
        inputHandler = controller.gameObject.GetComponent<PlayerInputHandler>();
        playerAnimation = controller.gameObject.GetComponent<PlayerAnimation>();
        _playerMovement = controller.gameObject.GetComponent<PlayerMovement>();

    }
    public void Enter()
    {

        playerAnimation.SetAnimationIdle();

    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {
        _playerMovement.ApplyFriction();
    }

    public void Update()
    {
        if (_playerMovement.jumpBufferCounter > 0f && _playerMovement.coyoteTimeCounter > 0f)
        {
            controller.playerStateMachine.ChangeState(controller.jumpState);
            return;
        }

        if (inputHandler.GetMoveDirection() != 0)
        {
            controller.playerStateMachine.ChangeState(controller.moveState);
        }
    }
}
