using UnityEngine;

public class ZagreusAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetWalk(bool isWalking)
    {
        if (animator == null) return;
        animator.SetBool("IsWalk", isWalking);
    }

    public void PlayAttack1()    { if (animator != null) animator.SetTrigger("Attack1"); }
    public void PlayAttack2()    { if (animator != null) animator.SetTrigger("Attack2"); }
    public void PlayAttack3()    { if (animator != null) animator.SetTrigger("Attack3"); }
    public void PlayChainCombo() { if (animator != null) animator.SetTrigger("ChainCombo"); }
    public void PlayBackDash()   { if (animator != null) animator.SetTrigger("BackDash"); }
    public void PlayHurt()       { if (animator != null) animator.SetTrigger("Hurt"); }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.ResetTrigger("Hurt");
        ResetAllAttackTriggers();
        animator.SetTrigger("Death");
    }

    public void ResetAllAttackTriggers()
    {
        if (animator == null) return;
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.ResetTrigger("ChainCombo");
        animator.ResetTrigger("BackDash");
    }
}
