using TowerDefense.Core.Models;
using TowerDefense.Core.Services;
using Xunit;

namespace TowerDefense.Tests;

/// <summary>Unit tests for PathFinder — BFS path finding on the game grid.</summary>
public class PathFinderTests
{
    private static Dictionary<(int, int), Cell> MakeGrid(int w, int h, IEnumerable<(int, int)> pathCells)
    {
        var grid = new Dictionary<(int, int), Cell>();
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                grid[(x, y)] = new Cell(x, y, CellType.Buildable);
        foreach (var (x, y) in pathCells)
            grid[(x, y)].Type = CellType.Path;
        return grid;
    }

    [Fact]
    public void FindPath_ReturnsStraightPath()
    {
        var grid = MakeGrid(5, 1, new[] { (0,0),(1,0),(2,0),(3,0),(4,0) });
        var pf = new PathFinder();
        var path = pf.FindPath(grid, (0, 0), (4, 0));
        Assert.NotNull(path);
        Assert.Equal(5, path!.Count);
        Assert.Equal((0, 0), path.First());
        Assert.Equal((4, 0), path.Last());
    }

    [Fact]
    public void FindPath_ReturnsNull_WhenBlocked()
    {
        var grid = MakeGrid(3, 1, new[] { (0,0),(2,0) }); // gap at (1,0)
        var pf = new PathFinder();
        var path = pf.FindPath(grid, (0, 0), (2, 0));
        Assert.Null(path);
    }

    [Fact]
    public void FindPath_FindsAroundObstacle()
    {
        // L-shaped path
        var grid = MakeGrid(3, 3, new[] { (0,0),(0,1),(0,2),(1,2),(2,2) });
        var pf = new PathFinder();
        var path = pf.FindPath(grid, (0, 0), (2, 2));
        Assert.NotNull(path);
        Assert.Equal((0, 0), path!.First());
        Assert.Equal((2, 2), path.Last());
    }

    [Fact]
    public void FindPath_StartEqualsEnd_ReturnsSingleCell()
    {
        var grid = MakeGrid(3, 3, new[] { (1,1) });
        var pf = new PathFinder();
        var path = pf.FindPath(grid, (1, 1), (1, 1));
        Assert.NotNull(path);
        Assert.Single(path!);
    }
}

/// <summary>Unit tests for Tower models — range, damage, and attack logic.</summary>
public class TowerTests
{
    [Fact]
    public void ArrowTower_InRange_WhenEnemyClose()
    {
        var tower = new ArrowTower { X = 5, Y = 5 };
        var enemy = new Goblin { X = 6, Y = 5 };
        Assert.True(tower.InRange(enemy));
    }

    [Fact]
    public void ArrowTower_NotInRange_WhenEnemyFar()
    {
        var tower = new ArrowTower { X = 0, Y = 0 };
        var enemy = new Goblin { X = 10, Y = 10 };
        Assert.False(tower.InRange(enemy));
    }

    [Fact]
    public void MageTower_DamageScalesWithLevel()
    {
        var tower = new MageTower();
        int dmgL1 = tower.Damage;
        tower.Upgrade();
        Assert.True(tower.Damage > dmgL1);
    }

    [Fact]
    public void TryAttack_ReturnsProjectile_WhenCooldownExpired()
    {
        var tower = new ArrowTower { X = 0, Y = 0 };
        var enemy = new Goblin { X = 1, Y = 0 };
        tower.CooldownRemaining = 0f;
        var proj = tower.TryAttack(enemy, 0.05f);
        Assert.NotNull(proj);
    }

    [Fact]
    public void TryAttack_ReturnsNull_WhenOnCooldown()
    {
        var tower = new ArrowTower { X = 0, Y = 0 };
        var enemy = new Goblin { X = 1, Y = 0 };
        tower.CooldownRemaining = 5f;
        var proj = tower.TryAttack(enemy, 0.05f);
        Assert.Null(proj);
    }
}

/// <summary>Unit tests for Enemy models — health, damage, and death logic.</summary>
public class EnemyTests
{
    [Fact]
    public void Enemy_TakeDamage_ReducesHealth()
    {
        var goblin = new Goblin();
        int startHp = goblin.CurrentHealth;
        goblin.TakeDamage(10);
        Assert.Equal(startHp - 10, goblin.CurrentHealth);
    }

    [Fact]
    public void Enemy_IsDead_AfterFatalDamage()
    {
        var goblin = new Goblin();
        goblin.TakeDamage(goblin.MaxHealth);
        Assert.True(goblin.IsDead);
    }

    [Fact]
    public void Enemy_TakeDamage_CannotGoBelowZero()
    {
        var goblin = new Goblin();
        goblin.TakeDamage(goblin.MaxHealth * 10);
        Assert.Equal(0, goblin.CurrentHealth);
    }
}

/// <summary>Unit tests for WaveManager — wave spawning logic.</summary>
public class WaveManagerTests
{
    [Fact]
    public void WaveManager_HasMoreWaves_Initially()
    {
        var wm = new WaveManager();
        Assert.True(wm.HasMoreWaves);
    }

    [Fact]
    public void WaveManager_StartNextWave_ReturnsFalse_WhenExhausted()
    {
        var wm = new WaveManager();
        // Drain all waves
        while (wm.StartNextWave()) { }
        Assert.False(wm.StartNextWave());
    }
}

/// <summary>Unit tests for SaveGameService — serialization and error handling.</summary>
public class SaveGameServiceTests
{
    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesScore()
    {
        var dir = Path.GetTempPath();
        var svc = new SaveGameService(dir);
        var state = new GameState { Score = 9999, Wave = 5, Gold = 250 };
        var towers = new Dictionary<Guid, Tower> { { Guid.NewGuid(), new ArrowTower { X = 3, Y = 4 } } };

        await svc.SaveAsync(state, towers);
        var (loaded, loadedTowers) = await svc.LoadAsync();

        Assert.Equal(9999, loaded.Score);
        Assert.Equal(5, loaded.Wave);
        Assert.Single(loadedTowers);
        Assert.IsType<ArrowTower>(loadedTowers[0]);
    }

    [Fact]
    public async Task Load_ReturnsDefaultState_WhenNoFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var svc = new SaveGameService(dir);
        var (state, towers) = await svc.LoadAsync();
        Assert.Equal(1, state.Wave);
        Assert.Empty(towers);
    }
}
