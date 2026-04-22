namespace FinalProject.Core;

public readonly struct GridPosition(int row, int col)
{
    public int Row { get; } = row;
    public int Col { get; } = col;

    public override string ToString() => $"({Row},{Col})";
}
