using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks; // --- YENİ: UniTask Kütüphanesi ---

public class MainMenuManager : MonoBehaviour
{
    [Header("Ekran Referansları")]
    public GameObject pressKeyScreen;
    public GameObject menuScreen;

    [Header("Yanıp Sönme Ayarları")]
    public TextMeshProUGUI pressKeyText;
    public float blinkSpeed = 3f;

    [Header("Kontrolcü Ayarları")]
    [Tooltip("Menü açıldığında otomatik olarak seçilecek ilk buton (Örn: Play Butonu)")]
    public GameObject firstSelectedButton;

    private bool isWaitingForKey = true;

    private void Start()
    {
        pressKeyScreen.SetActive(true);
        menuScreen.SetActive(false);
    }

    private void Update()
    {
        if (isWaitingForKey)
        {
            if (pressKeyText != null)
            {
                Color textColor = pressKeyText.color;
                textColor.a = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
                pressKeyText.color = textColor;
            }

            if (WasAnyKeyPressed())
            {
                TransitionToMainMenu();
            }
        }
    }

    private bool WasAnyKeyPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
            return true;

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            return true;

        return false;
    }

    private void TransitionToMainMenu()
    {
        isWaitingForKey = false;

        pressKeyScreen.SetActive(false);
        menuScreen.SetActive(true);

        if (pressKeyText != null)
        {
            Color textColor = pressKeyText.color;
            textColor.a = 1f;
            pressKeyText.color = textColor;
        }

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    // --- YENİ: UNITASK İLE ASENKRON (GECİKMELİ) METOTLAR ---

    // Unity butonları async void metotlarını sorunsuz okuyabilir
    public async void PlayGame()
    {
        // Butona tıklandığında Bump animasyonunu izlemek için 300 milisaniye (0.3 saniye) bekle
        await UniTask.Delay(300);

        SceneManager.LoadScene(1);
    }

    public async void QuitGame()
    {
        await UniTask.Delay(300);

        Debug.Log("Oyundan Çıkılıyor...");
        Application.Quit();
    }
}