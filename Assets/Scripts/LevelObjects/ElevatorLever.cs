using UnityEngine;

public class ElevatorLever : MonoBehaviour
{
    [Header("Bağlantı Ayarları")]
    [Tooltip("Bu şalterin tetikleyeceği asansörün ID'sini buraya girin.")]
    public string targetElevatorID = "Elevator_1";

    private ElevatorController linkedElevator;
    private Animator animator;
    private bool isLeverDown = false; // Şalterin mevcut durumu

    // --- COOLDOWN (BEKLEME SÜRESİ) DEĞİŞKENLERİ ---
    private float lastInteractTime = 0f;
    private float interactCooldown = 0.5f; // Şaltere vurduktan sonra yarım saniye tekrar vurulamaz

    private void Start()
    {
        animator = GetComponent<Animator>();
        ConnectToElevator();
    }

    private void ConnectToElevator()
    {
        ElevatorController[] allElevators = Object.FindObjectsByType<ElevatorController>(FindObjectsInactive.Exclude);

        foreach (var elevator in allElevators)
        {
            if (elevator.elevatorID == targetElevatorID)
            {
                linkedElevator = elevator;
                return;
            }
        }

        Debug.LogWarning($"DİKKAT: '{targetElevatorID}' ID'sine sahip asansör sahnede bulunamadı! Şalter boşta.");
    }

    // SwordSlash scripti tarafından çağrılacak metod
    public void Interact()
    {
        // Eğer son etkileşimin üzerinden yeterli süre geçmediyse hiçbir şey yapma (Çift tetiklenmeyi önler)
        if (Time.time - lastInteractTime < interactCooldown) return;

        lastInteractTime = Time.time; // Süreyi güncelle

        // TEST LOGU: Konsolda bunu görüyorsan kılıç şaltere sorunsuz çarpıyor demektir.
        Debug.Log("Kılıç şaltere başarıyla çarptı! Şalter tetikleniyor...");

        // Durumu tersine çevir
        isLeverDown = !isLeverDown;

        // 1. Animasyonu tetikle
        if (animator != null)
        {
            animator.SetBool("IsDown", isLeverDown);
        }

        // 2. Bağlı asansörü çalıştır
        if (linkedElevator != null)
        {
            linkedElevator.ToggleElevator();
        }
    }
}