

namespace TheAdventure.Models.Items;

public sealed class ExtraBombItem : ItemObject
{
    private ExtraBombItem(int texId, TextureData info, int x, int y) : base(texId, info, x, y) { }

    public static ExtraBombItem Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "bombicon.png"), out var info);
        return new ExtraBombItem(texId, info, x, y);
    }

    public override void Apply(PlayerObject player) => player.AddMaxBomb();
}
