using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        // Eğer Animator child objesindeyse GetComponentInChildren kullanabilirsin
        animator = GetComponentInChildren<Animator>();
    }

    public void PlayHurt()
    {
        if (animator == null) return;
        animator.SetTrigger("Hurt");
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.SetTrigger("Death");
    }

    public void SetWalk(bool isWalking)
    {
        if (animator == null) return;
        animator.SetBool("IsWalk", isWalking);
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.SetTrigger("Attack");
    }

    // Gerekirse tüm tetikleyicileri sıfırlamak için
    public void ResetAllTriggers()
    {
        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("Death");
        animator.ResetTrigger("Attack");
    }
}