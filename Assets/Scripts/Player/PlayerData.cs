using UnityEngine;
using System.Collections.Generic; // List kullanmak için bu kütüphane şart

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Can Verileri")]
    public int maxHealth = 3;
    public int currentHealth = 3;
    public int currentShield = 0;

    [Header("Toplanan Eşyalar")]
    // YENİ: Toplanan Max Health objelerinin ID'lerini burada tutacağız
    public List<string> collectedHealthUpgrades = new List<string>();

    public void ResetToDefaults()
    {
        maxHealth = 3;
        currentHealth = 3;
        currentShield = 0;

        // Oyunu tamamen sıfırladığında toplanan eşya hafızası da temizlensin
        collectedHealthUpgrades.Clear();
    }
}