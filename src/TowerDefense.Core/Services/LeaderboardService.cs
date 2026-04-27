using TowerDefense.Core.Models;

namespace TowerDefense.Core.Services;

/// <summary>
/// Manages the top-score leaderboard. Uses SortedSet for automatic ordering.
/// Persists to a CSV file; handles missing/corrupt files gracefully.
/// </summary>
public class LeaderboardService
{
    private readonly string _csvPath;
    private readonly SortedSet<ScoreEntry> _scores = new();

    public LeaderboardService(string saveDirectory = ".")
    {
        _csvPath = Path.Combine(saveDirectory, "leaderboard.csv");
        LoadFromFile();
    }

    /// <summary>All scores in descending order (SortedSet maintains this automatically).</summary>
    public IEnumerable<ScoreEntry> TopScores => _scores.Take(10);

    /// <summary>Add a score entry and persist to disk.</summary>
    public void AddScore(ScoreEntry entry)
    {
        _scores.Add(entry);
        SaveToFile();
    }

    /// <summary>Read scores from CSV. Silently ignores missing or malformed files.</summary>
    private void LoadFromFile()
    {
        if (!File.Exists(_csvPath)) return;
        try
        {
            foreach (var line in File.ReadAllLines(_csvPath))
            {
                var parts = line.Split(',');
                if (parts.Length < 4) continue;
                if (!int.TryParse(parts[1], out int score)) continue;
                if (!int.TryParse(parts[2], out int wave)) continue;
                if (!DateTime.TryParse(parts[3], out DateTime date)) continue;

                _scores.Add(new ScoreEntry
                {
                    PlayerName = parts[0],
                    Score = score,
                    Wave = wave,
                    Date = date
                });
            }
        }
        catch (IOException)
        {
            // Leaderboard is non-critical — swallow I/O errors
        }
    }

    /// <summary>Write top 100 scores to CSV.</summary>
    private void SaveToFile()
    {
        try
        {
            var lines = _scores.Take(100)
                .Select(e => $"{e.PlayerName},{e.Score},{e.Wave},{e.Date:O}");
            File.WriteAllLines(_csvPath, lines);
        }
        catch (IOException)
        {
            // Non-critical — don't crash the game over a leaderboard write failure
        }
    }
}
