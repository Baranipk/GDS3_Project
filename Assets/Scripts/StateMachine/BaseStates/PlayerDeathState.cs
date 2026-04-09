using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerDeathState : IplayerState
{
    PlayerController controller;
    SpriteRenderer spriteRenderer;
    Material _material;

    private static readonly int DissolveAmountID = Shader.PropertyToID("_DisolveAmount");

    public PlayerDeathState(PlayerController controller)
    {
        this.controller = controller;
        spriteRenderer = controller.GetComponent<SpriteRenderer>();
        _material = spriteRenderer.material;
    }

    public async void Enter()
    {
        controller.GetComponent<PlayerAnimation>().Death();
        controller.GetComponent<PlayerInputHandler>().DeactivateInput();
        controller.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
        SoundManager.Instance.Get("Death").Play();
       /* //Disolve Effect
         await DOTween.To(() => _material.GetFloat(DissolveAmountID),
                 x => _material.SetFloat(DissolveAmountID, x), 1.1f, 1.5f)
             .ToUniTask();
        await UniTask.Delay(1000);
        Time.timeScale = 0f;
        LevelManager.Instance.HandlePlayerDeath().Forget(); */
    } 

    public void Exit() { }

    public void FixedUpdate() { }

    public void Update() { }


}
