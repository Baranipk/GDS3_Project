using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerInputActions _playerInput;
    private PlayerController _playerController;

    private void Awake()
    {
        _playerInput = new PlayerInputActions();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _playerInput.Player.Enable();
        _playerInput.Player.Jump.performed += JumpPressed;
        _playerInput.Player.Pause.performed += PausePressed;
        _playerInput.Player.Throw.performed += ThrowPressed;
        _playerInput.Player.Attack.performed += AttackPressed;

        _playerInput.Player.Block.performed += BlockPressed;
        _playerInput.Player.Block.canceled += BlockPressed;
    }

    private void OnDisable()
    {
        _playerInput.Player.Disable();
        _playerInput.Player.Jump.performed -= JumpPressed;
        _playerInput.Player.Pause.performed -= PausePressed;
        _playerInput.Player.Throw.performed -= ThrowPressed;
        _playerInput.Player.Attack.performed -= AttackPressed;

        _playerInput.Player.Block.performed -= BlockPressed;
        _playerInput.Player.Block.canceled -= BlockPressed;
    }


    public float GetMoveDirection()
    {
        Vector2 moveDirection = _playerInput.Player.Move.ReadValue<Vector2>();

        return moveDirection.x;
    }

    public void JumpPressed(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            _playerController.playerStateMachine.ChangeState(_playerController.jumpState);
        }    
    }

    public void PausePressed(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            // EventBus<OnPausePressed>.Publish(new OnPausePressed());
        }
    }

    public void ThrowPressed(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Debug.Log("Throw Pressed!");
        }
    }

    public void AttackPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var attackScript = _playerController.GetComponent<PlayerAttack>();

            
            if (attackScript.CanAttack())
            {
                _playerController.playerStateMachine.ChangeState(_playerController.attackState);
            }
        }
    }

    public void BlockPressed(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            Debug.Log("Block Pressed");
        }
        else
        {
            Debug.Log("Block Realesed");
        }
    }


    public void ActivateInput()
    {
        _playerInput.Player.Enable();
    }

    public void DeactivateInput()
    {
        _playerInput.Player.Disable();
    }
}
