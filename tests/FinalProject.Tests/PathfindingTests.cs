using Xunit;
using FinalProject.Core;

public class PathfindingTests
{
    [Fact]
    public void Pathfinding_FindsPath()
    {
        var map = new GameMap(5, 5);
        var service = new PathfindingService();

        var path = service.FindPath(map, new GridPosition(0,0), new GridPosition(4,4));

        Assert.NotEmpty(path);
        Assert.Equal(new GridPosition(0,0), path.First());
        Assert.Equal(new GridPosition(4,4), path.Last());
    }
}
