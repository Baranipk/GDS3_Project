using UnityEngine;

public class Spike : MonoBehaviour
{
    // Dikenler "Trigger" (Tetikleyici) olacağı için OnTriggerEnter2D kullanıyoruz
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Temas eden obje Player mı?
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            // Player'ın Health scriptini bul
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Affetme, anında öldür!
                playerHealth.InstantKill();
                SoundManager.Instance?.TryPlayOneShot("SpikeHit");
                Debug.Log("Karakter dikene çarptı ve öldü!");
            }
        }
    }
}