using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Ekran Referansları")]
    public GameObject pressKeyScreen;
    public GameObject menuScreen;
    public GameObject settingsScreen;

    // --- YENİ: LEVEL MENÜSÜ EKRAN REFERANSLARI ---
    [Header("Level Menüsü Referansları")]
    public GameObject levelScreen;
    public GameObject levelFirstSelected; // Level menüsü açılınca seçilecek ilk buton (Örn: Level 1)

    [Header("Yanıp Sönme Ayarları")]
    public TextMeshProUGUI pressKeyText;
    public float blinkSpeed = 3f;

    [Header("Kontrolcü Ayarları")]
    public GameObject firstSelectedButton;
    public GameObject settingsFirstSelected;

    [Header("Ayarlar (Settings) Referansları")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Mute Buton İkonları")]
    public Image masterMuteImage;
    public Image musicMuteImage;
    public Image sfxMuteImage;

    [Tooltip("Ses AÇIKKEN görünecek ikon (Normal)")]
    public Sprite unmutedSprite;
    [Tooltip("Ses KAPALIYKEN görünecek ikon (Örn: Çarpılı/Susturulmuş)")]
    public Sprite mutedSprite;

    private bool isMasterMuted = false;
    private bool isMusicMuted = false;
    private bool isSfxMuted = false;

    private bool isWaitingForKey = true;

    private void Start()
    {
        pressKeyScreen.SetActive(true);
        menuScreen.SetActive(false);
        if (settingsScreen != null) settingsScreen.SetActive(false);
        if (levelScreen != null) levelScreen.SetActive(false); // Başlangıçta Level ekranını kapalı tut

        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Başlangıçta ikonları normal (Unmuted) duruma getir
        UpdateMuteVisuals();
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
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)) return true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
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

    // ==========================================
    // --- EKRAN GEÇİŞ METOTLARI ---
    // ==========================================

    public void OpenSettings()
    {
        menuScreen.SetActive(false);
        settingsScreen.SetActive(true);

        if (settingsFirstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(settingsFirstSelected);
        }
    }

    public void CloseSettings()
    {
        settingsScreen.SetActive(false);
        menuScreen.SetActive(true);

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    // --- YENİ: LEVEL MENÜSÜ GEÇİŞLERİ ---
    public void OpenLevelMenu()
    {
        menuScreen.SetActive(false);
        levelScreen.SetActive(true);

        if (levelFirstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(levelFirstSelected);
        }
    }

    public void CloseLevelMenu()
    {
        levelScreen.SetActive(false);
        menuScreen.SetActive(true);

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    // ==========================================
    // --- SES KONTROL METOTLARI ---
    // ==========================================

    public void OnMasterChanged(float val)
    {
        SoundManager.Instance?.SetVolume("MasterVolume", val);
        isMasterMuted = false;
        UpdateMuteVisuals();
    }
    public void OnMusicChanged(float val)
    {
        SoundManager.Instance?.SetVolume("MusicVolume", val);
        isMusicMuted = false;
        UpdateMuteVisuals();
    }
    public void OnSfxChanged(float val)
    {
        SoundManager.Instance?.SetVolume("SFXVolume", val);
        isSfxMuted = false;
        UpdateMuteVisuals();
    }

    public void ToggleMaster()
    {
        isMasterMuted = !isMasterMuted;
        SoundManager.Instance?.ToggleMute("MasterVolume", isMasterMuted, masterSlider.value);
        UpdateMuteVisuals();
    }
    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        SoundManager.Instance?.ToggleMute("MusicVolume", isMusicMuted, musicSlider.value);
        UpdateMuteVisuals();
    }
    public void ToggleSfx()
    {
        isSfxMuted = !isSfxMuted;
        SoundManager.Instance?.ToggleMute("SFXVolume", isSfxMuted, sfxSlider.value);
        UpdateMuteVisuals();
    }

    private void UpdateMuteVisuals()
    {
        if (masterMuteImage != null) masterMuteImage.sprite = isMasterMuted ? mutedSprite : unmutedSprite;
        if (musicMuteImage != null) musicMuteImage.sprite = isMusicMuted ? mutedSprite : unmutedSprite;
        if (sfxMuteImage != null) sfxMuteImage.sprite = isSfxMuted ? mutedSprite : unmutedSprite;
    }

    // ==========================================
    // --- ASENKRON (GECİKMELİ) OYUN METOTLARI ---
    // ==========================================

    // --- YENİ: LEVEL YÜKLEME METODU ---
    public async void LoadLevel(int levelIndex)
    {
        // Butonun "Bump" animasyonunu ve tıklama sesini duymak için bekle
        await UniTask.Delay(300);

        // Build Settings'deki sıraya (Index) göre sahneyi yükle
        SceneManager.LoadScene(levelIndex);
    }

    public async void QuitGame()
    {
        await UniTask.Delay(300);
        Debug.Log("Oyundan Çıkılıyor...");
        Application.Quit();
    }
}