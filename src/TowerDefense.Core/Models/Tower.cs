namespace TowerDefense.Core.Models;

/// <summary>Abstract base class for all tower types. Subclasses define attack behavior.</summary>
public abstract class Tower
{
    public Guid Id { get; } = Guid.NewGuid();
    public int X { get; set; }
    public int Y { get; set; }
    public int Level { get; private set; } = 1;
    public int Cost { get; protected set; }
    public abstract string Name { get; }
    public abstract float Range { get; }
    public abstract int Damage { get; }
    /// <summary>Seconds between attacks.</summary>
    public abstract float AttackCooldown { get; }
    public float CooldownRemaining { get; set; } = 0f;

    /// <summary>Returns true if this tower can target the given enemy (within range).</summary>
    public bool InRange(Enemy enemy)
    {
        float dx = enemy.X - X;
        float dy = enemy.Y - Y;
        return MathF.Sqrt(dx * dx + dy * dy) <= Range;
    }

    /// <summary>Upgrade the tower, increasing its level and stats.</summary>
    public virtual void Upgrade() => Level++;

    /// <summary>Attack the target enemy and return a projectile aimed at it.</summary>
    public Projectile? TryAttack(Enemy target, float deltaTime)
    {
        CooldownRemaining -= deltaTime;
        if (CooldownRemaining > 0f) return null;
        CooldownRemaining = AttackCooldown;
        return new Projectile(X, Y, target, Damage, ProjectileSpeed);
    }

    protected virtual float ProjectileSpeed => 5f;
}

/// <summary>Basic tower — low cost, moderate range and damage.</summary>
public class ArrowTower : Tower
{
    public override string Name => "Arrow Tower";
    public override float Range => 3f;
    public override int Damage => 10 * Level;
    public override float AttackCooldown => 1.0f;
    public ArrowTower() { Cost = 50; }
}

/// <summary>Mage tower — high damage, slow attack, magical projectiles.</summary>
public class MageTower : Tower
{
    public override string Name => "Mage Tower";
    public override float Range => 4f;
    public override int Damage => 25 * Level;
    public override float AttackCooldown => 2.5f;
    protected override float ProjectileSpeed => 4f;
    public MageTower() { Cost = 100; }
}

/// <summary>Cannon tower — area splash damage, very slow fire rate.</summary>
public class CannonTower : Tower
{
    public override string Name => "Cannon Tower";
    public override float Range => 2.5f;
    public override int Damage => 40 * Level;
    public override float AttackCooldown => 4.0f;
    protected override float ProjectileSpeed => 3f;
    public CannonTower() { Cost = 150; }
}
