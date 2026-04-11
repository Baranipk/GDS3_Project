using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator animator;

    public void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void SetAnimationWalk()
    {
        animator.SetBool("IsFall", false);
        animator.SetBool("IsWalk", true);
    }

    public void SetAnimationIdle()
    {
        animator.SetBool("IsFall", false);
        animator.SetBool("IsWalk", false);
    }

    public void SetAnimationJump()
    {
        animator.SetTrigger("Jump");
        animator.SetBool("IsFall", false);
    }

    public void SetAnimationFall()
    {
        animator.SetBool("IsFall", true);
    }

    public void Death()
    {
        animator.SetTrigger("Death");
    }

    public void Bump()
    {
        animator.SetTrigger("Bump");
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayBlockStart()
    {
        Debug.Log("1. PlayBlockStart metodu tetiklendi!");

        if (animator == null)
        {
            Debug.LogError("HATA: Animator (anim) referansı boş! Bu yüzden Trigger çalışmıyor.");
            return; // Hata varsa aşağıya inme
        }

        animator.SetTrigger("BlockTrigger");
        animator.SetBool("isBlocking", true);

        Debug.Log("2. Trigger ve Bool başarıyla Animator'a gönderildi!");
    }

    public void StopBlock()
    {     
        animator.SetBool("isBlocking", false);
    }

    public void PlayThrow()
    {
        animator.SetTrigger("ThrowTrigger");
    }

    public void PlayIdleForce()
    {
        if (animator == null) return;

        
        animator.ResetTrigger("Death");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("ThrowTrigger");
        animator.ResetTrigger("BlockTrigger");

        
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsFall", false);
        animator.SetBool("isBlocking", false);

        
        animator.Play("IdleAnimation", 0, 0f);
    }
}

