using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, ISubmitHandler
{
    [Header("Referanslar (Opsiyonel)")]
    public TextMeshProUGUI buttonText;
    public Image buttonImage;

    [Header("Renk Ayarları")]
    public bool changeColor = true;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Boyut Ayarları")]
    public float selectedScale = 1.15f;
    public float bumpScale = 0.9f;
    public float animDuration = 0.15f;

    private Vector3 defaultScale;
    private bool isInitialized = false;

    private void Awake()
    {
        // Boyutu Start yerine Awake'te alıyoruz ki en başından kesinleşsin
        defaultScale = transform.localScale;

        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();

        isInitialized = true;
    }

    private void Start()
    {
        SetInitialColors();
    }

    private void OnEnable()
    {
        // Menü her görünür olduğunda (açıldığında) boyutları güvenceye al
        if (isInitialized)
        {
            transform.localScale = defaultScale;
            SetInitialColors();
        }
    }

    private void OnDisable()
    {
        // Menü kapandığında yarıda kalan animasyonları sil ve SIFIRLA
        transform.DOKill();
        transform.localScale = defaultScale;

        if (changeColor)
        {
            if (buttonText != null)
            {
                buttonText.DOKill();
                buttonText.color = normalColor;
            }
            if (buttonImage != null)
            {
                buttonImage.DOKill();
                buttonImage.color = normalColor;
            }
        }
    }

    
    private void SetInitialColors()
    {
        if (!changeColor) return;
        if (buttonText != null) buttonText.color = normalColor;
        if (buttonImage != null) buttonImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData) { SelectEffect(); }
    public void OnSelect(BaseEventData eventData) { SelectEffect(); }
    public void OnPointerExit(PointerEventData eventData) { DeselectEffect(); }
    public void OnDeselect(BaseEventData eventData) { DeselectEffect(); }
    public void OnPointerDown(PointerEventData eventData) { BumpEffect(); }
    public void OnSubmit(BaseEventData eventData) { BumpEffect(); }

    private void SelectEffect()
    {
        transform.DOKill();
        transform.DOScale(defaultScale * selectedScale, animDuration).SetEase(Ease.OutBack).SetUpdate(true);

        if (changeColor)
        {
            if (buttonText != null) buttonText.DOColor(selectedColor, animDuration).SetUpdate(true);
            if (buttonImage != null) buttonImage.DOColor(selectedColor, animDuration).SetUpdate(true);
        }

        SoundManager.Instance?.Get("Hover")?.PlayOneShot();
    }

    private void DeselectEffect()
    {
        transform.DOKill();
        transform.DOScale(defaultScale, animDuration).SetEase(Ease.OutQuad).SetUpdate(true);

        if (changeColor)
        {
            if (buttonText != null) buttonText.DOColor(normalColor, animDuration).SetUpdate(true);
            if (buttonImage != null) buttonImage.DOColor(normalColor, animDuration).SetUpdate(true);
        }
    }

    private void BumpEffect()
    {
        transform.DOKill();
        SoundManager.Instance?.Get("Click")?.PlayOneShot();

        transform.DOScale(defaultScale * bumpScale, 0.1f).SetUpdate(true).OnComplete(() =>
        {
            transform.DOScale(defaultScale * selectedScale, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
        });
    }
}