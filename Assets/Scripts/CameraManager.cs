using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera vCam;
    private CinemachineConfiner2D _confiner;

    private void Awake()
    {
        if (vCam == null) vCam = GetComponent<CinemachineVirtualCamera>();
        _confiner = vCam.GetComponent<CinemachineConfiner2D>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) return;

        // 1. Oyuncuyu Bulma
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            vCam.Follow = player.transform;
            Debug.Log("<color=green>Kamera Takibi: Player bulundu ve atandı.</color>");
        }
        else
        {
            Debug.LogError("<color=red>Kamera Hatası: Yeni sahnede 'Player' etiketli obje bulunamadı!</color>");
        }

        // 2. Sınırları Bulma
        GameObject newBounds = GameObject.FindGameObjectWithTag("CameraBounds");
        
        if (newBounds == null)
        {
            Debug.LogWarning("Kamera Uyarısı: Bu sahnede 'CameraBounds' bulunamadı.");
        }
        else if (_confiner == null)
        {
            Debug.LogError("Kamera Hatası: Sınır objesi bulundu ancak Kameranızda 'CinemachineConfiner2D' bileşeni EKSİK!");
        }
        else
        {
            PolygonCollider2D poly = newBounds.GetComponent<PolygonCollider2D>();
            if (poly != null)
            {
                _confiner.m_BoundingShape2D = poly;
                _confiner.InvalidateCache();
                Debug.Log("<color=green>Kamera Sınırı: Yeni sınırlar atandı.</color>");
            }
            else
            {
                Debug.LogError("<color=red>Kamera Hatası: 'CameraBounds' objesinde PolygonCollider2D yok!</color>");
            }
        }
    }
}