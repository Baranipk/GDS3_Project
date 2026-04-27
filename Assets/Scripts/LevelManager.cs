using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Not: [PersistentSystems] içinde olduğu için DontDestroyOnLoad yazmıyoruz.
    }

    // 1. Belirli bir ismi olan sahneyi yüklemek için
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 2. Build Settings'teki bir sonraki sahneyi otomatik yüklemek için
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Eğer son sahnede değilsek bir sonrakine geç
        if (currentSceneIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        else
        {
            Debug.LogWarning("Sıradaki sahne yok! Ana menüye veya ilk sahneye dönülebilir.");
            // İstersen burada oyunu bitirebilir veya 0. sahneye döndürebilirsin:
            // SceneManager.LoadScene(0);
        }
    }

    // 3. Mevcut bölümü yeniden başlatmak için (Karakter ölünce vb.)
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}