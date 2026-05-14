using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SceneManagement; // YENİ: Sahne takibi için

public class HealthUI : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform mainContainer;

    [Header("Kalp Ayarları")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Kalkan Ayarları")]
    [SerializeField] private GameObject shieldPrefab;

    [Header("Animasyon Ayarları")]
    [SerializeField] private float bounceStrength = 0.4f;
    [SerializeField] private float bounceDuration = 0.3f;
    [SerializeField] private float waveInterval = 0.05f;
    [SerializeField] private float damagePunchStrength = 1.2f;
    [SerializeField] private float fallDistance = 150f;
    [SerializeField] private float fallDuration = 0.5f;

    private List<Image> _heartImages = new List<Image>();
    private List<GameObject> _shieldIcons = new List<GameObject>();

    private int _lastHealth;
    private int _lastShield;
    private int _lastMaxHealth;

    // --- YENİ: SAHNE TAKİBİ İÇİN SUBSCRIPTION ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Her restart atıldığında veya sahne değiştiğinde burası çalışır
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Önceki sahneden kalan tüm animasyonları temizle (Hataları önler)
        DOTween.KillAll();

        // 2. Referansları temizle (Update içindeki bulucuyu tetikler)
        playerHealth = null;

        // 3. Mevcut UI'ı temizle
        ClearUI();
    }

    private void Start()
    {
        FindAndSetupPlayer();
    }

    private void Update()
    {
        // Eğer Player yoksa bulmaya çalış
        if (playerHealth == null)
        {
            FindAndSetupPlayer();
            return;
        }

        // 1. MAKSİMUM CAN DEĞİŞİMİ
        if (_lastMaxHealth != playerHealth.maxHealth)
        {
            _lastMaxHealth = playerHealth.maxHealth;
            _lastHealth = playerHealth.currentHealth;
            SetupHearts();
            InstantUpdateHearts();
        }

        // 2. MEVCUT CAN DEĞİŞİMİ
        if (_lastHealth != playerHealth.currentHealth)
        {
            if (playerHealth.currentHealth > _lastHealth)
                PlayHealWave();
            else
                UpdateHearts(true);

            _lastHealth = playerHealth.currentHealth;
        }

        // 3. KALKAN DEĞİŞİMİ
        if (_lastShield != playerHealth.currentShield)
        {
            if (playerHealth.currentShield > _lastShield)
                OnShieldAdded(playerHealth.currentShield - _lastShield);
            else if (playerHealth.currentShield < _lastShield)
                OnShieldBroken(_lastShield - playerHealth.currentShield);

            _lastShield = playerHealth.currentShield;
        }
    }

    private void FindAndSetupPlayer()
    {
        // Etikete bakmak yerine sahnede Health scripti takılı olan objeyi bul
        playerHealth = Object.FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            Debug.Log("HealthUI: PlayerHealth scripti doğrudan bulundu ve bağlandı!");
            _lastMaxHealth = playerHealth.maxHealth;
            _lastHealth = playerHealth.currentHealth;
            _lastShield = playerHealth.currentShield;
            SetupHearts();
            InstantUpdateHearts();
            InstantUpdateShields();
        }
    }

    private void ClearUI()
    {
        foreach (Transform child in mainContainer) Destroy(child.gameObject);
        _heartImages.Clear();
        _shieldIcons.Clear();
    }

    public void SetupHearts()
    {
        ClearUI(); // Önce temizle

        for (int i = 0; i < playerHealth.maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, mainContainer);
            _heartImages.Add(newHeart.GetComponent<Image>());
            newHeart.transform.localScale = Vector3.one;
        }
    }

    private void InstantUpdateHearts()
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (i < _heartImages.Count)
                _heartImages[i].sprite = i < playerHealth.currentHealth ? fullHeartSprite : emptyHeartSprite;
        }
    }

    private void InstantUpdateShields()
    {
        for (int i = 0; i < playerHealth.currentShield; i++)
        {
            GameObject newShield = Instantiate(shieldPrefab, mainContainer);
            _shieldIcons.Add(newShield);
        }
    }

    public void UpdateHearts(bool isDamage)
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (i < playerHealth.currentHealth)
                _heartImages[i].sprite = fullHeartSprite;
            else
            {
                _heartImages[i].sprite = emptyHeartSprite;
                if (isDamage && i == playerHealth.currentHealth)
                {
                    Transform heartT = _heartImages[i].transform;
                    heartT.DOKill();
                    heartT.localScale = Vector3.one;
                    heartT.DOPunchScale(Vector3.one * damagePunchStrength, 0.4f, 5, 0.5f).SetLink(heartT.gameObject);
                }
            }
        }
    }

    private void PlayHealWave()
    {
        InstantUpdateHearts();
        Sequence healSequence = DOTween.Sequence();
        for (int i = 0; i < playerHealth.currentHealth; i++)
        {
            if (i < _heartImages.Count)
            {
                Transform heartT = _heartImages[i].transform;
                heartT.DOKill();
                heartT.localScale = Vector3.one;

                healSequence.Insert(i * waveInterval,
                    heartT.DOPunchScale(Vector3.one * bounceStrength, bounceDuration, 10, 1f)
                    .SetLink(heartT.gameObject));
            }
        }
    }

    private void OnShieldAdded(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject newShield = Instantiate(shieldPrefab, mainContainer);
            _shieldIcons.Add(newShield);
            RectTransform shieldRect = newShield.GetComponent<RectTransform>();
            Vector2 finalPos = shieldRect.anchoredPosition;
            shieldRect.anchoredPosition = finalPos + new Vector2(0, fallDistance);
            shieldRect.DOAnchorPos(finalPos, fallDuration).SetEase(Ease.OutBounce).SetLink(newShield);
            AnimateIconPunch(newShield.transform, bounceDuration);
        }
    }

    private void OnShieldBroken(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (_shieldIcons.Count == 0) break;
            int index = _shieldIcons.Count - 1;
            GameObject obj = _shieldIcons[index];
            _shieldIcons.RemoveAt(index);
            AnimateShieldBreak(obj);
        }
    }

    private void AnimateShieldBreak(GameObject shieldObj)
    {
        RectTransform rt = shieldObj.GetComponent<RectTransform>();
        CanvasGroup cg = shieldObj.GetComponent<CanvasGroup>();
        if (rt == null || cg == null) { Destroy(shieldObj); return; }
        rt.DOKill(); cg.DOKill();
        shieldObj.transform.SetParent(mainContainer.parent, true);
        rt.DOAnchorPos(rt.anchoredPosition - new Vector2(0, fallDistance), fallDuration).SetEase(Ease.InQuad).SetLink(shieldObj);
        cg.DOFade(0f, fallDuration).SetEase(Ease.Linear).SetLink(shieldObj).OnComplete(() => Destroy(shieldObj));
    }

    private void AnimateIconPunch(Transform iconTransform, float duration)
    {
        iconTransform.DOKill();
        iconTransform.localScale = Vector3.one;
        iconTransform.DOPunchScale(Vector3.one * bounceStrength, duration, 10, 1f).SetLink(iconTransform.gameObject);
    }
}