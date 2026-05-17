using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Background tilemap'ini randomize eder; column ve 3x3 block dekorasyonları yerleştirir.
///
/// Column yapısı:       3x3 Block yapısı (inspector sırası):
///   [Top]                [1][2][3]   ← üst sıra
///   [Middle] × N         [4][5][6]   ← orta sıra
///   [Bottom]             [7][8][9]   ← alt sıra
/// </summary>
public class BackgroundTilemapRandomizer : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════
    #region Inspector Fields

    [Header("Tilemap Referansı")]
    [Tooltip("BG, column ve block'ların çizileceği tek tilemap")]
    [SerializeField] private Tilemap backgroundTilemap;

    [Header("Randomizasyon Ayarları")]
    [SerializeField] private bool randomizeSeedOnStart = true;
    [SerializeField] private int fixedSeed = 42;
    [SerializeField] private bool randomizeOnStart = true;

    [Header("Background Tile Listesi")]
    [SerializeField] private List<TileEntry> tileEntries = new List<TileEntry>();

    [Header("Column Dekorasyonları")]
    [SerializeField] private List<ColumnDecoration> columns = new List<ColumnDecoration>();

    [Header("3x3 Block Dekorasyonları")]
    [SerializeField] private List<BlockDecoration> blocks = new List<BlockDecoration>();

    [SerializeField, HideInInspector] private float _totalWeight;

    #endregion

    // ═══════════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Start()
    {
        if (randomizeOnStart) Randomize();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════
    #region Public API

    public void Randomize()
    {
        if (backgroundTilemap == null)
        {
            Debug.LogError("[BackgroundRandomizer] backgroundTilemap atanmamış!", this);
            return;
        }

        int seed = randomizeSeedOnStart ? Random.Range(0, int.MaxValue) : fixedSeed;
        Random.InitState(seed);
        Debug.Log($"[BackgroundRandomizer] Seed: {seed}");

        if (tileEntries != null && tileEntries.Count > 0)
            RandomizeBackground();

        // Tüm block'ların ortak doluluk haritası — farklı block tipleri de birbirine binmez
        var blockOccupied = new HashSet<Vector2Int>();

        if (columns != null && columns.Count > 0)
            PlaceAllColumns();

        if (blocks != null && blocks.Count > 0)
            PlaceAllBlocks(blockOccupied);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════
    #region Background Randomization

    private void RandomizeBackground()
    {
        _totalWeight = CalculateTotalWeight(tileEntries);
        if (_totalWeight <= 0f) return;

        backgroundTilemap.CompressBounds();
        BoundsInt bounds = backgroundTilemap.cellBounds;
        int count = 0;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!backgroundTilemap.HasTile(pos)) continue;
            TileBase chosen = PickFromList(tileEntries, _totalWeight);
            if (chosen != null) { backgroundTilemap.SetTile(pos, chosen); count++; }
        }

        Debug.Log($"[BackgroundRandomizer] {count} background hücre randomize edildi.");
    }

    #endregion

    // ═══════════════════════════════════════════════════════════
    #region Column Placement

    private void PlaceAllColumns()
    {
        backgroundTilemap.CompressBounds();
        BoundsInt bounds = backgroundTilemap.cellBounds;

        foreach (var col in columns)
        {
            if (!col.enabled) continue;
            PlaceColumn(col, bounds);
        }
    }

    private void PlaceColumn(ColumnDecoration col, BoundsInt bounds)
    {
        if (col.bottomTile == null) { Debug.LogError($"[Column] '{col.label}' → bottomTile atanmamış!"); return; }
        if (col.middleTile == null) { Debug.LogError($"[Column] '{col.label}' → middleTile atanmamış!"); return; }
        if (col.topTile == null) { Debug.LogError($"[Column] '{col.label}' → topTile atanmamış!"); return; }

        int maxPossibleHeight = col.maxMiddleCount + 2;
        if (maxPossibleHeight > bounds.size.y)
        {
            Debug.LogError($"[Column] '{col.label}' → Max yükseklik ({maxPossibleHeight}) tilemap yüksekliğini ({bounds.size.y}) aşıyor!");
            return;
        }

        int placed = 0, skippedChance = 0, skippedBounds = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            if (Random.value > col.placementChance) { skippedChance++; continue; }

            int middleCount = Random.Range(
                Mathf.Max(0, col.minMiddleCount),
                Mathf.Max(col.minMiddleCount, col.maxMiddleCount) + 1);

            int totalHeight = middleCount + 2;
            int startY = GetColumnStartY(col, bounds, x, totalHeight);

            if (startY == int.MinValue) { skippedBounds++; continue; }

            backgroundTilemap.SetTile(new Vector3Int(x, startY, 0), col.bottomTile);
            for (int m = 1; m <= middleCount; m++)
                backgroundTilemap.SetTile(new Vector3Int(x, startY + m, 0), col.middleTile);
            backgroundTilemap.SetTile(new Vector3Int(x, startY + middleCount + 1, 0), col.topTile);

            placed++;
        }

        Debug.Log($"[Column] '{col.label}' → Yerleştirilen:{placed} | Şans:{skippedChance} | Sınır:{skippedBounds}");
    }

    private int GetColumnStartY(ColumnDecoration col, BoundsInt bounds, int x, int totalHeight)
    {
        int lastY = bounds.yMax - 1;
        switch (col.placementMode)
        {
            case ColumnPlacementMode.FromBottom:
                return (bounds.yMin + totalHeight - 1 <= lastY) ? bounds.yMin : int.MinValue;
            case ColumnPlacementMode.FromTop:
                int st = lastY - (totalHeight - 1);
                return (st >= bounds.yMin) ? st : int.MinValue;
            case ColumnPlacementMode.RandomY:
                int minS = bounds.yMin, maxS = lastY - (totalHeight - 1);
                return (minS <= maxS) ? Random.Range(minS, maxS + 1) : int.MinValue;
            case ColumnPlacementMode.AlignToExistingTiles:
                return FindLowestFilledY(bounds, x, totalHeight);
            default: return bounds.yMin;
        }
    }

    private int FindLowestFilledY(BoundsInt bounds, int x, int totalHeight)
    {
        int lastY = bounds.yMax - 1;
        for (int y = bounds.yMin; y <= lastY; y++)
        {
            if (backgroundTilemap.HasTile(new Vector3Int(x, y, 0)))
                return (y + totalHeight - 1 <= lastY) ? y : int.MinValue;
        }
        return int.MinValue;
    }

    #endregion

    // ═══════════════════════════════════════════════════════════
    #region 3x3 Block Placement

    private void PlaceAllBlocks(HashSet<Vector2Int> occupied)
    {
        // Bounds'u bir kez al — block yerleştirme bu bounds dışına çıkamaz
        backgroundTilemap.CompressBounds();
        BoundsInt bounds = backgroundTilemap.cellBounds;

        Debug.Log($"[Block] Tilemap bounds → X:[{bounds.xMin}~{bounds.xMax - 1}] Y:[{bounds.yMin}~{bounds.yMax - 1}]");

        foreach (var block in blocks)
        {
            if (!block.enabled) continue;
            block.SyncTilesArray();
            PlaceBlock(block, bounds, occupied);
        }
    }

    private void PlaceBlock(BlockDecoration block, BoundsInt bounds, HashSet<Vector2Int> occupied)
    {
        // ── Tile null kontrolü ─────────────────────────────────
        for (int i = 0; i < 9; i++)
        {
            if (block.tiles[i] != null) continue;
            Debug.LogError($"[Block] '{block.label}' → tile{i + 1} ({GetTilePositionLabel(i)}) atanmamış!");
            return;
        }

        // ── Sınır kontrolü ────────────────────────────────────
        // Block 3 tile genişlik + 3 tile yükseklik.
        // bounds.xMax ve bounds.yMax exclusive → son geçerli başlangıç: xMax-3, yMax-3
        // Örnek: xMin=0, xMax=10 → geçerli x: 0..7 (x+2 = 9 = xMax-1 ✓)
        int maxStartX = bounds.xMax - 3;
        int maxStartY = bounds.yMax - 3;

        if (maxStartX < bounds.xMin || maxStartY < bounds.yMin)
        {
            Debug.LogError($"[Block] '{block.label}' → Tilemap 3x3 block için çok küçük! " +
                           $"En az 3 genişlik ve 3 yükseklik gerekli.");
            return;
        }

        // ── Aday pozisyon listesi oluştur ──────────────────────
        // Sadece geçerli konumları listeye al, sonra karıştır.
        // Bu sayede placement tamamen rastgele ve bounds dışına çıkmaz.
        var candidates = BuildCandidateList(block, bounds, maxStartX, maxStartY);
        Shuffle(candidates);

        int placed = 0, skippedChance = 0, skippedOverlap = 0;

        foreach (var pos in candidates)
        {
            // Şans kontrolü
            if (Random.value > block.placementChance) { skippedChance++; continue; }

            // Overlap kontrolü — 9 pozisyonun hiçbiri dolu olmamalı
            if (block.preventOverlap && IsAreaOccupied(pos.x, pos.y, occupied))
            {
                skippedOverlap++;
                continue;
            }

            // Yerleştir
            PlaceBlockAt(block, pos.x, pos.y);

            // Kullanılan 9 pozisyonu işaretle
            if (block.preventOverlap)
                MarkArea(pos.x, pos.y, occupied);

            placed++;
        }

        Debug.Log($"[Block] '{block.label}' → Yerleştirilen:{placed} | " +
                  $"Şans:{skippedChance} | Overlap:{skippedOverlap}");
    }

    /// <summary>
    /// Block'un başlangıç noktası olabilecek (x, y) pozisyonlarını
    /// placement mode'a göre filtreler ve liste döndürür.
    /// </summary>
    private List<Vector2Int> BuildCandidateList(BlockDecoration block, BoundsInt bounds,
                                                 int maxStartX, int maxStartY)
    {
        var list = new List<Vector2Int>();

        for (int x = bounds.xMin; x <= maxStartX; x++)
        {
            for (int y = bounds.yMin; y <= maxStartY; y++)
            {
                switch (block.placementMode)
                {
                    case BlockPlacementMode.FromBottom:
                        if (y == bounds.yMin) list.Add(new Vector2Int(x, y));
                        break;

                    case BlockPlacementMode.FromTop:
                        // Block'un üst tile'ı en üst satırda olacak: y + 2 = bounds.yMax - 1
                        if (y == maxStartY) list.Add(new Vector2Int(x, y));
                        break;

                    case BlockPlacementMode.Anywhere:
                    case BlockPlacementMode.RandomY:
                        list.Add(new Vector2Int(x, y));
                        break;
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Verilen (x, y) sol-alt köşesine 3x3 block'u yerleştirir.
    ///
    /// Inspector sırası → Tilemap konumu:
    ///   tile1(1) tile2(2) tile3(3)  →  y+2 (üst sıra)
    ///   tile4(4) tile5(5) tile6(6)  →  y+1 (orta sıra)
    ///   tile7(7) tile8(8) tile9(9)  →  y+0 (alt sıra)
    /// </summary>
    private void PlaceBlockAt(BlockDecoration block, int x, int y)
    {
        // Alt sıra — inspector'daki 7, 8, 9
        backgroundTilemap.SetTile(new Vector3Int(x, y, 0), block.tiles[6]);
        backgroundTilemap.SetTile(new Vector3Int(x + 1, y, 0), block.tiles[7]);
        backgroundTilemap.SetTile(new Vector3Int(x + 2, y, 0), block.tiles[8]);

        // Orta sıra — inspector'daki 4, 5, 6
        backgroundTilemap.SetTile(new Vector3Int(x, y + 1, 0), block.tiles[3]);
        backgroundTilemap.SetTile(new Vector3Int(x + 1, y + 1, 0), block.tiles[4]);
        backgroundTilemap.SetTile(new Vector3Int(x + 2, y + 1, 0), block.tiles[5]);

        // Üst sıra — inspector'daki 1, 2, 3
        backgroundTilemap.SetTile(new Vector3Int(x, y + 2, 0), block.tiles[0]);
        backgroundTilemap.SetTile(new Vector3Int(x + 1, y + 2, 0), block.tiles[1]);
        backgroundTilemap.SetTile(new Vector3Int(x + 2, y + 2, 0), block.tiles[2]);
    }

    // ── Overlap Yardımcıları ───────────────────────────────────

    /// <summary>
    /// 3x3 alandaki 9 pozisyondan herhangi biri dolu mu?
    /// </summary>
    private bool IsAreaOccupied(int x, int y, HashSet<Vector2Int> occupied)
    {
        for (int dx = 0; dx < 3; dx++)
            for (int dy = 0; dy < 3; dy++)
                if (occupied.Contains(new Vector2Int(x + dx, y + dy)))
                    return true;
        return false;
    }

    /// <summary>
    /// 3x3 alandaki 9 pozisyonu dolu olarak işaretle.
    /// </summary>
    private void MarkArea(int x, int y, HashSet<Vector2Int> occupied)
    {
        for (int dx = 0; dx < 3; dx++)
            for (int dy = 0; dy < 3; dy++)
                occupied.Add(new Vector2Int(x + dx, y + dy));
    }

    // ── Shuffle ───────────────────────────────────────────────

    /// <summary>
    /// Fisher-Yates shuffle — listeyi yerinde karıştırır.
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private string GetTilePositionLabel(int i)
    {
        string[] labels =
        {
            "Üst-Sol",  "Üst-Orta",  "Üst-Sağ",
            "Orta-Sol", "Merkez",    "Orta-Sağ",
            "Alt-Sol",  "Alt-Orta",  "Alt-Sağ"
        };
        return i >= 0 && i < labels.Length ? labels[i] : i.ToString();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════
    #region Weighted Random Utility

    private float CalculateTotalWeight(List<TileEntry> list)
    {
        float total = 0f;
        foreach (var e in list) total += Mathf.Max(0f, e.weight);
        return total;
    }

    private TileBase PickFromList(List<TileEntry> list, float totalWeight)
    {
        float roll = Random.Range(0f, totalWeight), cumulative = 0f;
        foreach (var entry in list)
        {
            if (entry.tile == null || entry.weight <= 0f) continue;
            cumulative += entry.weight;
            if (roll <= cumulative) return entry.tile;
        }
        for (int i = list.Count - 1; i >= 0; i--)
            if (list[i].tile != null) return list[i].tile;
        return null;
    }

    #endregion

    // ═══════════════════════════════════════════════════════════
#if UNITY_EDITOR
    private void OnValidate()
    {
        _totalWeight = 0f;
        if (tileEntries != null)
            foreach (var e in tileEntries) _totalWeight += Mathf.Max(0f, e.weight);

        if (blocks != null)
            foreach (var b in blocks)
                b.SyncTilesArray();
    }
#endif
}

// ═══════════════════════════════════════════════════════════════
#region Data Classes

[System.Serializable]
public class TileEntry
{
    public string label = "Tile";
    public TileBase tile;
    [Min(0f)] public float weight = 1f;
}

// ── Column ──────────────────────────────────────────────────────

public enum ColumnPlacementMode
{
    FromBottom, FromTop, RandomY, AlignToExistingTiles
}

[System.Serializable]
public class ColumnDecoration
{
    public string label = "Column";
    public bool enabled = true;

    [Header("Tile Atamaları")]
    public TileBase bottomTile;
    public TileBase middleTile;
    public TileBase topTile;

    [Header("Yükseklik (Orta Tile Tekrar Sayısı)")]
    [Min(0)] public int minMiddleCount = 1;
    [Min(0)] public int maxMiddleCount = 4;

    [Header("Yerleşim")]
    public ColumnPlacementMode placementMode = ColumnPlacementMode.FromBottom;
    [Range(0f, 1f)] public float placementChance = 0.15f;
}

// ── 3x3 Block ───────────────────────────────────────────────────

public enum BlockPlacementMode
{
    [Tooltip("Tilemap içinde herhangi bir geçerli konuma")]
    Anywhere,
    [Tooltip("Sadece alt sınırdan")]
    FromBottom,
    [Tooltip("Sadece üst sınırdan")]
    FromTop,
    [Tooltip("Tamamen rastgele Y (Anywhere ile aynı etki)")]
    RandomY
}

[System.Serializable]
public class BlockDecoration
{
    public string label = "3x3 Block";
    public bool enabled = true;

    [Header("3x3 Tile Atamaları  (1-2-3 / 4-5-6 / 7-8-9)")]
    [Tooltip("1 - Üst Sol")] public TileBase tile1;
    [Tooltip("2 - Üst Orta")] public TileBase tile2;
    [Tooltip("3 - Üst Sağ")] public TileBase tile3;
    [Space(2)]
    [Tooltip("4 - Orta Sol")] public TileBase tile4;
    [Tooltip("5 - Merkez")] public TileBase tile5;
    [Tooltip("6 - Orta Sağ")] public TileBase tile6;
    [Space(2)]
    [Tooltip("7 - Alt Sol")] public TileBase tile7;
    [Tooltip("8 - Alt Orta")] public TileBase tile8;
    [Tooltip("9 - Alt Sağ")] public TileBase tile9;

    // Runtime dizisi — SyncTilesArray() ile güncellenir
    [HideInInspector] public TileBase[] tiles = new TileBase[9];

    [Header("Yerleşim")]
    public BlockPlacementMode placementMode = BlockPlacementMode.Anywhere;
    [Range(0f, 1f)] public float placementChance = 0.08f;

    [Tooltip("True: Block'lar kendi aralarında ve diğer block tipleriyle üst üste gelmez")]
    public bool preventOverlap = true;

    /// <summary>Inspector alanlarını tiles[] dizisine senkronize eder.</summary>
    public void SyncTilesArray()
    {
        if (tiles == null || tiles.Length != 9) tiles = new TileBase[9];
        tiles[0] = tile1; tiles[1] = tile2; tiles[2] = tile3;
        tiles[3] = tile4; tiles[4] = tile5; tiles[5] = tile6;
        tiles[6] = tile7; tiles[7] = tile8; tiles[8] = tile9;
    }
}

#endregion