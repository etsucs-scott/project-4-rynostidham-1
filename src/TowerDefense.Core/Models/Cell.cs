namespace TowerDefense.Core.Models;

/// <summary>Represents one tile on the game grid.</summary>
public enum CellType { Path, Buildable, Blocked }

/// <summary>A single cell on the game map grid.</summary>
public class Cell
{
    public int X { get; set; }
    public int Y { get; set; }
    public CellType Type { get; set; }
    /// <summary>Tower placed on this cell, if any.</summary>
    public Tower? Tower { get; set; }

    public Cell(int x, int y, CellType type)
    {
        X = x; Y = y; Type = type;
    }
}
