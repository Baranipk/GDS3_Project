public class ZagreusAttack3State : ZagreusAttackStateBase
{
    public ZagreusAttack3State(ZagreusController boss) : base(boss) { }
    protected override ZagreusAttackData Data => boss.attack3Data;
    protected override void PlayAnimation() => boss.zagAnim?.PlayAttack3();
    protected override void PlaySound() => SoundManager.Instance?.TryPlayOneShot(boss.attack3SoundName);
}
