using UnityEngine;

public enum CollectibleType
{
    Shield,
    SmallHealth,
    FullHealth,
    MaxHealthIncrease // YENİ EKLENDİ
}

public class Collectible : MonoBehaviour
{
    [Header("Eşya Ayarları")]
    public CollectibleType type;
    public int amount = 1;

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
            Health playerHealth = collision.GetComponentInParent<Health>();

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

                    case CollectibleType.MaxHealthIncrease: // YENİ MANTIK
                        playerHealth.IncreaseMaxHealth(amount);
                        isCollected = true;
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