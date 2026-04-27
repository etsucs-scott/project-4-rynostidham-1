using TowerDefense.Core.Models;

namespace TowerDefense.Core.Services;

/// <summary>
/// Central game engine. Owns the game loop (Timer-based tick), all game state,
/// and coordinates towers, enemies, projectiles, and waves.
/// Uses Dictionary for O(1) grid access, Dictionary for tower registry,
/// and List for active enemies and projectiles.
/// </summary>
public class GameEngine : IDisposable
{
    // --- Data structures ---
    /// <summary>Grid map: (x,y) → Cell. Dictionary for O(1) lookup.</summary>
    public Dictionary<(int, int), Cell> Grid { get; } = new();
    /// <summary>Tower registry: Guid → Tower for fast tower lookup/removal.</summary>
    public Dictionary<Guid, Tower> Towers { get; } = new();
    public List<Enemy> Enemies { get; } = new();
    public List<Projectile> Projectiles { get; } = new();

    // --- Services ---
    private readonly WaveManager _waveManager;
    private readonly PathFinder _pathFinder;
    private readonly SaveGameService _saveService;
    private Timer? _timer;

    // --- State ---
    public GameState State { get; private set; } = new();
    public List<(int x, int y)> EnemyPath { get; private set; } = new();
    public (int x, int y) SpawnPoint { get; private set; }
    public (int x, int y) EndPoint { get; private set; }
    public bool WaveInProgress { get; private set; } = false;

    public int MapWidth => 20;
    public int MapHeight => 12;

    /// <summary>Raised every tick so Blazor can call StateHasChanged.</summary>
    public event Action? OnStateChanged;

    public GameEngine(WaveManager waveManager, PathFinder pathFinder, SaveGameService saveService)
    {
        _waveManager = waveManager;
        _pathFinder = pathFinder;
        _saveService = saveService;
        BuildMap();
        RecalculatePath();
        State.TotalWaves = _waveManager.TotalWaves;
    }

    /// <summary>Start the game loop at ~20 fps.</summary>
    public void Start()
    {
        _timer = new Timer(_ => Tick(), null, 0, 50);
    }

    /// <summary>Main tick: move enemies, fire towers, move projectiles, check wave.</summary>
    private void Tick()
    {
        if (State.IsGameOver || State.IsVictory) return;

        const float delta = 0.05f; // 50ms per tick

        if (WaveInProgress)
        {
            SpawnEnemies(delta);
            MoveEnemies(delta);
            FireTowers(delta);
            MoveProjectiles(delta);
            CheckWaveComplete();
        }

        OnStateChanged?.Invoke();
    }

    /// <summary>Spawn enemies from the wave manager this tick.</summary>
    private void SpawnEnemies(float delta)
    {
        foreach (var enemy in _waveManager.Tick(delta, SpawnPoint))
            Enemies.Add(enemy);
    }

    /// <summary>Move all active enemies along their path. Enemies at the end deal base damage.</summary>
    private void MoveEnemies(float delta)
    {
        var toRemove = new List<Enemy>();
        foreach (var enemy in Enemies)
        {
            if (enemy.IsDead) { toRemove.Add(enemy); continue; }
            if (enemy.PathIndex >= EnemyPath.Count)
            {
                // Reached the base
                State.BaseHealth -= enemy.BaseDamage;
                State.Score = Math.Max(0, State.Score - 5);
                toRemove.Add(enemy);
                if (State.BaseHealth <= 0)
                {
                    State.BaseHealth = 0;
                    State.IsGameOver = true;
                }
                continue;
            }

            // Move toward current waypoint
            var waypoint = EnemyPath[enemy.PathIndex];
            float tx = waypoint.x, ty = waypoint.y;
            float dx = tx - enemy.X, dy = ty - enemy.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            float step = enemy.Speed * delta;

            if (dist <= step) { enemy.X = tx; enemy.Y = ty; enemy.PathIndex++; }
            else { enemy.X += (dx / dist) * step; enemy.Y += (dy / dist) * step; }
        }
        foreach (var e in toRemove) Enemies.Remove(e);
    }

    /// <summary>Each tower attacks the nearest enemy in range, if off cooldown.</summary>
    private void FireTowers(float delta)
    {
        foreach (var tower in Towers.Values)
        {
            // Use PriorityQueue to pick highest-priority target (furthest along path)
            var pq = new PriorityQueue<Enemy, int>();
            foreach (var enemy in Enemies)
                if (!enemy.IsDead && tower.InRange(enemy))
                    pq.Enqueue(enemy, -enemy.PathIndex); // negative = highest index = furthest

            if (pq.TryDequeue(out var target, out _))
            {
                var proj = tower.TryAttack(target, delta);
                if (proj != null) Projectiles.Add(proj);
            }
            else
            {
                // Still tick the cooldown even if no target
                if (tower.CooldownRemaining > 0) tower.CooldownRemaining -= delta;
            }
        }
    }

