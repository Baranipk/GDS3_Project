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

    /// <summary>
    /// Ölüm animasyonunu oynatır.
    ///
    /// DÜZELTME: Önce tüm boolean ve trigger'ları sıfırla.
    /// Bunlar aktifken (IsWalk=true, IsFall=true vb.) Animator
    /// mevcut state'den çıkmayı reddediyordu — Death trigger'ı çalışmıyordu.
    /// </summary>
    public void Death()
    {
        // 1. Tüm boolean'ları sıfırla — bunlar aktifken Death geçişi bloke olur
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsFall", false);
        animator.SetBool("isBlocking", false);

        // 2. Bekleyen tüm trigger'ları temizle — bunlar Death trigger'ını "yiyebilir"
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("ThrowTrigger");
        animator.ResetTrigger("BlockTrigger");
        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("Bump");

        // 3. Şimdi Death trigger'ını set et — artık hiçbir şey engel değil
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
        if (animator == null)
        {
            Debug.LogError("[PlayerAnimation] Animator referansı boş!");
            return;
        }

        animator.SetTrigger("BlockTrigger");
        animator.SetBool("isBlocking", true);
    }

    public void StopBlock()
    {
        animator.SetBool("isBlocking", false);
    }

    public void PlayThrow()
    {
        animator.SetTrigger("ThrowTrigger");
    }

    public void PlayHurt()
    {
        animator.SetTrigger("Hurt");
    }

    /// <summary>
    /// Respawn sonrası Idle animasyonunu zorla oynatır.
    ///
    /// DÜZELTME: animator.Play() çağrısından önce tüm state temizleniyor.
    /// Death animasyonu bitişi bazen Idle'a geçişi engelliyordu.
    /// </summary>
    public void PlayIdleForce()
    {
        if (animator == null) return;

        // Tüm trigger'ları sıfırla
        animator.ResetTrigger("Death");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("ThrowTrigger");
        animator.ResetTrigger("BlockTrigger");
        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("Bump");

        // Tüm boolean'ları sıfırla
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsFall", false);
        animator.SetBool("isBlocking", false);

        // Idle animasyonunu baştan zorla oynat
        animator.Play("IdleAnimation", 0, 0f);
    }
}