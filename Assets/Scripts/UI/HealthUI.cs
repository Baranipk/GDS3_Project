using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Kalp ve Kalkan UI'ını yönetir.
///
/// Düzeltilen Buglar:
/// 1. _isInitializing flag'i: Update() her frame yeni coroutine başlatamaz → restart donması gitti.
/// 2. PlayerHealth.OnPlayerReady event'i: timing sorununu köküyle çözer.
///    OnSceneLoaded → Start()'tan önce gelir → HealthUI yanlış değerleri (default) okurdu.
///    OnPlayerReady → Start()'tan sonra gelir → veriler kesinlikle yüklü, UI doğru kurulur.
/// 3. OnSceneLoaded'da StopAllCoroutines + _isInitializing reset: hızlı geçişlerde coroutine
///    birikimini önler.
/// 4. FindObjectsByType ile inactive objeler de aranır (Unity 6 uyumlu).
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
            return;
        }

        // HealthUI ilk kez oluşturulduğunda (DontDestroyOnLoad ilk sahne)
        // OnPlayerReady zaten subscribe, ek bir şey yapmaya gerek yok.
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // ÇÖZÜM: PlayerHealth.Start() bitince bu event tetiklenir.
        // OnSceneLoaded timing sorununu tamamen bypass eder.
        PlayerHealth.OnPlayerReady += OnPlayerHealthReady;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerHealth.OnPlayerReady -= OnPlayerHealthReady;
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Eski tweenleri temizle
        DOTween.KillAll(complete: false);

        // Önceki coroutine'i durdur ve flag'i sıfırla.
        // (Hızlı sahne geçişlerinde eski coroutine _isInitializing=true bırakabiliyordu)
        StopAllCoroutines();
        _isInitializing = false;
        _isUIReady = false;

        // UI içeriğini temizle — OnPlayerReady event'i gelince yeniden kurulacak.
        ClearUI();

        // NOT: Ana menüde (buildIndex 0) player yok, StartInitUI() çağırmıyoruz.
        // OnPlayerReady event'i zaten oyun sahnelerinde tetiklenecek.
    }

    /// <summary>
    /// PlayerHealth.Start() tamamlandığında çağrılır.
    /// Bu noktada maxHealth ve currentHealth kesinlikle doğru değerlere sahip.
    /// </summary>
    private void OnPlayerHealthReady(PlayerHealth health)
    {
        if (mainContainer == null) return;

        // Önceki coroutine'i durdur — artık gerek yok
        StopAllCoroutines();
        _isInitializing = false;

        playerHealth = health;
        ForceUpdateUI();
        _isUIReady = true;
    }

    private void Update()
    {
        if (!_isUIReady || mainContainer == null) return;

        if (playerHealth == null)
        {
            _isUIReady = false;
            StartInitUI(); // Fallback: player kaybolursa coroutine ile tekrar ara
            return;
        }

        if (_lastMaxHealth != playerHealth.maxHealth ||
            _lastHealth != playerHealth.currentHealth ||
            _lastShield != playerHealth.currentShield)
        {
            HandleChanges();
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────── 
    #region Initialization (Fallback)

    // Bu coroutine artık yalnızca yedek olarak kullanılır.
    // Normal akış: OnPlayerReady event'i → OnPlayerHealthReady() → ForceUpdateUI()
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
            Debug.LogWarning("[HealthUI] 5 saniye içinde PlayerHealth bulunamadı! " +
                             "Player objesinin 'Player' tag'ine sahip olduğundan ve " +
                             "PlayerHealth script'inin eklendiğinden emin ol.");
            yield break;
        }

        ForceUpdateUI();
        _isUIReady = true;
    }

    private void FindPlayer()
    {
        // 1. Statik referans (en hızlı)
        if (PlayerHealth.Instance != null)
        {
            playerHealth = PlayerHealth.Instance;
            return;
        }

        // 2. Tag ile ara
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null) return;
        }

        // 3. Inactive objeler dahil tüm sahnede ara (Unity 6 uyumlu)
        var all = Object.FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include);
        if (all.Length > 0)
            playerHealth = all[0];
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
        if (_lastMaxHealth != playerHealth.maxHealth)
        {
            _lastMaxHealth = playerHealth.maxHealth;
            ClearUI();
            SetupHearts();
            InstantUpdateHearts();
            InstantUpdateShields();
        }

        if (_lastHealth != playerHealth.currentHealth)
        {
            if (playerHealth.currentHealth > _lastHealth)
                PlayHealWave();
            else
                UpdateHearts(true);

            _lastHealth = playerHealth.currentHealth;
        }

        if (_lastShield != playerHealth.currentShield)
        {
            if (playerHealth.currentShield > _lastShield)
                OnShieldAdded(playerHealth.currentShield - _lastShield);
            else
                OnShieldBroken(_lastShield - playerHealth.currentShield);

            _lastShield = playerHealth.currentShield;
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
        if (heartPrefab == null || mainContainer == null) return;

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
            _heartImages[i].sprite = (i < playerHealth.currentHealth)
                ? fullHeartSprite
                : emptyHeartSprite;
        }
    }

    private void InstantUpdateShields()
    {
        if (shieldPrefab == null || mainContainer == null) return;

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
        InstantUpdateHearts();
        Sequence healSequence = DOTween.Sequence();

        for (int i = 0; i < playerHealth.currentHealth; i++)
        {
            if (i >= _heartImages.Count) break;

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

        if (rt == null || cg == null)
        {
            Destroy(shieldObj);
            return;
        }

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