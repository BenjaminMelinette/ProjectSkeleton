using TheAdventure.Models.Items;

namespace TheAdventure.Models.Enemies;

public sealed class BasicEnemy : EnemyObject
{
    protected override int Speed => 55;

    private BasicEnemy(int texId, TextureData info, int x, int y)
        : base(texId, info, x, y, size: 32, health: 1) { }

    public static BasicEnemy Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "slime_red.png"), out var info);
        return new BasicEnemy(texId, info, x, y);
    }

    public override ItemObject? GetDrop(Random rng, GameRenderer renderer)
    {
        return rng.NextDouble() switch
        {
            < 0.15 => HeartItem.Create(renderer, X, Y),
            < 0.35 => SpeedBoostItem.Create(renderer, X, Y),
            < 0.55 => ExtraBombItem.Create(renderer, X, Y),
            < 0.70 => FastRechargeItem.Create(renderer, X, Y),
            _ => null
        };
    }
}
