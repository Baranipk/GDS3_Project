using UnityEngine;

public class BossChaseState : IBossState
{
    private readonly BossController boss;

    private float _enterTime;
    private float _nextThinkTime;
    private float _lastDebugTime;

    private const float POST_ATTACK_REST = 0.4f;

    // Boss "rush" modunda mı? — melee için hızlıca yaklaşıyor
    private bool _rushingForMelee;
    private float _rushEndTime;
    private const float MAX_RUSH_DURATION = 2.5f; // 2.5 sn'de melee'ye ulaşamazsa vazgeçer

    private bool _comboPending;

    public BossChaseState(BossController boss) { this.boss = boss; }

    public void Enter()
    {
        _enterTime = Time.time;
        _nextThinkTime = Time.time + 0.1f;
        boss.bossAnim.SetWalk(false);

        if (_comboPending) _nextThinkTime = Time.time + 0.05f;
    }

    public void Update()
    {
        if (boss.player == null) return;

        boss.FaceTarget(boss.player.position);

        float dist = Vector2.Distance(boss.transform.position, boss.player.position);
        bool meleeReady = Time.time >= boss.lastMeleeTime + boss.meleeCooldown;
        bool rangedReady = Time.time >= boss.lastRangedTime + boss.rangedCooldown;
        float chaseElapsed = Time.time - _enterTime;

        // Rush süresi doldu mu?
        if (_rushingForMelee && Time.time >= _rushEndTime) _rushingForMelee = false;

        if (Time.time - _lastDebugTime > 0.7f)
        {
            _lastDebugTime = Time.time;
            Debug.Log($"[Boss] dist={dist:F2} | melee={meleeReady} | ranged={rangedReady} | rush={_rushingForMelee} | combo={_comboPending}", boss);
        }

        // ─── ANINDA MELEE: menzildeyse ve hazırsa düşünmeden vur ───
        if (dist <= boss.meleeMaxDistance && meleeReady)
        {
            _rushingForMelee = false;
            _comboPending = (!_comboPending) && (Random.value < boss.comboChance);
            boss.StateMachine.ChangeState(boss.meleeState);
            return;
        }

        // Karar verme — sadece belli aralıklarla
        if (Time.time < _nextThinkTime) return;
        _nextThinkTime = Time.time + boss.patternThinkInterval;

        if (chaseElapsed < POST_ATTACK_REST && !_comboPending) return;

        // ─── RANGED kararı ─────────────────────────────────────
        if (rangedReady && dist >= boss.rangedMinDistance && !_rushingForMelee)
        {
            float threshold = boss.aggressiveness;
            if (dist > boss.preferredRange) threshold += 0.2f;

            if (Random.value < threshold)
            {
                _comboPending = (!_comboPending) && (Random.value < boss.comboChance);
                boss.StateMachine.ChangeState(boss.rangedState);
                return;
            }
        }

        // ─── RUSH kararı ───────────────────────────────────────
        // Melee hazırsa ve henüz rush'ta değilsek, agresifliğe göre rush başlat
        if (meleeReady && !_rushingForMelee && dist > boss.meleeMaxDistance)
        {
            // Rush şansı: yakındaysa daha yüksek, agresiflik etkili
            float rushChance = boss.aggressiveness * 0.7f;
            if (dist < boss.preferredRange) rushChance += 0.25f;   // yakındaysa sık rush
            if (!rangedReady) rushChance += 0.3f;                  // ranged cooldown'da ise mecbur kal

            if (Random.value < rushChance)
            {
                _rushingForMelee = true;
                _rushEndTime = Time.time + MAX_RUSH_DURATION;
                Debug.Log("[Boss] Rush başladı — melee için yaklaşıyor!", boss);
            }
        }

        _comboPending = false;
    }

    public void FixedUpdate()
    {
        if (boss.player == null || boss.rb == null) return;

        float dx = boss.player.position.x - boss.transform.position.x;
        float absDx = Mathf.Abs(dx);
        float dirToPlayer = Mathf.Sign(dx);
        bool meleeReady = Time.time >= boss.lastMeleeTime + boss.meleeCooldown;

        Vector2 vel = boss.rb.linearVelocity;

        // ─── HAREKET MODU SEÇİMİ ─────────────────────────────
        if (_rushingForMelee)
        {
            // Rush: tam hızla player'a yaklaş (melee menziline kadar)
            if (absDx > boss.meleeMaxDistance * 0.8f)
            {
                vel.x = dirToPlayer * boss.moveSpeed * 1.2f; // %20 hızlı
                boss.bossAnim.SetWalk(true);
            }
            else
            {
                // Melee menzilindeyiz — dur, Update melee'yi tetikleyecek
                vel.x = 0f;
                boss.bossAnim.SetWalk(false);
            }
        }
        else if (absDx < boss.tooCloseDistance && !meleeReady)
        {
            // Çok yakın AMA melee cooldown'da → geri çekil
            vel.x = -dirToPlayer * boss.backstepSpeed;
            boss.bossAnim.SetWalk(true);
        }
        else if (absDx < boss.tooCloseDistance && meleeReady)
        {
            // Çok yakın ve melee hazır → dur, Update'in melee'yi tetiklemesini bekle
            vel.x = 0f;
            boss.bossAnim.SetWalk(false);
        }
        else if (absDx > boss.preferredRange + boss.rangeBuffer)
        {
            // Çok uzak → yaklaş
            vel.x = dirToPlayer * boss.moveSpeed;
            boss.bossAnim.SetWalk(true);
        }
        else if (absDx < boss.preferredRange - boss.rangeBuffer)
        {
            // Sweet spot'un altında ama tooClose'un üstünde → hafif geri çekil
            vel.x = -dirToPlayer * (boss.backstepSpeed * 0.6f);
            boss.bossAnim.SetWalk(true);
        }
        else
        {
            // Sweet spot — dur
            vel.x = 0f;
            boss.bossAnim.SetWalk(false);
        }

        boss.rb.linearVelocity = vel;
    }

    public void Exit()
    {
        if (boss.rb != null)
            boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        boss.bossAnim.SetWalk(false);
        _rushingForMelee = false;
    }
}
