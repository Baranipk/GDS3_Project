using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Sinematik letterbox bar geçişi — coroutine tabanlı, Time.timeScale'den BAĞIMSIZ.
/// DOTween kullanmaz, DOTween.KillAll çağrıları bu animasyonları etkilemez.
///
/// Otomatik kurulumludur — sahnede manuel olarak eklemeye gerek yok.
/// </summary>
public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance { get; private set; }

    [Header("Bar Ayarları")]
    [SerializeField] private float barHeightPercent = 0f;
    [SerializeField] private float openDuration  = 1.6f;
    [SerializeField] private float closeDuration = 0.6f;
    [SerializeField] private Color barColor = Color.black;

    [Header("Main Menu Atlama")]
    [Tooltip("Bu build index'teki sahnede açılma animasyonu OYNAMAZ (main menu).")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    public float OpenDuration  => openDuration;
    public float CloseDuration => closeDuration;

    private Canvas _canvas;
    private RectTransform _topBar;
    private RectTransform _bottomBar;

    private Coroutine _activeAnim;
    private Coroutine _callbackRoutine;

    private const int SORT_ORDER = 9999;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("[LevelTransition]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<LevelTransition>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Sadece root + regular scene'deysek DDL uygula (child ise parent zaten DDL'lemiş olmalı)
        if (transform.parent == null && gameObject.scene.buildIndex != -1)
            DontDestroyOnLoad(gameObject);

        BuildUI();

        SceneManager.sceneLoaded += OnSceneLoaded;

        int currentIdx = SceneManager.GetActiveScene().buildIndex;
        if (currentIdx == mainMenuBuildIndex)
            SetBarsOpenImmediate();
        else
            PlayOpen();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == mainMenuBuildIndex)
        {
            SetBarsOpenImmediate();
        }
        else
        {
            // Bars hâlâ "open" pozisyonundaysa (main menu'den geliyorsak) önce kapat,
            // sonra animate ile aç — yoksa görünür animasyon olmaz
            SetBarsClosedImmediate();
            PlayOpen();
        }
    }

    // ─── UI build ────────────────────────────────────────────

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = SORT_ORDER;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        _topBar    = CreateBar("TopBar",    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
        _bottomBar = CreateBar("BottomBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));

        SetBarsClosedImmediate();
    }

    private RectTransform CreateBar(string n, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject go = new GameObject(n, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(_canvas.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, 0f);

        Image img = go.GetComponent<Image>();
        img.color = barColor;
        img.raycastTarget = false;
        return rt;
    }

    private void SetBarsClosedImmediate()
    {
        float h = Screen.height * 0.5f;
        _topBar.sizeDelta = new Vector2(0f, h);
        _bottomBar.sizeDelta = new Vector2(0f, h);
    }

    public void SetBarsOpenImmediate()
    {
        float h = Screen.height * barHeightPercent;
        _topBar.sizeDelta = new Vector2(0f, h);
        _bottomBar.sizeDelta = new Vector2(0f, h);
    }

    // ─── Public API ─────────────────────────────────────────

    /// <summary>Barları aç. (closed → open) Time.timeScale'den bağımsız çalışır.</summary>
    public void PlayOpen(Action onComplete = null)
    {
        StopActiveAnim();
        float targetH = Screen.height * barHeightPercent;
        _activeAnim = StartCoroutine(AnimateBars(targetH, openDuration, EaseMode.InOutCubic, onComplete));
    }

    /// <summary>Barları kapat. (open → closed) Time.timeScale'den bağımsız çalışır.</summary>
    public void PlayClose(Action onComplete = null)
    {
        StopActiveAnim();
        float targetH = Screen.height * 0.5f;

        // Callback'i animasyondan ayrı olarak da garanti et (animasyon kesilse bile)
        if (_callbackRoutine != null) StopCoroutine(_callbackRoutine);
        if (onComplete != null)
            _callbackRoutine = StartCoroutine(InvokeAfterRealtime(closeDuration, onComplete));

        _activeAnim = StartCoroutine(AnimateBars(targetH, closeDuration, EaseMode.InCubic, null));
    }

    public void HideBars()
    {
        StopActiveAnim();
        _topBar.sizeDelta = new Vector2(0f, 0f);
        _bottomBar.sizeDelta = new Vector2(0f, 0f);
    }

    // ─── Animation engine (coroutine, unscaled) ──────────────

    private enum EaseMode { Linear, InCubic, OutCubic, InOutCubic }

    private void StopActiveAnim()
    {
        if (_activeAnim != null)
        {
            StopCoroutine(_activeAnim);
            _activeAnim = null;
        }
    }

    private IEnumerator AnimateBars(float targetHeight, float duration, EaseMode ease, Action onComplete)
    {
        float startTopH = _topBar.sizeDelta.y;
        float startBotH = _bottomBar.sizeDelta.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float k = ApplyEase(t, ease);

            _topBar.sizeDelta    = new Vector2(0f, Mathf.LerpUnclamped(startTopH, targetHeight, k));
            _bottomBar.sizeDelta = new Vector2(0f, Mathf.LerpUnclamped(startBotH, targetHeight, k));
            yield return null;
        }

        _topBar.sizeDelta    = new Vector2(0f, targetHeight);
        _bottomBar.sizeDelta = new Vector2(0f, targetHeight);
        _activeAnim = null;

        onComplete?.Invoke();
    }

    private IEnumerator InvokeAfterRealtime(float seconds, Action cb)
    {
        yield return new WaitForSecondsRealtime(seconds);
        cb?.Invoke();
        _callbackRoutine = null;
    }

    private static float ApplyEase(float t, EaseMode mode)
    {
        switch (mode)
        {
            case EaseMode.InCubic:    return t * t * t;
            case EaseMode.OutCubic:   { float u = 1f - t; return 1f - u * u * u; }
            case EaseMode.InOutCubic: return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
            default: return t;
        }
    }
}
