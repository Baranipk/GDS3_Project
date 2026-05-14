using UnityEngine;

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
    // YENİ: Unity Editör'den her bir objeye özel bir ID yazacaksın (Örn: "Level1_GizliKutu")
    public string uniqueID;

    [Header("Görsel Efekt")]
    public float floatSpeed = 3f;
    public float floatHeight = 0.1f;
    private float _startY;

    private void Start()
    {
        _startY = transform.position.y;
    }

    private void Update()
    {
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
                            isCollected = true;
                        }
                        break;

                    case CollectibleType.SmallHealth:
                        if (playerHealth.currentHealth < playerHealth.maxHealth)
                        {
                            playerHealth.Heal(amount);
                            isCollected = true;
                        }
                        break;

                    case CollectibleType.FullHealth:
                        if (playerHealth.currentHealth < playerHealth.maxHealth)
                        {
                            playerHealth.Heal(playerHealth.maxHealth);
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
                    Destroy(gameObject);
                }
            }
        }
    }
}