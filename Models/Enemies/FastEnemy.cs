using TheAdventure.Models.Items;

namespace TheAdventure.Models.Enemies;

public sealed class FastEnemy : EnemyObject
{
    protected override int Speed => 100;

    private FastEnemy(int texId, TextureData info, int x, int y)
        : base(texId, info, x, y, size: 28, health: 1) { }

    public static FastEnemy Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "slime_orange.png"), out var info);
        return new FastEnemy(texId, info, x, y);
    }

    public override ItemObject? GetDrop(Random rng, GameRenderer renderer)
    {
        return rng.NextDouble() switch
        {
            < 0.10 => HeartItem.Create(renderer, X, Y),
            < 0.40 => SpeedBoostItem.Create(renderer, X, Y),
            < 0.55 => ExtraBombItem.Create(renderer, X, Y),
            < 0.70 => FastRechargeItem.Create(renderer, X, Y),
            _ => null
        };
    }
}
