using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootEntry
{
    public GameObject itemPrefab; // Düşecek eşya (Boş bırakırsan "hiçbir şey düşmez" anlamına gelir)
    public int weight;            // Ağırlığı (Örn: 100 çok sık, 1 çok nadir)
}

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootEntry> lootEntries;

    public GameObject GetRandomLoot()
    {
        if (lootEntries == null || lootEntries.Count == 0) return null;

        // 1. Toplam ağırlığı hesapla
        int totalWeight = 0;
        foreach (var entry in lootEntries)
        {
            totalWeight += entry.weight;
        }

        // 2. 0 ile toplam ağırlık arasında rastgele bir sayı seç
        int randomValue = Random.Range(0, totalWeight);

        // 3. Seçilen sayının hangi aralığa düştüğünü bul
        int cursor = 0;
        foreach (var entry in lootEntries)
        {
            cursor += entry.weight;
            if (randomValue < cursor)
            {
                return entry.itemPrefab;
            }
        }

        return null;
    }
}