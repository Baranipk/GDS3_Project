using UnityEngine;

public class BossAnimation : MonoBehaviour
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

    public void PlayIntro()
    {
        if (animator == null) return;
        animator.SetTrigger("Intro");
    }

    public void PlayMelee()
    {
        if (animator == null) return;
        animator.SetTrigger("AttackMelee");
    }

    public void PlayRanged()
    {
        if (animator == null) return;
        animator.SetTrigger("AttackRanged");
    }

    public void PlayHurt()
    {
        if (animator == null) return;
        animator.SetTrigger("Hurt");
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("AttackMelee");
        animator.ResetTrigger("AttackRanged");
        animator.SetTrigger("Death");
    }

    public void ResetAllTriggers()
    {
        if (animator == null) return;
        animator.ResetTrigger("Intro");
        animator.ResetTrigger("AttackMelee");
        animator.ResetTrigger("AttackRanged");
        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("Death");
    }
}
