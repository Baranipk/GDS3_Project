using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening; // --- DOTWEEN KÜTÜPHANESİ ---

public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, ISubmitHandler
{
    [Header("Referanslar")]
    public TextMeshProUGUI buttonText;

    [Header("Renk Ayarları")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Boyut Ayarları")]
    public float selectedScale = 1.2f; // Üzerine gelince ne kadar büyüyecek?
    public float bumpScale = 0.9f;     // Tıklayınca ne kadar içeri göçecek?
    public float animDuration = 0.2f;  // Animasyonun gerçekleşme süresi

    private Vector3 defaultScale;

    private void Start()
    {
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        defaultScale = transform.localScale;
        buttonText.color = normalColor;
    }

    private void OnDisable()
    {
        // Obje aniden kapanırsa arkada çalışan DoTween animasyonlarını temizle (Hata önleyici)
        transform.DOKill();
        if (buttonText != null) buttonText.DOKill();
    }

    // --- FARE (MOUSE) KONTROLLERİ ---
    public void OnPointerEnter(PointerEventData eventData) { SelectButton(); }
    public void OnPointerExit(PointerEventData eventData) { DeselectButton(); }

    // --- KLAVYE / OYUN KOLU KONTROLLERİ ---
    public void OnSelect(BaseEventData eventData) { SelectButton(); }
    public void OnDeselect(BaseEventData eventData) { DeselectButton(); }

    // --- TIKLAMA / ONAYLAMA (BUMP EFEKTİ) ---
    public void OnPointerDown(PointerEventData eventData) { BumpEffect(); } // Fare ile tıklayınca
    public void OnSubmit(BaseEventData eventData) { BumpEffect(); }         // Enter veya oyun kolu (A/X) ile basınca

    private void SelectButton()
    {
        transform.DOKill();
        buttonText.DOKill();

        transform.DOScale(defaultScale * selectedScale, animDuration).SetEase(Ease.OutBack);
        buttonText.DOColor(selectedColor, animDuration);

        // --- YENİ: ÜZERİNE GELİNCE SES ÇAL ---
        // Üst üste hızlı geçişlerde ses kesilmesin diye PlayOneShot kullanıyoruz
        SoundManager.Instance.Get("Hover")?.PlayOneShot();
    }

    private void DeselectButton()
    {
        transform.DOKill();
        buttonText.DOKill();

        // Ease.OutQuad: Sakin ve pürüzsüz bir şekilde küçülür
        transform.DOScale(defaultScale, animDuration).SetEase(Ease.OutQuad);
        buttonText.DOColor(normalColor, animDuration);
    }

    private void BumpEffect()
    {
        transform.DOKill();

        // --- YENİ: TIKLANINCA SES ÇAL ---
        SoundManager.Instance.Get("Click")?.PlayOneShot();

        transform.DOScale(defaultScale * bumpScale, 0.1f).OnComplete(() =>
        {
            transform.DOScale(defaultScale * selectedScale, 0.15f).SetEase(Ease.OutBack);
        });
    }
}