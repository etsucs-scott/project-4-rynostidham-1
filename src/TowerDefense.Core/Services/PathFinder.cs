using TowerDefense.Core.Models;

namespace TowerDefense.Core.Services;

/// <summary>
/// Finds paths through the game map using Breadth-First Search.
/// Uses a Dictionary-based grid graph for O(1) cell lookup.
/// </summary>
public class PathFinder
{
    /// <summary>
    /// Find a path from start to end through walkable cells.
    /// Returns ordered list of (x,y) waypoints, or null if no path exists.
    /// </summary>
    public List<(int x, int y)>? FindPath(
        Dictionary<(int, int), Cell> grid,
        (int x, int y) start,
        (int x, int y) end)
    {
        // Standard BFS with parent tracking
        var queue = new Queue<(int x, int y)>();
        var visited = new HashSet<(int, int)>();
        var parent = new Dictionary<(int, int), (int, int)>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end)
                return ReconstructPath(parent, start, end);

            foreach (var neighbor in GetNeighbors(current, grid))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null; // No path found
    }

    /// <summary>Get valid walkable neighbors (4-directional, no diagonals).</summary>
    private static IEnumerable<(int x, int y)> GetNeighbors(
        (int x, int y) cell,
        Dictionary<(int, int), Cell> grid)
    {
        var directions = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
        foreach (var (dx, dy) in directions)
        {
            var neighbor = (cell.x + dx, cell.y + dy);
            // Walkable = Path cells with no tower placed
            if (grid.TryGetValue(neighbor, out var c) &&
                (c.Type == CellType.Path) &&
                c.Tower == null)
            {
                yield return neighbor;
            }
        }
    }

    /// <summary>Reconstruct path from BFS parent map.</summary>
    private static List<(int x, int y)> ReconstructPath(
        Dictionary<(int, int), (int, int)> parent,
        (int x, int y) start,
        (int x, int y) end)
    {
        var path = new List<(int, int)>();
        var current = end;
        while (current != start)
        {
            path.Add(current);
            current = parent[current];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }
}
