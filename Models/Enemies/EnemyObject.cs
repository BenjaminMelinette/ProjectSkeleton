using Silk.NET.Maths;
using TheAdventure.Interfaces;
using TheAdventure.Models.Items;

namespace TheAdventure.Models.Enemies;

public abstract class EnemyObject : RenderableGameObject, IDamageable
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Health { get; private set; }
    public bool IsDead => Health <= 0;

    protected abstract int Speed { get; }
    public int Size => _size;
    private readonly int _size;

    private double _fracX;
    private double _fracY;

    protected EnemyObject(int texId, TextureData info, int x, int y, int size, int health)
        : base(texId, info)
    {
        X = x;
        Y = y;
        _size = size;
        Health = health;
        RefreshDestination();
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
    }

    public abstract ItemObject? GetDrop(Random rng, GameRenderer renderer);

    public void MoveToward(int targetX, int targetY, double ms)
    {
        var dx = targetX - X;
        var dy = targetY - Y;
        var distSq = dx * dx + dy * dy;
        if (distSq < 4) return;

        var dist = Math.Sqrt(distSq);
        var step = Speed * (ms / 1000.0);

        _fracX += dx / dist * step;
        _fracY += dy / dist * step;

        var moveX = (int)_fracX;
        var moveY = (int)_fracY;
        _fracX -= moveX;
        _fracY -= moveY;

        X += moveX;
        Y += moveY;
        RefreshDestination();
    }

    private void RefreshDestination()
    {
        TextureDestination = new Rectangle<int>(X - _size / 2, Y - _size / 2, _size, _size);
    }

    public override bool Update(double msSinceLastFrame) => true;
}
