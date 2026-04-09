using UnityEngine;

public class PlayerIdleState : IplayerState
{
    Animator animator;
    PlayerController controller;
    PlayerInputHandler inputHandler;
    PlayerAnimation playerAnimation;

    public PlayerIdleState(PlayerController controller)
    {
        this.controller = controller;

        animator = controller.gameObject.GetComponent<Animator>();
        inputHandler = controller.gameObject.GetComponent<PlayerInputHandler>();
        playerAnimation = controller.gameObject.GetComponent<PlayerAnimation>();

    }
    public void Enter()
    {

        playerAnimation.SetAnimationIdle();

    }

    public void Exit()
    {

    }

    public void FixedUpdate() { }

    public void Update()
    {
        if (inputHandler.GetMoveDirection() != 0)
        {
            controller.playerStateMachine.ChangeState(controller.moveState);
        }
    }
}
