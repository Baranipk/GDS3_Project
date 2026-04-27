using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI Referansları")]
    public GameObject pauseMenuScreen;
    public GameObject firstSelectedButton;

    // İŞTE BURASI ÇOK ÖNEMLİ: Artık public! Böylece karakterin oyunun durduğunu bilebilecek.
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

    // --- SAHNE TAKİBİ ---
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
        isPaused = false;
        if (pauseMenuScreen != null) pauseMenuScreen.SetActive(false);
        Time.timeScale = 1f;
    }
    // ----------------------------------------

    private void Start()
    {
        if (pauseMenuScreen != null) pauseMenuScreen.SetActive(false);
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

    public async void Pause()
    {
        // Obje yok ediliyorsa metottan çık
        if (this == null || gameObject == null) return;

        isPaused = true;
        if (pauseMenuScreen != null) pauseMenuScreen.SetActive(true);
        Time.timeScale = 0f;

        await UniTask.Yield();

        // EventSystem kontrolü (73. satır hatası için kesin çözüm)
        if (UnityEngine.EventSystems.EventSystem.current != null && firstSelectedButton != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public async void Resume()
    {
        pauseMenuScreen.SetActive(false);

        // Input Bleed çözümü için bekleme
        await UniTask.Delay(100, ignoreTimeScale: true);

        isPaused = false;
        Time.timeScale = 1f;
    }

    public async void RestartLevel()
    {
        pauseMenuScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        await UniTask.Delay(200);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public async void GoToMainMenu()
    {
        pauseMenuScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        await UniTask.Delay(200);
        SceneManager.LoadScene(0);
    }
}