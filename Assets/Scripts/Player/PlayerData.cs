using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Can Verileri")]
    public int maxHealth = 3;
    public int currentHealth = 3;
    public int currentShield = 0;

    // Oyunu tamamen kapattığında veya baştan başladığında verileri sıfırlamak için
    public void ResetToDefaults()
    {
        maxHealth = 3;
        currentHealth = 3;
        currentShield = 0;
    }
}