using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;

    [Header("UI Referansları")]
    public GameObject deathScreenUI;
    public GameObject firstSelectedButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (deathScreenUI != null) deathScreenUI.SetActive(false);
    }

    // --- ÖLÜM EKRANINI AÇ ---
    public async void ShowDeathScreen()
    {
        // Ölüm ekranı açıldığında zamanı durdurabiliriz
        Time.timeScale = 0f;

        if (deathScreenUI != null) deathScreenUI.SetActive(true);

        // EventSystem'in butonu seçmesi için minik bir bekleme
        await UniTask.Yield();

        if (UnityEngine.EventSystems.EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    // --- BUTON FONKSİYONLARI ---

    public async void RestartLevel()
    {
        Time.timeScale = 1f; // Zamanı geri al
        deathScreenUI.SetActive(false);

        // Sahneyi yeniden yükle
        await UniTask.Delay(100, ignoreTimeScale: true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public async void GoToMainMenu()
    {
        Time.timeScale = 1f;
        deathScreenUI.SetActive(false);

        await UniTask.Delay(100, ignoreTimeScale: true);
        SceneManager.LoadScene(0); // Main Menu indexi 0 demiştik
    }
}