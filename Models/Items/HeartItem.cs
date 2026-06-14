namespace TheAdventure.Models.Items;

public sealed class HeartItem : ItemObject
{
    private HeartItem(int texId, TextureData info, int x, int y) : base(texId, info, x, y) { }

    public static HeartItem Create(GameRenderer renderer, int x, int y)
    {
        var texId = renderer.LoadTexture(Path.Combine("Assets", "heart.png"), out var info);
        return new HeartItem(texId, info, x, y);
    }

    public override void Apply(PlayerObject player) => player.AddLife();
}
