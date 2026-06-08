public class ZagreusAttack1State : ZagreusAttackStateBase
{
    public ZagreusAttack1State(ZagreusController boss) : base(boss) { }
    protected override ZagreusAttackData Data => boss.attack1Data;
    protected override void PlayAnimation() => boss.zagAnim?.PlayAttack1();
    protected override void PlaySound() => SoundManager.Instance?.TryPlayOneShot(boss.attack1SoundName);
}
