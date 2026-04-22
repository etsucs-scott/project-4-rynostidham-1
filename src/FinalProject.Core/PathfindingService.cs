namespace FinalProject.Core;

public class PathfindingService
{
    public List<GridPosition> FindPath(GameMap map, GridPosition start, GridPosition goal)
    {
        var queue = new Queue<GridPosition>();
        var visited = new HashSet<GridPosition>();
        var cameFrom = new Dictionary<GridPosition, GridPosition>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.Equals(goal))
                break;

            foreach (var neighbor in map.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && map.IsWalkable(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return new List<GridPosition>();

        var path = new List<GridPosition>();
        var node = goal;

        while (!node.Equals(start))
        {
            path.Add(node);
            node = cameFrom[node];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
}
