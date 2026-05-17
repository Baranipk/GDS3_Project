using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI Referansları")]
    public GameObject pauseMenuScreen;
    public GameObject firstSelectedButton;

    public bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sahne yüklenince tüm DOTween tweenlerini temizle
        // (restart donması ve HealthUI tween çakışması önlenir)
        DOTween.KillAll(complete: false);

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuScreen != null)
            pauseMenuScreen.SetActive(false);
    }

    private void Start()
    {
        if (pauseMenuScreen != null)
            pauseMenuScreen.SetActive(false);

        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (this == null || gameObject == null) return;

        isPaused = true;

        if (pauseMenuScreen != null)
            pauseMenuScreen.SetActive(true);

        Time.timeScale = 0f;

        // timeScale = 0 olduğu için direkt seç (async gerekmez)
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public async void Resume()
    {
        if (pauseMenuScreen != null)
            pauseMenuScreen.SetActive(false);

        // Input bleed önlemek için kısa bekleme
        // ignoreTimeScale: timeScale=0'dayken de çalışır
        await UniTask.Delay(100, ignoreTimeScale: true);

        isPaused = false;
        Time.timeScale = 1f;
    }

    public async void RestartLevel()
    {
        if (pauseMenuScreen != null)
            pauseMenuScreen.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        DOTween.KillAll(complete: false);

        await UniTask.Delay(200, ignoreTimeScale: true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public async void GoToMainMenu()
    {
        if (pauseMenuScreen != null)
            pauseMenuScreen.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        DOTween.KillAll(complete: false);

        await UniTask.Delay(200, ignoreTimeScale: true);
        SceneManager.LoadScene(0);
    }
}