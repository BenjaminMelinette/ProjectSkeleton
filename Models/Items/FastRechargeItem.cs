

namespace TheAdventure.Models.Items;

public sealed class FastRechargeItem : ItemObject
{
    private FastRechargeItem(int texId, TextureData info, int x, int y) : base(texId, info, x, y) { }

    public static FastRechargeItem Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "hourglass.png"), out var info);
        return new FastRechargeItem(texId, info, x, y);
    }

    public override void Apply(PlayerObject player) => player.ReduceRecharge(150.0);
}
