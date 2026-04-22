namespace FinalProject.Core;

public class Tower
{
    public int Id { get; set; }
    public string Name { get; set; } = "Basic Tower";
    public int Damage { get; set; } = 10;
    public int Range { get; set; } = 2;
    public double FireRate { get; set; } = 1.0;
    public int Cost { get; set; } = 50;
    public GridPosition Position { get; set; }
}
