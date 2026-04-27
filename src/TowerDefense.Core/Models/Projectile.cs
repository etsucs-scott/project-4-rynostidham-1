namespace TowerDefense.Core.Models;

/// <summary>A projectile fired by a tower at a target enemy.</summary>
public class Projectile
{
    public Guid Id { get; } = Guid.NewGuid();
    public float X { get; set; }
    public float Y { get; set; }
    public Enemy Target { get; }
    public int Damage { get; }
    public float Speed { get; }
    public bool HasHit { get; private set; } = false;

    public Projectile(float startX, float startY, Enemy target, int damage, float speed)
    {
        X = startX; Y = startY;
        Target = target; Damage = damage; Speed = speed;
    }

    /// <summary>
    /// Move the projectile toward its target. Returns true if it reached and hit the target.
    /// </summary>
    public bool Move(float deltaTime)
    {
        if (HasHit || Target.IsDead) { HasHit = true; return false; }

        float dx = Target.X - X;
        float dy = Target.Y - Y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);

        // Check if close enough to hit this frame
        if (dist <= Speed * deltaTime)
        {
            Target.TakeDamage(Damage);
            HasHit = true;
            return true;
        }

        // Move toward target
        X += (dx / dist) * Speed * deltaTime;
        Y += (dy / dist) * Speed * deltaTime;
        return false;
    }
}
