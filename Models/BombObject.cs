using Silk.NET.Maths;

namespace TheAdventure.Models;

public sealed class BombObject : RenderableGameObject
{
    public const int BlastRadius = 96;
    private const double CountdownMs = 2500.0;

    public int X { get; }
    public int Y { get; }
    public bool HasExploded { get; private set; }

    private double _remainingMs = CountdownMs;

    private BombObject(int texId, TextureData info, int x, int y) : base(texId, info)
    {
        X = x;
        Y = y;
        TextureDestination = new Rectangle<int>(x - info.Width / 2, y - info.Height / 2, info.Width, info.Height);
        TextureSource = new Rectangle<int>(0, 0, info.Width, info.Height);
    }

    public static BombObject Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "bomb_sprite.png"), out var info);
        return new BombObject(texId, info, x, y);
    }

    public override bool Update(double msSinceLastFrame)
    {
        _remainingMs -= msSinceLastFrame;
        if (_remainingMs <= 0)
        {
            HasExploded = true;
            return false;
        }
        return true;
    }
}
