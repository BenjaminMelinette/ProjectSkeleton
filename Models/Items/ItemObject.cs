using Silk.NET.Maths;
using TheAdventure.Interfaces;

namespace TheAdventure.Models.Items;

public abstract class ItemObject : RenderableGameObject, IPickupable
{
    public int X { get; }
    public int Y { get; }
    private const int CollectionRadius = 24;

    protected ItemObject(int texId, TextureData info, int x, int y) : base(texId, info)
    {
        X = x;
        Y = y;
        TextureDestination = new Rectangle<int>(x - info.Width / 2, y - info.Height / 2, info.Width, info.Height);
        TextureSource = new Rectangle<int>(0, 0, info.Width, info.Height);
    }

    public abstract void Apply(PlayerObject player);

    public bool IsInRange(int playerX, int playerY)
    {
        var dx = playerX - X;
        var dy = playerY - Y;
        return dx * dx + dy * dy <= CollectionRadius * CollectionRadius;
    }

    public override bool Update(double msSinceLastFrame) => true;
}
