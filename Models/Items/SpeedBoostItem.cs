

namespace TheAdventure.Models.Items;

public sealed class SpeedBoostItem : ItemObject
{
    private SpeedBoostItem(int texId, TextureData info, int x, int y) : base(texId, info, x, y) { }

    public static SpeedBoostItem Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "lightning.png"), out var info);
        return new SpeedBoostItem(texId, info, x, y);
    }

    public override void Apply(PlayerObject player) => player.AddSpeed(32);
}
