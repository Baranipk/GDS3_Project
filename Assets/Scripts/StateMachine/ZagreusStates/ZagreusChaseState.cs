using UnityEngine;

public class ZagreusChaseState : IBossState
{
    private readonly ZagreusController boss;
    private float enterTime;
    private float nextThinkTime;
    private float lastDebugTime;
    private bool postComboChecked;
    private float footstepTimer;

    public ZagreusChaseState(ZagreusController boss) { this.boss = boss; }

    public void Enter()
    {
        enterTime = Time.time;
        nextThinkTime = Time.time + 0.1f;
        postComboChecked = false;
        footstepTimer = 0f;
        boss.zagAnim?.SetWalk(false);
    }

    public void Update()
    {
        if (boss.player == null) return;
        boss.FaceTarget(boss.player.position);

        float dist = Vector2.Distance(boss.transform.position, boss.player.position);
        float chaseElapsed = Time.time - enterTime;

        if (Time.time - lastDebugTime > 0.8f)
        {
            lastDebugTime = Time.time;
            Debug.Log($"[Zagreus] dist={dist:F2} sinceCombo={Time.time - boss.lastComboEndTime:F2}", boss);
        }

        // ── 1) Combo'dan henüz çıktıysa: post-combo backdash kontrolü (bir kez) ─
        if (!postComboChecked && Time.time - boss.lastComboEndTime < 0.2f)
        {
            postComboChecked = true;
            if (Random.value < boss.backDashChance_PostCombo)
            {
                boss.StateMachine.ChangeState(boss.backDashState);
                return;
            }
        }

        // ── 2) Think tick'i bekle ───────────────────────────────────────────────
        if (Time.time < nextThinkTime) return;
        nextThinkTime = Time.time + boss.patternThinkInterval;

        // ── 3) Combo cooldown ───────────────────────────────────────────────────
        if (chaseElapsed < boss.postComboRest) return;

        // ── 4) Çok yakın & combo seçilmediyse: defensive backdash ───────────────
        if (dist < boss.tooCloseDistance && Random.value < boss.backDashChance_TooClose)
        {
            boss.StateMachine.ChangeState(boss.backDashState);
            return;
        }

        // ── 5) Combo karar tablosu — sırayla dene, ilk uyan tetiklenir ──────────
        foreach (var entry in boss.comboTable)
        {
            if (dist < entry.minDistance || dist > entry.maxDistance) continue;
            if (Random.value < entry.chance)
            {
                boss.StartCombo(entry.combo);
                return;
            }
        }

        // ── 6) Random backdash (chase tick'inde nadir) ──────────────────────────
        if (Random.value < boss.backDashChance_Random)
        {
            boss.StateMachine.ChangeState(boss.backDashState);
            return;
        }
    }

    public void FixedUpdate()
    {
        if (boss.player == null || boss.rb == null) return;

        float dx = boss.player.position.x - boss.transform.position.x;
        float absDx = Mathf.Abs(dx);
        float dirToPlayer = Mathf.Sign(dx);

        Vector2 vel = boss.rb.linearVelocity;

        if (absDx > boss.preferredRange + boss.rangeBuffer)
        {
            // Çok uzak → yaklaş
            vel.x = dirToPlayer * boss.moveSpeed;
            boss.zagAnim?.SetWalk(true);
        }
        else if (absDx < boss.tooCloseDistance)
        {
            // Çok yakın → geri çekil (backdash atılamadıysa pasif geri çekilme)
            vel.x = -dirToPlayer * boss.backstepSpeed;
            boss.zagAnim?.SetWalk(true);
        }
        else if (absDx < boss.preferredRange - boss.rangeBuffer)
        {
            // Sweet spot'un altında, hafif geri
            vel.x = -dirToPlayer * (boss.backstepSpeed * 0.6f);
            boss.zagAnim?.SetWalk(true);
        }
        else
        {
            // Sweet spot — dur
            vel.x = 0f;
            boss.zagAnim?.SetWalk(false);
        }

        boss.rb.linearVelocity = vel;

        // Footstep: hareket varsa timer'ı azalt, 0'a düşünce adım sesi çal
        if (Mathf.Abs(vel.x) > 0.05f)
        {
            footstepTimer -= Time.fixedDeltaTime;
            if (footstepTimer <= 0f)
            {
                boss.PlayFootstep();
                footstepTimer = boss.footstepInterval;
            }
        }
        else
        {
            // Durdu — bir sonraki ilk adım hemen çalsın
            footstepTimer = 0f;
        }
    }

    public void Exit()
    {
        if (boss.rb != null)
            boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        boss.zagAnim?.SetWalk(false);
    }
}
