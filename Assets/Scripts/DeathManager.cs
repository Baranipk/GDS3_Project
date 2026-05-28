using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

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
        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);
    }

    // ── Ölüm Ekranını Göster ───────────────────────────────────
    // async KALDIRILDI: PlayerDeathState içinde zaten 2 saniyelik bekleme var.
    // async + timeScale=0 kombinasyonu UniTask'ı askıya alıyordu → ölüm ekranı açılmıyordu.
    public void ShowDeathScreen()
    {
        if (deathScreenUI == null)
        {
            Debug.LogError("[DeathManager] deathScreenUI atanmamış! Inspector'dan sürükleyin.", this);
            return;
        }

        Time.timeScale = 0f;
        deathScreenUI.SetActive(true);

        // timeScale = 0 olduğu için Invoke çalışmaz, direkt seç
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    // ── Buton Fonksiyonları ────────────────────────────────────

    public async void RestartLevel()
    {
        Time.timeScale = 1f;

        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        DOTween.KillAll(complete: false);

        await UniTask.Delay(100, ignoreTimeScale: true);

        int idx = SceneManager.GetActiveScene().buildIndex;
        if (LevelTransition.Instance != null)
            LevelTransition.Instance.PlayClose(() => SceneManager.LoadScene(idx));
        else
            SceneManager.LoadScene(idx);
    }

    public async void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        DOTween.KillAll(complete: false);

        await UniTask.Delay(100, ignoreTimeScale: true);

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.PlayClose(() => SceneManager.LoadScene(0));
        else
            SceneManager.LoadScene(0);
    }
}