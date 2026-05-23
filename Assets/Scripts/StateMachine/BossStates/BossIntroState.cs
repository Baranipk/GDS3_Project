using UnityEngine;

public class BossIntroState : IBossState
{
    private readonly BossController boss;

    public BossIntroState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        boss.IsInvulnerable = true;
        if (boss.rb != null) boss.rb.linearVelocity = Vector2.zero;
        boss.bossAnim.SetWalk(false);
        boss.bossAnim.PlayIntro();

        SoundManager.Instance?.TryPlayOneShot(boss.introSoundName);
    }

    public void Update() { }
    public void FixedUpdate() { }

    public void Exit()
    {
        boss.IsInvulnerable = false;
    }
}