    /// <summary>Move all projectiles. Remove those that hit or whose target died.</summary>
    private void MoveProjectiles(float delta)
    {
        var toRemove = new List<Projectile>();
        foreach (var proj in Projectiles)
        {
            proj.Move(delta);
            if (proj.HasHit)
            {
                if (proj.Target.IsDead)
                {
                    State.Gold += proj.Target.GoldReward;
                    State.Score += proj.Target.GoldReward * 10;
                }
                toRemove.Add(proj);
            }
        }
        foreach (var p in toRemove) Projectiles.Remove(p);
    }

    private void CheckWaveComplete()
    {
        if (_waveManager.IsCurrentWaveComplete && Enemies.Count == 0)
        {
            WaveInProgress = false;
            if (!_waveManager.HasMoreWaves)
                State.IsVictory = true;
        }
    }

    /// <summary>Attempt to place a tower at (x,y). Returns false if cell is invalid or occupied.</summary>
    public bool PlaceTower(Tower tower, int x, int y)
    {
        if (!Grid.TryGetValue((x, y), out var cell)) return false;
        if (cell.Type != CellType.Buildable || cell.Tower != null) return false;
        if (State.Gold < tower.Cost) return false;

        cell.Tower = tower;
        tower.X = x; tower.Y = y;
        Towers[tower.Id] = tower;
        State.Gold -= tower.Cost;
        return true;
    }

    /// <summary>Remove a tower from the board and refund half its cost.</summary>
    public bool RemoveTower(Guid towerId)
    {
        if (!Towers.TryGetValue(towerId, out var tower)) return false;
        if (Grid.TryGetValue((tower.X, tower.Y), out var cell)) cell.Tower = null;
        Towers.Remove(towerId);
        State.Gold += tower.Cost / 2;
        return true;
    }

    /// <summary>Start the next wave of enemies.</summary>
    public void StartNextWave()
    {
        if (WaveInProgress || State.IsGameOver || State.IsVictory) return;
        if (_waveManager.StartNextWave())
        {
            State.Wave++;
            WaveInProgress = true;
        }
    }

    /// <summary>Recalculate enemy path after tower placements change.</summary>
    public void RecalculatePath()
    {
        var path = _pathFinder.FindPath(Grid, SpawnPoint, EndPoint);
        if (path != null) EnemyPath = path;
    }

    /// <summary>Save the current game state to disk.</summary>
    public async Task SaveAsync() => await _saveService.SaveAsync(State, Towers);

    /// <summary>Load a previously saved game state from disk.</summary>
    public async Task LoadAsync()
    {
        var (state, towers) = await _saveService.LoadAsync();
        State = state;
        // Restore towers to the grid
        foreach (var t in towers)
        {
            Towers[t.Id] = t;
            if (Grid.TryGetValue((t.X, t.Y), out var cell))
                cell.Tower = t;
        }
    }

    /// <summary>
    /// Build the 20×12 game map. Path cells form a corridor; the rest are Buildable.
    /// Enemies travel from the left edge to the right edge along a fixed winding path.
    /// </summary>
    private void BuildMap()
    {
        // Fill everything as buildable first
        for (int x = 0; x < MapWidth; x++)
            for (int y = 0; y < MapHeight; y++)
                Grid[(x, y)] = new Cell(x, y, CellType.Buildable);

        // Define a winding path of (x,y) coordinates
        var pathCoords = new List<(int, int)>
        {
            (0,2),(1,2),(2,2),(3,2),(4,2),(5,2),(5,3),(5,4),(5,5),(5,6),
            (6,6),(7,6),(8,6),(9,6),(9,5),(9,4),(9,3),(9,2),(10,2),(11,2),
            (12,2),(12,3),(12,4),(12,5),(12,6),(12,7),(12,8),(13,8),(14,8),
            (15,8),(15,7),(15,6),(15,5),(15,4),(15,3),(15,2),(16,2),(17,2),
            (18,2),(19,2)
        };

        foreach (var (x, y) in pathCoords)
            Grid[(x, y)].Type = CellType.Path;

        SpawnPoint = (0, 2);
        EndPoint = (19, 2);
    }

    public void Dispose() => _timer?.Dispose();
}
