using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Kalp ve Kalkan UI'ını yönetir.
///
/// DÜZELTME: Update() polling kaldırıldı.
/// Artık PlayerHealth.OnHealthChanged event'i ile çalışır:
///   - Her can/kalkan değişiminde anında tetiklenir
///   - Sahne yeniden yüklenince OnPlayerReady ile sıfırdan kurulur
///   - _lastXxx değerleri artık sadece animasyon kararları için kullanılır
/// </summary>
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

    // ── Durum ──────────────────────────────────────────────────
    private List<Image> _heartImages = new List<Image>();
    private List<GameObject> _shieldIcons = new List<GameObject>();

    // Animasyon kararları için — hangi yönde değişti?
    private int _lastHealth = -1;
    private int _lastShield = -1;
    private int _lastMaxHealth = -1;

    private bool _isUIReady = false;
    private bool _isInitializing = false;

    // ─────────────────────────────────────────────────────────── 
    #region Unity Lifecycle

    private void Start()
    {
        if (mainContainer == null)
        {
            Debug.LogError("[HealthUI] mainContainer atanmamış! Inspector'dan sürükleyin.", this);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerHealth.OnPlayerReady += OnPlayerHealthReady;
        PlayerHealth.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerHealth.OnPlayerReady -= OnPlayerHealthReady;
        PlayerHealth.OnHealthChanged -= OnHealthChanged;
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DOTween.KillAll(complete: false);
        StopAllCoroutines();

        _isInitializing = false;
        _isUIReady = false;

        // BUG FIX: _lastXxx sıfırlanmazsa ForceUpdateUI aynı değerlerde
        // "değişiklik yok" zannedip UI'ı güncellemiyordu
        ResetLastValues();

        ClearUI();
    }

    #endregion

    // ─────────────────────────────────────────────────────────── 
    #region Event Handlers

    /// <summary>
    /// PlayerHealth.Start() tamamlandığında — UI'ı sıfırdan kur.
    /// </summary>
    private void OnPlayerHealthReady(PlayerHealth health)
    {
        if (mainContainer == null) return;

        StopAllCoroutines();
        _isInitializing = false;

        playerHealth = health;
        ResetLastValues();
        ForceUpdateUI();
        _isUIReady = true;
    }

    /// <summary>
    /// Her can/kalkan değişiminde tetiklenir (TakeDamage, Heal, AddShield, ResetHealth...).
    /// Update() polling yerine bu event kullanılır — anlık ve güvenilir.
    /// </summary>
    private void OnHealthChanged(PlayerHealth health)
    {
        if (!_isUIReady || mainContainer == null) return;
        if (health != playerHealth) return; // Başka bir player'ın eventi ise yoksay

        HandleChanges();
    }

    #endregion

    // ─────────────────────────────────────────────────────────── 
    #region Initialization (Fallback)

    /// <summary>
    /// Player bulunamazsa yedek coroutine başlatır.
    /// Normal akış: OnPlayerReady event'i üzerinden.
    /// </summary>
    private void StartInitUI()
    {
        if (_isInitializing) return;
        _isInitializing = true;
        StartCoroutine(InitUI());
    }

    private IEnumerator InitUI()
    {
        playerHealth = null;
        float elapsed = 0f;
        float timeout = 5f;

        while (playerHealth == null && elapsed < timeout)
        {
            FindPlayer();
            if (playerHealth == null)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        _isInitializing = false;

        if (playerHealth == null)
        {
            Debug.LogWarning("[HealthUI] 5 saniye içinde PlayerHealth bulunamadı!");
            yield break;
        }

        ResetLastValues();
        ForceUpdateUI();
        _isUIReady = true;
    }

    private void FindPlayer()
    {
        if (PlayerHealth.Instance != null)
        {
            playerHealth = PlayerHealth.Instance;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null) return;
        }

        var all = Object.FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include);
        if (all.Length > 0)
            playerHealth = all[0];
    }

    private void ResetLastValues()
    {
        _lastMaxHealth = -1;
        _lastHealth = -1;
        _lastShield = -1;
    }

    private void ForceUpdateUI()
    {
        if (playerHealth == null || mainContainer == null) return;

        _lastMaxHealth = playerHealth.maxHealth;
        _lastHealth = playerHealth.currentHealth;
        _lastShield = playerHealth.currentShield;

        ClearUI();
        SetupHearts();
        InstantUpdateHearts();
        InstantUpdateShields();
    }

    #endregion

    // ─────────────────────────────────────────────────────────── 
    #region Change Handling

    private void HandleChanges()
    {
        if (playerHealth == null) return;

        // Maksimum can değiştiyse — UI'ı sıfırdan kur
        if (_lastMaxHealth != playerHealth.maxHealth)
        {
            _lastMaxHealth = playerHealth.maxHealth;
            _lastHealth = playerHealth.currentHealth;
            _lastShield = playerHealth.currentShield;

            ClearUI();
            SetupHearts();
            InstantUpdateHearts();
            InstantUpdateShields();
            return;
        }

        // Can değiştiyse
        if (_lastHealth != playerHealth.currentHealth)
        {
            bool isHeal = playerHealth.currentHealth > _lastHealth;
            _lastHealth = playerHealth.currentHealth;

            if (isHeal)
                PlayHealWave();
            else
                UpdateHearts(isDamage: true);
        }

        // Kalkan değiştiyse
        if (_lastShield != playerHealth.currentShield)
        {
            bool isAdded = playerHealth.currentShield > _lastShield;
            int diff = Mathf.Abs(playerHealth.currentShield - _lastShield);
            _lastShield = playerHealth.currentShield;

            if (isAdded)
                OnShieldAdded(diff);
            else
                OnShieldBroken(diff);
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────── 
    #region UI Build & Update

    private void ClearUI()
    {
        if (mainContainer == null) return;

        foreach (Transform child in mainContainer)
        {
            DOTween.Kill(child);
            Destroy(child.gameObject);
        }

        _heartImages.Clear();
        _shieldIcons.Clear();
    }

    public void SetupHearts()
    {
        if (heartPrefab == null || mainContainer == null || playerHealth == null) return;

        for (int i = 0; i < playerHealth.maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, mainContainer);
            _heartImages.Add(newHeart.GetComponent<Image>());
            newHeart.transform.localScale = Vector3.one;
        }
    }

    private void InstantUpdateHearts()
    {
        if (playerHealth == null) return;

        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] == null) continue;
            _heartImages[i].sprite = (i < playerHealth.currentHealth)
                ? fullHeartSprite
                : emptyHeartSprite;
        }
    }

    private void InstantUpdateShields()
    {
        if (shieldPrefab == null || mainContainer == null || playerHealth == null) return;

        for (int i = 0; i < playerHealth.currentShield; i++)
        {
            GameObject newShield = Instantiate(shieldPrefab, mainContainer);
            _shieldIcons.Add(newShield);
        }
    }

    public void UpdateHearts(bool isDamage)
    {
        if (playerHealth == null) return;

        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] == null) continue;

            if (i < playerHealth.currentHealth)
            {
                _heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                _heartImages[i].sprite = emptyHeartSprite;

                if (isDamage && i == playerHealth.currentHealth)
                {
                    Transform heartT = _heartImages[i].transform;
                    heartT.DOKill();
                    heartT.localScale = Vector3.one;
                    heartT.DOPunchScale(
                        Vector3.one * damagePunchStrength, 0.4f, 5, 0.5f)
                          .SetLink(heartT.gameObject);
                }
            }
        }
    }

    private void PlayHealWave()
    {
        if (playerHealth == null) return;

        InstantUpdateHearts();
        Sequence healSequence = DOTween.Sequence();

        for (int i = 0; i < playerHealth.currentHealth; i++)
        {
            if (i >= _heartImages.Count || _heartImages[i] == null) break;

            Transform heartT = _heartImages[i].transform;
            heartT.DOKill();
            heartT.localScale = Vector3.one;

            healSequence.Insert(
                i * waveInterval,
                heartT.DOPunchScale(Vector3.one * bounceStrength, bounceDuration, 10, 1f)
                      .SetLink(heartT.gameObject));
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────── 
    #region Shield Animations

    private void OnShieldAdded(int amount)
    {
        if (shieldPrefab == null || mainContainer == null) return;

        for (int i = 0; i < amount; i++)
        {
            GameObject newShield = Instantiate(shieldPrefab, mainContainer);
            _shieldIcons.Add(newShield);

            RectTransform shieldRect = newShield.GetComponent<RectTransform>();
            if (shieldRect != null)
            {
                Vector2 finalPos = shieldRect.anchoredPosition;
                shieldRect.anchoredPosition = finalPos + new Vector2(0, fallDistance);
                shieldRect.DOAnchorPos(finalPos, fallDuration)
                          .SetEase(Ease.OutBounce)
                          .SetLink(newShield);
            }

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
        if (shieldObj == null) return;

        RectTransform rt = shieldObj.GetComponent<RectTransform>();
        CanvasGroup cg = shieldObj.GetComponent<CanvasGroup>();

        if (rt == null || cg == null) { Destroy(shieldObj); return; }

        rt.DOKill();
        cg.DOKill();

        shieldObj.transform.SetParent(mainContainer.parent, true);
        rt.DOAnchorPos(rt.anchoredPosition - new Vector2(0, fallDistance), fallDuration)
          .SetEase(Ease.InQuad)
          .SetLink(shieldObj);
        cg.DOFade(0f, fallDuration)
          .SetEase(Ease.Linear)
          .SetLink(shieldObj)
          .OnComplete(() => { if (shieldObj != null) Destroy(shieldObj); });
    }

    private void AnimateIconPunch(Transform iconTransform, float duration)
    {
        if (iconTransform == null) return;

        iconTransform.DOKill();
        iconTransform.localScale = Vector3.one;
        iconTransform.DOPunchScale(Vector3.one * bounceStrength, duration, 10, 1f)
                     .SetLink(iconTransform.gameObject);
    }

    #endregion
}