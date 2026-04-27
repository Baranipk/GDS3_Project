using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public ElevatorController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Sadece asansörü çalıştırıyoruz. 
            // Düşmeye sebep olan "SetParent" (çocuk yapma) kodlarını tamamen sildik!
            controller.ToggleElevator();
        }
    }

    // OnTriggerExit2D metodunu da tamamen sildik çünkü artık bağ koparmamıza gerek yok.
}