using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerAttackState : IplayerState
{
    PlayerController controller;
    PlayerAnimation pAnim;
    PlayerInputHandler inputHandler;
    Rigidbody2D rigidbody;
    PlayerAttack playerAttack;


    public PlayerAttackState(PlayerController controller)
    {
        this.controller = controller;
        rigidbody = controller.GetComponent<Rigidbody2D>();
        pAnim = controller.GetComponent<PlayerAnimation>();
        inputHandler = controller.GetComponent<PlayerInputHandler>();
        playerAttack = controller.GetComponent<PlayerAttack>();
    }
    public async void Enter()
    {
        pAnim.Attack(); // Karakterin vuruş animasyonunu oynatır
        playerAttack.PerformAttack(); // Efekti çıkarır ve cooldown'ı başlatır

        // Animasyonun veya vuruşun "kilit" süresi
        // Bu süreyi de attackSpeed'e oranlayabilirsin
        await UniTask.Delay(300);

        controller.playerStateMachine.ChangeState(controller.idleState);
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
