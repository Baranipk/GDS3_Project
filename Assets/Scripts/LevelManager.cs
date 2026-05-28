using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

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
        // Önce barları kapat, sonra sahneyi yükle
        if (LevelTransition.Instance != null)
            LevelTransition.Instance.PlayClose(() => SceneManager.LoadScene(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }

    // 2. Build Settings'teki bir sonraki sahneyi otomatik yüklemek için
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            int next = currentSceneIndex + 1;
            if (LevelTransition.Instance != null)
                LevelTransition.Instance.PlayClose(() => SceneManager.LoadScene(next));
            else
                SceneManager.LoadScene(next);
        }
        else
        {
            Debug.LogWarning("Sıradaki sahne yok! Ana menüye veya ilk sahneye dönülebilir.");
        }
    }

    // 3. Mevcut bölümü yeniden başlatmak için (Karakter ölünce vb.)
    public void RestartLevel()
    {
        int idx = SceneManager.GetActiveScene().buildIndex;
        if (LevelTransition.Instance != null)
            LevelTransition.Instance.PlayClose(() => SceneManager.LoadScene(idx));
        else
            SceneManager.LoadScene(idx);
    }
}
