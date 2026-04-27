using System.Text.Json;
using TowerDefense.Core.Models;

namespace TowerDefense.Core.Services;

/// <summary>
/// Persists game state to/from a JSON file.
/// All file operations are wrapped in try/catch to prevent crashes on missing or corrupt files.
/// </summary>
public class SaveGameService
{
    private readonly string _savePath;
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public SaveGameService(string saveDirectory = ".")
    {
        _savePath = Path.Combine(saveDirectory, "savegame.json");
    }

    /// <summary>Serialize and write game state + tower placements to disk.</summary>
    public async Task SaveAsync(GameState state, Dictionary<Guid, Tower> towers)
    {
        try
        {
            // Build a serializable snapshot (no circular refs)
            state.SavedTowers = towers.Values.Select(t => new SavedTower
            {
                TowerType = t.GetType().Name,
                X = t.X,
                Y = t.Y,
                Level = t.Level
            }).ToList();

            var json = JsonSerializer.Serialize(state, _opts);
            await File.WriteAllTextAsync(_savePath, json);
        }
        catch (IOException ex)
        {
            throw new SaveGameException("Failed to write save file.", ex);
        }
    }

    /// <summary>
    /// Read and deserialize the save file. Returns a fresh state if no file exists.
    /// Throws SaveGameException on corrupt data.
    /// </summary>
    public async Task<(GameState State, List<Tower> Towers)> LoadAsync()
    {
        if (!File.Exists(_savePath))
            return (new GameState(), new List<Tower>());

        try
        {
            var json = await File.ReadAllTextAsync(_savePath);
            var state = JsonSerializer.Deserialize<GameState>(json, _opts)
                ?? throw new SaveGameException("Save file was empty or invalid.");

            // Reconstruct tower objects from saved type names
            var towers = state.SavedTowers.Select(s => RehydrateTower(s)).ToList();
            return (state, towers);
        }
        catch (JsonException ex)
        {
            throw new SaveGameException("Save file is corrupted.", ex);
        }
        catch (IOException ex)
        {
            throw new SaveGameException("Could not read save file.", ex);
        }
    }

    /// <summary>Reconstruct a Tower instance from its saved type name.</summary>
    private static Tower RehydrateTower(SavedTower saved)
    {
        Tower tower = saved.TowerType switch
        {
            nameof(ArrowTower) => new ArrowTower(),
            nameof(MageTower) => new MageTower(),
            nameof(CannonTower) => new CannonTower(),
            _ => throw new SaveGameException($"Unknown tower type: {saved.TowerType}")
        };
        tower.X = saved.X;
        tower.Y = saved.Y;
        return tower;
    }
}

/// <summary>Custom exception for save/load failures. Keeps error messaging user-friendly.</summary>
public class SaveGameException : Exception
{
    public SaveGameException(string message) : base(message) { }
    public SaveGameException(string message, Exception inner) : base(message, inner) { }
}
