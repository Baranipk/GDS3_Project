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

        // 2. Sürtünmeyi artır (Karakterin kaymasını engeller)
        rigidbody.linearDamping = 20f;

        // --- YENİ: SALDIRI SESİNİ ÇAL ---
        // SoundManager'daki ses listesinde adı "Attack" olan sesi bul ve çal.
        // ?. kullanımı sayesinde eğer ses bulunamazsa oyunun çökmesini engelleriz.
        SoundManager.Instance?.Get("Attack")?.PlayOneShot();

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

    public void FixedUpdate() { }

    public void Update() { }
}