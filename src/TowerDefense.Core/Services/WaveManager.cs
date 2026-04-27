using TowerDefense.Core.Models;

namespace TowerDefense.Core.Services;

/// <summary>Defines the composition of a single wave.</summary>
public record WaveDefinition(int Wave, List<(Type EnemyType, int Count)> Enemies);

/// <summary>
/// Manages wave definitions and enemy spawn logic.
/// Uses a Queue for pending waves and PriorityQueue for enemy targeting priority.
/// </summary>
public class WaveManager
{
    private readonly Queue<WaveDefinition> _pendingWaves = new();
    private readonly List<WaveDefinition> _allWaves;
    private float _spawnTimer = 0f;
    private const float SpawnInterval = 0.8f;
    private Queue<Enemy>? _currentSpawnQueue;

    public int TotalWaves => _allWaves.Count;
    public bool HasMoreWaves => _pendingWaves.Count > 0 || (_currentSpawnQueue?.Count > 0);

    public WaveManager()
    {
        _allWaves = BuildWaveDefinitions();
        foreach (var wave in _allWaves)
            _pendingWaves.Enqueue(wave);
    }

    /// <summary>Start the next wave. Returns false if no more waves remain.</summary>
    public bool StartNextWave()
    {
        if (_pendingWaves.Count == 0) return false;
        var wave = _pendingWaves.Dequeue();
        _currentSpawnQueue = BuildSpawnQueue(wave);
        return true;
    }

    /// <summary>
    /// Tick spawn logic. Returns any newly spawned enemies this frame.
    /// </summary>
    public IEnumerable<Enemy> Tick(float deltaTime, (int x, int y) spawnPoint)
    {
        if (_currentSpawnQueue == null || _currentSpawnQueue.Count == 0)
            yield break;

        _spawnTimer -= deltaTime;
        if (_spawnTimer > 0f) yield break;

        _spawnTimer = SpawnInterval;
        var enemy = _currentSpawnQueue.Dequeue();
        enemy.X = spawnPoint.x;
        enemy.Y = spawnPoint.y;
        yield return enemy;
    }

    public bool IsCurrentWaveComplete => _currentSpawnQueue == null || _currentSpawnQueue.Count == 0;

    /// <summary>Build the ordered spawn queue for a given wave definition.</summary>
    private static Queue<Enemy> BuildSpawnQueue(WaveDefinition wave)
    {
        var queue = new Queue<Enemy>();
        foreach (var (type, count) in wave.Enemies)
        {
            for (int i = 0; i < count; i++)
            {
                // Use Activator to instantiate enemy by type (supports extensibility)
                var enemy = (Enemy)Activator.CreateInstance(type)!;
                queue.Enqueue(enemy);
            }
        }
        return queue;
    }

    /// <summary>
    /// Hardcoded wave definitions. Wave 10 ends with a boss.
    /// Difficulty scales by adding more enemies and tougher types each wave.
    /// </summary>
    private static List<WaveDefinition> BuildWaveDefinitions() => new()
    {
        new(1,  new() { (typeof(Goblin), 5) }),
        new(2,  new() { (typeof(Goblin), 8) }),
        new(3,  new() { (typeof(Goblin), 6), (typeof(Troll), 2) }),
        new(4,  new() { (typeof(Goblin), 10), (typeof(Troll), 3) }),
        new(5,  new() { (typeof(Troll), 5) }),
        new(6,  new() { (typeof(Goblin), 12), (typeof(Troll), 4) }),
        new(7,  new() { (typeof(Goblin), 8), (typeof(Troll), 6) }),
        new(8,  new() { (typeof(Troll), 8), (typeof(Goblin), 10) }),
        new(9,  new() { (typeof(Goblin), 15), (typeof(Troll), 8) }),
        new(10, new() { (typeof(Goblin), 10), (typeof(Troll), 5), (typeof(BossOrc), 1) }),
    };
}
