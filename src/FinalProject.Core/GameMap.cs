namespace FinalProject.Core;

public class GameMap
{
    public int Rows { get; }
    public int Cols { get; }

    public HashSet<GridPosition> Path { get; } = new();

    public GameMap(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
    }

    public bool IsWalkable(GridPosition pos) => !Path.Contains(pos);

    public IEnumerable<GridPosition> GetNeighbors(GridPosition pos)
    {
        var dirs = new (int r, int c)[] { (1,0), (-1,0), (0,1), (0,-1) };
        foreach (var (dr, dc) in dirs)
        {
            var next = new GridPosition(pos.Row + dr, pos.Col + dc);
            if (next.Row >= 0 && next.Row < Rows && next.Col >= 0 && next.Col < Cols)
                yield return next;
        }
    }
}
