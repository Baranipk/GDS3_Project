using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerAttackState : IplayerState
{
    PlayerController controller;
    PlayerAnimation pAnim;
    PlayerInputHandler inputHandler;
    Rigidbody2D rigidbody;
    PlayerAttack playerAttack;

    private float originalDamping;

    public PlayerAttackState(PlayerController controller)
    {
        this.controller = controller;
        rigidbody = controller.GetComponent<Rigidbody2D>();
        pAnim = controller.GetComponent<PlayerAnimation>();
        inputHandler = controller.GetComponent<PlayerInputHandler>();
        playerAttack = controller.GetComponent<PlayerAttack>();
    }
    public void Enter()
    {
        // 1. Mevcut sürtünme değerini kaydet
        originalDamping = rigidbody.linearDamping;

        // 2. Sürtünmeyi uçur (Karakterin kaymasını engeller ama yerçekimini bozmaz)
        rigidbody.linearDamping = 20f;

        pAnim.Attack();
        playerAttack.PerformAttack();
        FinishAttack();
    }

    private async void FinishAttack()
    {
        await UniTask.Delay(300);
        controller.playerStateMachine.ChangeState(controller.idleState);
    }

    public void Exit()
    {
        rigidbody.linearDamping = originalDamping;
    }

    public void FixedUpdate()
    {
        
    }

    public void Update()
    {
       
    }
}
