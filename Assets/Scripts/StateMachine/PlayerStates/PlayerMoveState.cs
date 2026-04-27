using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoveState : IplayerState
{
    PlayerMovement _playerMovement;
    Rigidbody2D rigidbody;
    PlayerController Controller;
    PlayerAnimation playerAnimation;

    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f; // Örn: Her 0.3 saniyede bir adım sesi (Animasyon hızına göre ayarla)
    public PlayerMoveState(PlayerController controller)
    {
        Controller = controller;
        _playerMovement = controller.gameObject.GetComponent<PlayerMovement>();
        rigidbody = controller.gameObject.GetComponent<Rigidbody2D>();
        playerAnimation = controller.gameObject.GetComponent<PlayerAnimation>();

    }
    public void Enter()
    {
        playerAnimation.SetAnimationWalk();

    }
    public void Exit()
    {

    }
    public void FixedUpdate()
    {
        _playerMovement.Move();
    }
    public void Update()
    {
        if (_playerMovement.IsGrounded())
        {
             HandleFootsteps();
        }

        if (_playerMovement.jumpBufferCounter > 0f && _playerMovement.coyoteTimeCounter > 0f)
        {
            Controller.playerStateMachine.ChangeState(Controller.jumpState);
            return;
        }

        // DUVARA DAYANMA ÇÖZÜMÜ: Yürüyoruz ama duvara çarptıysak Idle'a dön!
        if (_playerMovement.IsTouchingWall(Controller.FacingDirection) && _playerMovement.IsGrounded())
        {
            Controller.playerStateMachine.ChangeState(Controller.idleState);
            return;
        }

        if (Mathf.Abs(rigidbody.linearVelocity.x) <= 0.01f)
        {
            //rigidbody.linearVelocity = Vector3.zero;
            Controller.playerStateMachine.ChangeState(Controller.idleState);
        }
    }

     private void HandleFootsteps()
    {
        footstepTimer -= Time.deltaTime; // Süreyi azalt

        if (footstepTimer <= 0)
        {
            // SoundManager üzerinden sesi çek
            Sound s = SoundManager.Instance.Get("Walk"); // Buradaki "Footstep" ismini kendi ses isminle değiştir

            if (s != null)
            {
                // Rastgelelik ekle (0.8 ile 1.2 arası pitch)
                float randomPitch = Random.Range(0.8f, 1.2f);
                float randomVolume = Random.Range(0.8f, 1.0f); // Hafif ses şiddeti değişimi de doğallık katar

                // Sesi ayarla ve çal (PlayOneShot üst üste binmeye izin verir)
                s.SetPitch(randomPitch)
                 .SetVolume(randomVolume)
                 .PlayOneShot();
            }

            // Sayacı tekrar kur
            footstepTimer = footstepInterval; 
        } 
    } 
}
