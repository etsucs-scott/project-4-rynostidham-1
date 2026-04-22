namespace FinalProject.Core;

public class GameState
{
    public int Lives { get; set; } = 20;
    public int Gold { get; set; } = 100;
    public int CurrentWave { get; set; } = 0;

    public Dictionary<int, Tower> Towers { get; } = new();
    public List<Enemy> Enemies { get; } = new();
    public Queue<Enemy> SpawnQueue { get; } = new();
    public HashSet<GridPosition> OccupiedTiles { get; } = new();

    public GameMap Map { get; set; } = new GameMap(10, 10);
}
