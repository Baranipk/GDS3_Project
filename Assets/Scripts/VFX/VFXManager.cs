using UnityEngine;

/// <summary>
/// Tüm VFX'leri spawn eden merkezi manager.
/// Singleton — sahnede bir kez bulunur.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Karakter VFX Prefabları")]
    [SerializeField] private GameObject healVFXPrefab;
    [SerializeField] private GameObject shieldVFXPrefab;
    [SerializeField] private GameObject damageVFXPrefab;

    [Header("Düşman VFX Prefabları")]
    [SerializeField] private GameObject hitSparkVFXPrefab;

    [Header("Spawn Offset Ayarları")]
    [SerializeField] private float _healYOffset = 0.8f;
    [SerializeField] private float _damageYOffset = 0.5f;
    [SerializeField] private float _damageXRange = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Karakter VFX ──────────────────────────────────────────

    public void PlayHeal(Vector3 position, int amount = 1)
    {
        if (healVFXPrefab == null) { Debug.LogWarning("[VFXManager] healVFXPrefab atanmamış!"); return; }

        Vector3 spawnPos = position + new Vector3(0f, _healYOffset, 0f);
        GameObject vfxObj = Instantiate(healVFXPrefab, spawnPos, Quaternion.identity);
        HealVFX healVFX = vfxObj.GetComponent<HealVFX>();

        if (healVFX != null) healVFX.Play(amount);
        else Debug.LogError("[VFXManager] Prefab'da HealVFX bulunamadı!");
    }

    public void PlayShield(Vector3 position, int amount = 1)
    {
        if (shieldVFXPrefab == null) { Debug.LogWarning("[VFXManager] shieldVFXPrefab atanmamış!"); return; }

        GameObject vfxObj = Instantiate(shieldVFXPrefab, position, Quaternion.identity);
        ShieldVFX shieldVFX = vfxObj.GetComponent<ShieldVFX>();

        if (shieldVFX != null) shieldVFX.Play(amount);
        else Debug.LogError("[VFXManager] Prefab'da ShieldVFX bulunamadı!");
    }

    public void PlayDamage(Vector3 position, int damage = 1, string label = null)
    {
        if (damageVFXPrefab == null) { Debug.LogWarning("[VFXManager] damageVFXPrefab atanmamış!"); return; }

        float xOffset = Random.Range(-_damageXRange, _damageXRange);
        Vector3 spawnPos = position + new Vector3(xOffset, _damageYOffset, 0f);
        GameObject vfxObj = Instantiate(damageVFXPrefab, spawnPos, Quaternion.identity);
        DamageVFX damageVFX = vfxObj.GetComponent<DamageVFX>();

        if (damageVFX != null) damageVFX.Play(damage, label);
        else Debug.LogError("[VFXManager] Prefab'da DamageVFX bulunamadı!");
    }

    // ── Düşman VFX ────────────────────────────────────────────

    /// <summary>
    /// Düşmana vurunca çıkan kıvılcım/flash VFX.
    /// position: vurulma noktası (genellikle düşman transform.position)
    /// </summary>
    public void PlayHitSpark(Vector3 position)
    {
        if (hitSparkVFXPrefab == null)
        {
            Debug.LogWarning("[VFXManager] hitSparkVFXPrefab atanmamış!");
            return;
        }

        // Düşmanın tam merkezinde spawn et — hafif rastgele offset
        Vector3 spawnPos = position + new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.1f, 0.2f),
            0f);

        GameObject vfxObj = Instantiate(hitSparkVFXPrefab, spawnPos, Quaternion.identity);
        HitSparkVFX hitSparkVFX = vfxObj.GetComponent<HitSparkVFX>();

        if (hitSparkVFX != null) hitSparkVFX.Play();
        else Debug.LogError("[VFXManager] Prefab'da HitSparkVFX bulunamadı!");
    }
}