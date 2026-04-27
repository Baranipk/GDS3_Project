using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class HealthUI : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Health playerHealth;
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
    private int _lastMaxHealth; // YENİ TAKİP DEĞİŞKENİ

    private void Start()
    {
        // 1. Önce Player'ı bulmaya çalış
        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<Health>();
            }
        }

        // 2. Eğer Player GERÇEKTEN o an sahnede yoksa, Start'ı durdur (Çökmeyi önler).
        // Merak etme, Update metodu Player sahneye girdiği an bu kurulumu (SetupHearts) kendisi yapacak!
        if (playerHealth == null) return;

        // 3. Player varsa normal kuruluma devam et
        _lastMaxHealth = playerHealth.maxHealth;
        _lastHealth = playerHealth.currentHealth;
        _lastShield = playerHealth.currentShield;

        SetupHearts();
        InstantUpdateHearts();
        InstantUpdateShields();
    }

    private void Update()
    {
        // --- YENİ EKLENEN GÜVENLİK VE OTOMATİK REFERANS BULUCU ---
        if (playerHealth == null)
        {
            // Sahnede Player etiketli objeyi ara
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<Health>();

                // Yeni Player bulundu! Değerleri güncelle ve UI'ı baştan çiz
                if (playerHealth != null)
                {
                    _lastMaxHealth = playerHealth.maxHealth;
                    _lastHealth = playerHealth.currentHealth;
                    _lastShield = playerHealth.currentShield;

                    ResetAllHeartsScale();
                    SetupHearts();
                    InstantUpdateHearts();
                    InstantUpdateShields();
                }
            }
            // Player henüz sahnede yoksa veya bulunamadıysa aşağıdaki kodları çalıştırma (Çökmeyi önler)
            return;
        }

        // 1. MAKSİMUM CAN DEĞİŞİMİ
        if (_lastMaxHealth != playerHealth.maxHealth)
        {
            ResetAllHeartsScale();
            SetupHearts();
            InstantUpdateShields();
            PlayHealWave();
            _lastMaxHealth = playerHealth.maxHealth;
            _lastHealth = playerHealth.currentHealth;
            return;
        }

        // 2. MEVCUT CAN DEĞİŞİMİ
        if (_lastHealth != playerHealth.currentHealth)
        {
            ResetAllHeartsScale();

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

    public void SetupHearts()
    {
        foreach (Transform child in mainContainer) Destroy(child.gameObject);
        _heartImages.Clear();
        _shieldIcons.Clear();

        for (int i = 0; i < playerHealth.maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, mainContainer);
            _heartImages.Add(newHeart.GetComponent<Image>());
            // Yeni oluşturulan kalbin boyutunun 1 olduğundan emin ol
            newHeart.transform.localScale = Vector3.one;
        }
    }

    private void InstantUpdateHearts()
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
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
                // Hasar alan (boşalan) kalbin büyümesini engellemek için:
                if (isDamage && i == playerHealth.currentHealth)
                {
                    Transform heartT = _heartImages[i].transform;
                    heartT.DOKill();
                    heartT.localScale = Vector3.one;
                    heartT.DOPunchScale(Vector3.one * damagePunchStrength, 0.4f, 5, 0.5f)
                        .SetLink(heartT.gameObject);
                }
            }
        }
    }

    private void ResetAllHeartsScale()
    {
        foreach (var heartImg in _heartImages)
        {
            if (heartImg != null)
            {
                heartImg.transform.DOKill(); // Devam eden animasyonu durdur
                heartImg.transform.localScale = Vector3.one; // Boyutu tam 1.0 yap
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
        // BUG ÇÖZÜMÜ BURADA
        iconTransform.DOKill();
        iconTransform.localScale = Vector3.one;

        iconTransform.DOPunchScale(Vector3.one * bounceStrength, duration, 10, 1f).SetLink(iconTransform.gameObject);
    }
}