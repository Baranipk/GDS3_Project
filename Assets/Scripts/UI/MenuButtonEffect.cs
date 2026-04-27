using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI; // Image bileşeni için gerekli
using DG.Tweening;

public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, ISubmitHandler
{
    [Header("Referanslar (Opsiyonel)")]
    public TextMeshProUGUI buttonText;
    public Image buttonImage; // Kare butonlar veya slider görselleri için

    [Header("Renk Ayarları")]
    public bool changeColor = true;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Boyut Ayarları")]
    public float selectedScale = 1.15f;
    public float bumpScale = 0.9f;
    public float animDuration = 0.15f;

    private Vector3 defaultScale;

    private void Start()
    {
        // Eğer atanmamışsa otomatik bulmaya çalış
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();

        defaultScale = transform.localScale;
        SetInitialColors();
    }

    private void SetInitialColors()
    {
        if (!changeColor) return;
        if (buttonText != null) buttonText.color = normalColor;
        if (buttonImage != null) buttonImage.color = normalColor;
    }

    private void OnDisable()
    {
        transform.DOKill();
        if (buttonText != null) buttonText.DOKill();
        if (buttonImage != null) buttonImage.DOKill(); // Uzantı metodu yoksa aşağıdakini kullanın
    }

    // --- SEÇİLME / ÜZERİNE GELME ---
    public void OnPointerEnter(PointerEventData eventData) { SelectEffect(); }
    public void OnSelect(BaseEventData eventData) { SelectEffect(); }

    // --- SEÇİMDEN ÇIKMA ---
    public void OnPointerExit(PointerEventData eventData) { DeselectEffect(); }
    public void OnDeselect(BaseEventData eventData) { DeselectEffect(); }

    // --- TIKLAMA / BUMP ---
    public void OnPointerDown(PointerEventData eventData) { BumpEffect(); }
    public void OnSubmit(BaseEventData eventData) { BumpEffect(); }

    private void SelectEffect()
    {
        transform.DOKill();
        transform.DOScale(defaultScale * selectedScale, animDuration).SetEase(Ease.OutBack);

        if (changeColor)
        {
            if (buttonText != null) buttonText.DOColor(selectedColor, animDuration);
            if (buttonImage != null) buttonImage.DOColor(selectedColor, animDuration);
        }

        SoundManager.Instance?.Get("Hover")?.PlayOneShot();
    }

    private void DeselectEffect()
    {
        transform.DOKill();
        transform.DOScale(defaultScale, animDuration).SetEase(Ease.OutQuad);

        if (changeColor)
        {
            if (buttonText != null) buttonText.DOColor(normalColor, animDuration);
            if (buttonImage != null) buttonImage.DOColor(normalColor, animDuration);
        }
    }

    private void BumpEffect()
    {
        transform.DOKill();
        SoundManager.Instance?.Get("Click")?.PlayOneShot();

        transform.DOScale(defaultScale * bumpScale, 0.1f).OnComplete(() =>
        {
            transform.DOScale(defaultScale * selectedScale, 0.15f).SetEase(Ease.OutBack);
        });
    }
}