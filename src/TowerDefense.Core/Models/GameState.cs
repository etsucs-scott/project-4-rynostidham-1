namespace TowerDefense.Core.Models;

/// <summary>
/// Snapshot of the full game state. Serialized to JSON for save/load.
/// </summary>
public class GameState
{
    public int Wave { get; set; } = 1;
    public int Gold { get; set; } = 150;
    public int BaseHealth { get; set; } = 20;
    public int Score { get; set; } = 0;
    public bool IsGameOver { get; set; } = false;
    public bool IsVictory { get; set; } = false;
    public int TotalWaves { get; set; } = 10;

    /// <summary>Saved tower placements for file persistence.</summary>
    public List<SavedTower> SavedTowers { get; set; } = new();
}

/// <summary>Serializable representation of a placed tower (no circular refs).</summary>
public class SavedTower
{
    public string TowerType { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Level { get; set; } = 1;
}

/// <summary>Leaderboard score entry.</summary>
public class ScoreEntry : IComparable<ScoreEntry>
{
    public string PlayerName { get; set; } = "Player";
    public int Score { get; set; }
    public int Wave { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>Sort descending by score.</summary>
    public int CompareTo(ScoreEntry? other)
    {
        if (other == null) return -1;
        int cmp = other.Score.CompareTo(Score); // descending
        return cmp != 0 ? cmp : other.Date.CompareTo(Date);
    }
}
