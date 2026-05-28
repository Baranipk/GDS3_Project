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
        _confiner = vCam != null ? vCam.GetComponent<CinemachineConfiner2D>() : null;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // KRİTİK FIX: Bu sahneye direkt Play ile girildiyse OnSceneLoaded ateşlenmemiş olabilir.
        // Mevcut sahne için bir kez manuel bind et.
        BindToScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindToScene(scene);
    }

    /// <summary>
    /// Aktif sahnedeki Player ve CameraBounds'u bulup vCam'a bağlar.
    /// Hem sahne yüklenince hem de direkt Play durumunda (Start) çağrılır.
    /// </summary>
    private void BindToScene(Scene scene)
    {
        if (scene.buildIndex == 0) return;
        if (vCam == null)
        {
            Debug.LogError("<color=red>[CameraManager] vCam atanmamış!</color>", this);
            return;
        }

        // 1. Oyuncuyu Bulma
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            vCam.Follow = player.transform;
            Debug.Log("<color=green>[CameraManager] Player bulundu ve takibe atandı.</color>");
        }
        else
        {
            Debug.LogError("<color=red>[CameraManager] 'Player' etiketli obje bulunamadı!</color>");
        }

        // 2. Sınırları Bulma
        GameObject newBounds = GameObject.FindGameObjectWithTag("CameraBounds");

        if (newBounds == null)
        {
            Debug.LogWarning("<color=yellow>[CameraManager] Bu sahnede 'CameraBounds' etiketli obje bulunamadı.</color>");
            return;
        }

        if (_confiner == null)
        {
            // Awake'te bulunamadıysa tekrar dene (vCam runtime değişmiş olabilir)
            _confiner = vCam.GetComponent<CinemachineConfiner2D>();
        }

        if (_confiner == null)
        {
            Debug.LogError("<color=red>[CameraManager] CameraBounds var ama vCam'da 'CinemachineConfiner2D' bileşeni EKSİK!</color>", vCam);
            return;
        }

        // Collider tipi kontrolü — PolygonCollider2D veya CompositeCollider2D olabilir
        Collider2D boundsCol = newBounds.GetComponent<PolygonCollider2D>();
        if (boundsCol == null) boundsCol = newBounds.GetComponent<CompositeCollider2D>();
        if (boundsCol == null) boundsCol = newBounds.GetComponent<BoxCollider2D>();

        if (boundsCol == null)
        {
            Debug.LogError("<color=red>[CameraManager] 'CameraBounds' objesinde Collider2D (Polygon/Composite/Box) yok!</color>", newBounds);
            return;
        }

        _confiner.m_BoundingShape2D = boundsCol;
        _confiner.InvalidateCache();
        Debug.Log($"<color=green>[CameraManager] Sınırlar atandı: {newBounds.name} ({boundsCol.GetType().Name})</color>", newBounds);
    }
}
