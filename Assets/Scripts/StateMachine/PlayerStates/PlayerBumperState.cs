using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerBumperState : IplayerState
{
    PlayerController controller;
    PlayerAnimation pAnim;
    Rigidbody2D rigidbody;
    public PlayerBumperState(PlayerController controller)
    {
        this.controller = controller;
        rigidbody = controller.GetComponent<Rigidbody2D>();
        pAnim = controller.GetComponent<PlayerAnimation>();

    }

    public async void Enter()
    {
        await UniTask.Delay(400);
        pAnim.Bump();
        controller.playerStateMachine.ChangeState(controller.moveState);
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {

    }
}
