using UnityEngine;

/// <summary>
/// Animator child objesindeyken Animation Event'lerinin
/// parent'taki BossController'a iletilmesi için köprü.
/// Bu script, Animator ile AYNI GameObject'e eklenmelidir.
/// </summary>
public class BossAnimationEventRelay : MonoBehaviour
{
    private BossController _boss;

    private void Awake()
    {
        _boss = GetComponentInParent<BossController>();
        if (_boss == null)
            Debug.LogError("[BossAnimationEventRelay] Parent'ta BossController bulunamadı!", this);
    }

    // Animation Event'lerinin Function listesinde bu isimler görünecek
    public void AnimEvent_MeleeHit()          => _boss?.AnimEvent_MeleeHit();
    public void AnimEvent_SpawnProjectile()   => _boss?.AnimEvent_SpawnProjectile();
    public void AnimEvent_AttackComplete()    => _boss?.AnimEvent_AttackComplete();
    public void AnimEvent_IntroComplete()     => _boss?.AnimEvent_IntroComplete();
    public void AnimEvent_DeathComplete()     => _boss?.AnimEvent_DeathComplete();
    public void AnimEvent_Footstep()          => _boss?.AnimEvent_Footstep();
}
