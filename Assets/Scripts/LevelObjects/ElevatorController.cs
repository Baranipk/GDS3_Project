using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("Kimlik (ID) Ayarları")]
    public string elevatorID = "Elevator_1"; // Hangi lever ile eşleşeceğini belirleyen ID

    [Header("Asansör Ayarları")]
    public float speed = 2f;
    public float distanceInTiles = 5f;
    public bool startsAtBottom = false;

    [Header("Referanslar")]
    public Transform board;
    public Transform pulley;
    public SpriteRenderer leftChain;
    public SpriteRenderer rightChain;

    private Vector2 topPosition;
    private Vector2 bottomPosition;
    private Vector2 targetPosition;
    private Rigidbody2D boardRb;

    private Animator pulleyAnimator;
    // --- YENİ: SPRITE REFERANSLARI ---
    private SpriteRenderer pulleySpriteRenderer;
    private Sprite defaultPulleySprite; // Orijinal görseli hafızada tutacağımız değişken

    private bool isMoving = false;
    private bool isAtTop;

    private void Start()
    {
        boardRb = board.GetComponent<Rigidbody2D>();
        boardRb.bodyType = RigidbodyType2D.Kinematic;
        boardRb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Pulley bileşenlerini bul ve orijinal görseli kaydet
        if (pulley != null)
        {
            pulleyAnimator = pulley.GetComponent<Animator>();
            pulleySpriteRenderer = pulley.GetComponent<SpriteRenderer>();

            // Oyun başlamadan önce Inspector'da atadığın o düzgün görseli hafızaya alıyoruz
            if (pulleySpriteRenderer != null)
            {
                defaultPulleySprite = pulleySpriteRenderer.sprite;
            }
        }

        topPosition = board.position;
        bottomPosition = topPosition - new Vector2(0, distanceInTiles);

        if (startsAtBottom)
        {
            board.position = bottomPosition;
            isAtTop = false;
        }
        else
        {
            isAtTop = true;
        }

        targetPosition = board.position;

        // Oyun başladığında animasyonu kapat ve orijinal düz görseli yerleştir
        StopPulleyAnimation();
    }

    private void Update()
    {
        float currentDistance = Mathf.Abs(pulley.position.y - board.position.y);
        leftChain.size = new Vector2(leftChain.size.x, currentDistance);
        rightChain.size = new Vector2(rightChain.size.x, currentDistance);
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            Vector2 newPos = Vector2.MoveTowards(boardRb.position, targetPosition, speed * Time.fixedDeltaTime);
            boardRb.MovePosition(newPos);

            if (Vector2.Distance(boardRb.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                isAtTop = !isAtTop;

                // Hedefe ulaştık, animasyonu durdur ve görseli sıfırla
                StopPulleyAnimation();
            }
        }
    }

    public void ToggleElevator()
    {
        if (isMoving) return;

        isMoving = true;
        targetPosition = isAtTop ? bottomPosition : topPosition;

        // Hareket başladı, animasyonu aç
        if (pulleyAnimator != null)
        {
            pulleyAnimator.enabled = true;
        }
    }

    // Kod tekrarını önlemek için görsel sıfırlama işlemini bir metoda aldık
    private void StopPulleyAnimation()
    {
        if (pulleyAnimator != null)
        {
            pulleyAnimator.enabled = false; // Animator'ı kapat ki Sprite'ı değiştirmemize izin versin
        }

        if (pulleySpriteRenderer != null && defaultPulleySprite != null)
        {
            pulleySpriteRenderer.sprite = defaultPulleySprite; // Hafızadaki orijinal düz görseli geri yükle
        }
    }
}