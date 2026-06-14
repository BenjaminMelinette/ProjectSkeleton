using TheAdventure.Models.Items;

namespace TheAdventure.Models.Enemies;

public sealed class TankEnemy : EnemyObject
{
    protected override int Speed => 38;

    private TankEnemy(int texId, TextureData info, int x, int y)
        : base(texId, info, x, y, size: 40, health: 3) { }

    public static TankEnemy Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "knight.png"), out var info);
        return new TankEnemy(texId, info, x, y);
    }

    public override ItemObject? GetDrop(Random rng, GameRenderer renderer)
    {
        return rng.NextDouble() switch
        {
            < 0.20 => HeartItem.Create(renderer, X, Y),
            < 0.30 => SpeedBoostItem.Create(renderer, X, Y),
            < 0.60 => ExtraBombItem.Create(renderer, X, Y),
            < 0.85 => FastRechargeItem.Create(renderer, X, Y),
            _ => null
        };
    }
}
