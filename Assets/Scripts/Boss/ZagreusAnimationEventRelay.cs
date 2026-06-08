using UnityEngine;

/// <summary>
/// Animator child'daysa Animation Event'leri parent ZagreusController'a iletir.
/// Şu an Zagreus timer tabanlı — event'ler opsiyonel (ses tetikleme için kullanılabilir).
/// </summary>
public class ZagreusAnimationEventRelay : MonoBehaviour
{
    private ZagreusController _boss;

    private void Awake()
    {
        _boss = GetComponentInParent<ZagreusController>();
        if (_boss == null)
            Debug.LogError("[ZagreusAnimationEventRelay] Parent'ta ZagreusController bulunamadı!", this);
    }

    // Chain combo'daki hit'leri tetiklemek için (frame-perfect timing)
    public void AnimEvent_ChainHit1() { _boss?.AnimEvent_ChainHit1(); }
    public void AnimEvent_ChainHit2() { _boss?.AnimEvent_ChainHit2(); }

    // Opsiyonel — ses tetiklemek için
    public void Anim_PlayAttack1Sound()    { SoundManager.Instance?.TryPlayOneShot(_boss?.attack1SoundName); }
    public void Anim_PlayAttack2Sound()    { SoundManager.Instance?.TryPlayOneShot(_boss?.attack2SoundName); }
    public void Anim_PlayAttack3Sound()    { SoundManager.Instance?.TryPlayOneShot(_boss?.attack3SoundName); }
    public void Anim_PlayChainComboSound() { SoundManager.Instance?.TryPlayOneShot(_boss?.chainComboSoundName); }
    public void Anim_PlayBackDashSound()   { SoundManager.Instance?.TryPlayOneShot(_boss?.backDashSoundName); }
}
