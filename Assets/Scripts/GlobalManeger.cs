using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;

    private void Awake()
    {
        // Eğer sahnede zaten bir tane varsa yenisini yok et (Double manager hatasını önler)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Bu obje ve içindekiler artık her sahnede kalıcı!
    }
}