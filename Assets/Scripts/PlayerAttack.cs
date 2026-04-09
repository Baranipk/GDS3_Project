using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Saldırı Ayarları")]
    [SerializeField] private float attacksPerSecond = 2f; // Saniyede yapılabilecek saldırı sayısı

    [Header("Görsel Efekt Ayarları")]
    [SerializeField] private GameObject slashPrefab;    // Slash (kesik) efekti prefabı
    [SerializeField] private Transform attackPoint;     // Efektin çıkacağı nokta (karakterin önünde bir boş obje)

    private PlayerController _controller;
    private float _nextAttackTime = 0f;

    private void Awake()
    {
        // Yön bilgisini alabilmek için controller referansını alıyoruz
        _controller = GetComponent<PlayerController>();
    }

    /// <summary>
    /// Saldırı yapılabilecek durumda olup olmadığımızı kontrol eder.
    /// PlayerInputHandler veya State içinden çağrılır.
    /// </summary>
    public bool CanAttack()
    {
        return Time.time >= _nextAttackTime;
    }

    /// <summary>
    /// Saldırı mantığını başlatır.
    /// </summary>
    public void PerformAttack()
    {
        // Bir sonraki saldırı için bekleme süresini hesapla
        _nextAttackTime = Time.time + (1f / attacksPerSecond);

        // Efekti oluştur (Eğer Animation Event kullanmıyorsan burası tetikler)
        SpawnSlash();
    }

    /// <summary>
    /// Slash efektini oluşturur ve yönünü ayarlar.
    /// Bu metodu dilersen Animation Event (Animasyon Etkinliği) olarak da çağırabilirsin.
    /// </summary>
    public void SpawnSlash()
    {
        if (slashPrefab == null || attackPoint == null)
        {
            Debug.LogWarning("Slash Prefab veya Attack Point atanmamış!");
            return;
        }

        // 1. Efekti belirlediğimiz noktada oluştur
        GameObject slash = Instantiate(slashPrefab, attackPoint.position, attackPoint.rotation);

        // 2. Efektin içindeki SwordSlash scriptine ulaş
        if (slash.TryGetComponent(out SwordSlash slashScript))
        {
            // Controller'daki FacingDirection (1 veya -1) bilgisini gönderiyoruz
            // Hatırlatma: GetMoveDirection'a göre güncellediğin FacingDirection'ı kullanır.
            slashScript.Setup(_controller.FacingDirection);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Gizmo rengini belirle (Örn: Parlak Kırmızı)
            Gizmos.color = Color.red;

            // AttackPoint pozisyonuna içi boş bir küre çiz
            // 0.2f kürenin büyüklüğüdür, isteğine göre değiştirebilirsin
            Gizmos.DrawWireSphere(attackPoint.position, 0.2f);

            // İstersen vuruş yönünü göstermek için küçük bir çizgi de ekleyebilirsin
            Gizmos.DrawLine(attackPoint.position, attackPoint.position + attackPoint.right * 0.5f);
        }
    }
}
