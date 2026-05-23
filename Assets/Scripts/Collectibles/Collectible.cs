using UnityEngine;
using DG.Tweening;

public enum CollectibleType
{
    Shield,
    SmallHealth,
    FullHealth,
    MaxHealthIncrease
}

public class Collectible : MonoBehaviour
{
    [Header("Eşya Ayarları")]
    public CollectibleType type;
    public int amount = 1;

    [Header("Benzersiz Kimlik (Sadece MaxHealth için)")]
    public string uniqueID;

    [Header("Görsel Efekt (Float)")]
    public float floatSpeed = 3f;
    public float floatHeight = 0.1f;

    [Header("Toplama Animasyonu")]
    public float pickupScaleTo = 1.6f;
    public float pickupRiseY = 0.5f;
    public float pickupDuration = 0.3f;
    public Ease pickupScaleEase = Ease.OutBack;
    public Ease pickupFadeEase = Ease.InQuad;
    public Ease pickupRiseEase = Ease.OutCubic;

    private float _startY;
    private bool _isCollected;
    private SpriteRenderer _sr;
    private Collider2D _col;

    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        _startY = transform.position.y;
    }

    private void Update()
    {
        if (_isCollected) return; // Toplama animasyonu sırasında float'ı durdur

        float newY = _startY + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector2(transform.position.x, newY);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                bool isCollected = false;

                switch (type)
                {
                    case CollectibleType.Shield:
                        if (playerHealth.currentShield < playerHealth.maxShield)
                        {
                            playerHealth.AddShield(amount);
                            SoundManager.Instance?.TryPlayOneShot("PickupShield");
                            isCollected = true;
                        }
                        break;

                    case CollectibleType.SmallHealth:
                        if (playerHealth.currentHealth < playerHealth.maxHealth)
                        {
                            playerHealth.Heal(amount);
                            SoundManager.Instance?.TryPlayOneShot("PickupHealthSmall");
                            isCollected = true;
                        }
                        break;

                    case CollectibleType.FullHealth:
                        if (playerHealth.currentHealth < playerHealth.maxHealth)
                        {
                            playerHealth.Heal(playerHealth.maxHealth);
                            SoundManager.Instance?.TryPlayOneShot("PickupHealthFull");
                            isCollected = true;
                        }
                        break;

                    case CollectibleType.MaxHealthIncrease:
                        // 1. Eğer benzersiz bir ID girildiyse
                        if (!string.IsNullOrEmpty(uniqueID))
                        {
                            // 2. Bu ID daha ÖNCE ALINMADIYSA canı artır ve hafızaya kaydet
                            if (!playerHealth.HasCollectedUpgrade(uniqueID))
                            {
                                playerHealth.IncreaseMaxHealth(amount);
                                playerHealth.RecordUpgrade(uniqueID);
                                SoundManager.Instance?.TryPlayOneShot("PickupMaxHealth");
                            }

                            // 3. Her halükarda (daha önce alınmış olsa bile) objeyi toplanmış sayıp yok edeceğiz
                            isCollected = true;
                        }
                        else
                        {
                            Debug.LogWarning("MaxHealthIncrease objesinin ID'si boş! Eşya alınamadı.");
                        }
                        break;
                }

                if (isCollected)
                {
                    PlayPickupAnimation();
                }
            }
        }
    }

    private void PlayPickupAnimation()
    {
        if (_isCollected) return;
        _isCollected = true;

        // Tekrar tetiklenmesin
        if (_col != null) _col.enabled = false;

        // Mevcut transform tween'lerini durdur
        transform.DOKill();

        // Scale-up
        transform.DOScale(transform.localScale * pickupScaleTo, pickupDuration)
                 .SetEase(pickupScaleEase);

        // Yukarı süzülme
        transform.DOMoveY(transform.position.y + pickupRiseY, pickupDuration)
                 .SetEase(pickupRiseEase);

        // Fade out (sprite alpha)
        if (_sr != null)
        {
            _sr.DOFade(0f, pickupDuration)
               .SetEase(pickupFadeEase)
               .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            // Sprite yoksa yine de animasyon sonunda destroy
            Invoke(nameof(DestroySelf), pickupDuration);
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}