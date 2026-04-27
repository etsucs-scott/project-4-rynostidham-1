namespace TowerDefense.Core.Models;

/// <summary>Abstract base class for all enemy types. Each subclass defines stats and behavior.</summary>
public abstract class Enemy
{
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary>Grid X position (float for smooth movement between cells).</summary>
    public float X { get; set; }
    public float Y { get; set; }
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; private set; }
    public abstract string Name { get; }
    public abstract float Speed { get; }
    /// <summary>Gold rewarded to player on kill.</summary>
    public abstract int GoldReward { get; }
    /// <summary>How much damage this enemy deals to the player's base on reaching the end.</summary>
    public abstract int BaseDamage { get; }
    public bool IsDead => CurrentHealth <= 0;
    /// <summary>Index of the next waypoint this enemy is moving toward.</summary>
    public int PathIndex { get; set; } = 0;

    protected Enemy(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    /// <summary>Apply damage to this enemy. Returns actual damage dealt.</summary>
    public int TakeDamage(int amount)
    {
        int dealt = Math.Min(amount, CurrentHealth);
        CurrentHealth -= dealt;
        return dealt;
    }

    public float HealthPercent => (float)CurrentHealth / MaxHealth;
}

/// <summary>Fast but weak enemy.</summary>
public class Goblin : Enemy
{
    public override string Name => "Goblin";
    public override float Speed => 2.5f;
    public override int GoldReward => 10;
    public override int BaseDamage => 1;
    public Goblin() : base(40) { X = 0; Y = 0; }
}

/// <summary>Slow, tanky enemy with high health.</summary>
public class Troll : Enemy
{
    public override string Name => "Troll";
    public override float Speed => 1.0f;
    public override int GoldReward => 25;
    public override int BaseDamage => 3;
    public Troll() : base(150) { X = 0; Y = 0; }
}

/// <summary>Boss enemy — very high health, deals massive base damage.</summary>
public class BossOrc : Enemy
{
    public override string Name => "Boss Orc";
    public override float Speed => 0.8f;
    public override int GoldReward => 100;
    public override int BaseDamage => 10;
    public BossOrc() : base(500) { X = 0; Y = 0; }
}
