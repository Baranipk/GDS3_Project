using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class SliderControllerFix : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Slider slider;
    private bool isEditing = false;
    private Navigation originalNav;
    private float lockedValue;

    [Header("Görsel Geri Bildirim (Sprite)")]
    [Tooltip("Slider düzenleme modundayken (seçiliyken) görünecek yeni kalkan görseli")]
    public Sprite editingSprite;

    private Sprite originalSprite; // Kod başlangıçtaki orijinal kalkanı hafızaya alacak
    private Image handleImage;

    void Awake()
    {
        slider = GetComponent<Slider>();

        // Kulp (Handle) objesini ve orijinal görselini bulup kaydet
        if (slider.handleRect != null)
        {
            handleImage = slider.handleRect.GetComponent<Image>();
            if (handleImage != null)
            {
                originalSprite = handleImage.sprite;
            }
        }

        // Başlangıçtaki o ayarladığın "Explicit" bağlantılarını hafızaya al
        originalNav = slider.navigation;
        lockedValue = slider.value;
    }

    void Update()
    {
        // KİLİT SİSTEMİ: Eğer düzenleme modunda değilsek ve yana geçmeye basarsan
        // Slider'ın sesinin 1 tık bozulmasını anında engeller.
        if (!isEditing && slider.value != lockedValue)
        {
            slider.SetValueWithoutNotify(lockedValue);
        }
    }

    // Oyun kolu (A/X) veya klavyede (Enter) basıldığında
    public void OnSubmit(BaseEventData eventData)
    {
        SetEditing(!isEditing);
        SoundManager.Instance?.Get("Click")?.PlayOneShot();
    }

    private void SetEditing(bool state)
    {
        isEditing = state;

        if (isEditing)
        {
            // DÜZENLEME MODU GİRİŞİ: Gezinmeyi "None" yaparak sağa sola kaçışı engelleriz
            Navigation customNav = slider.navigation;
            customNav.mode = Navigation.Mode.None;
            slider.navigation = customNav;
        }
        else
        {
            // DÜZENLEME MODU ÇIKIŞI: Orijinal bağlantılara (Explicit) geri döneriz
            slider.navigation = originalNav;
            lockedValue = slider.value; // Çıkarken ayarladığın yeni sesi kaydeder
        }

        // --- YENİ: Görsel (Sprite) Değişimi ---
        if (handleImage != null && editingSprite != null)
        {
            // Eğer düzenleme modundaysa yeni görseli, değilse eski görseli koy
            handleImage.sprite = isEditing ? editingSprite : originalSprite;
        }
    }

    // Seçildiğinde her zaman "Gezinme" modunda başla
    public void OnSelect(BaseEventData eventData)
    {
        lockedValue = slider.value;
        SetEditing(false);
    }

    // Çıkıldığında moddan çıkar
    public void OnDeselect(BaseEventData eventData)
    {
        SetEditing(false);
    }

    // --- FARE DESTEĞİ --- 
    public void OnPointerDown(PointerEventData eventData) { SetEditing(true); }
    public void OnPointerUp(PointerEventData eventData) { SetEditing(false); }
}