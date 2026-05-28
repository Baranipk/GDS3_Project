using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

    [Header("Gizlenecek Gruplar")]
    public GameObject uiContainer;      // HUD, Canlar, Pause Menüsü
    public GameObject eventSystemObject; // PersistentSystems içindeki EventSystem objesi
    public GameObject playerObject;      // Karakter (Gerekiyorsa)
    public GameObject cameraObject;      // Kamera (Gerekiyorsa)

    private void Awake()
    {
        // Singleton Yapısı
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ANA MENÜYE GİRİNCE (Build Index: 0)
        if (scene.buildIndex == 0)
        {
            // Senin önerdiğin gibi: EventSystem'i kapat ki Main Menu'deki ile çakışmasın
            if (eventSystemObject != null) eventSystemObject.SetActive(false);

            // Diğer oyun içi görselleri de gizle ama SoundManager'a dokunma!
            if (uiContainer != null) uiContainer.SetActive(false);
            if (playerObject != null) playerObject.SetActive(false);
            if (cameraObject != null) cameraObject.SetActive(false);

            Time.timeScale = 1f;
        }
        // OYUN BÖLÜMLERİNE GİRİNCE (Level 1, 2 vb.)
        else
        {
            // EventSystem'i tekrar aç, artık tek yetkili o olsun
            if (eventSystemObject != null) eventSystemObject.SetActive(true);

            // Gizlediğimiz her şeyi geri getir
            if (uiContainer != null) uiContainer.SetActive(true);
            if (playerObject != null) playerObject.SetActive(true);
            if (cameraObject != null) cameraObject.SetActive(true);
        }
    }
}