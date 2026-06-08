public class ZagreusAttack2State : ZagreusAttackStateBase
{
    public ZagreusAttack2State(ZagreusController boss) : base(boss) { }
    protected override ZagreusAttackData Data => boss.attack2Data;
    protected override void PlayAnimation() => boss.zagAnim?.PlayAttack2();
    protected override void PlaySound() => SoundManager.Instance?.TryPlayOneShot(boss.attack2SoundName);
}
