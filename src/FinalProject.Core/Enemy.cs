namespace FinalProject.Core;

public class Enemy
{
    public int Id { get; set; }
    public int Health { get; set; } = 50;
    public double Speed { get; set; } = 1.0;
    public int Reward { get; set; } = 10;

    public int PathIndex { get; set; } = 0;
    public bool IsAlive => Health > 0;
}
