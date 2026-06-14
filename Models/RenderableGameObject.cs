using Silk.NET.Maths;

namespace TheAdventure.Models;

public class RenderableGameObject : GameObject
{
    public int TextureId { get; protected set; }
    public Rectangle<int> TextureSource { get; set; }
    public Rectangle<int> TextureDestination { get; set; }
    public TextureData TextureInformation { get; protected set; }

    public RenderableGameObject(string fileName, GameRenderer renderer)
    {
        TextureId = renderer.LoadTexture(fileName, out var textureData);
        TextureInformation = textureData;
        TextureSource = new Rectangle<int>(0, 0, textureData.Width, textureData.Height);
        TextureDestination = new Rectangle<int>(0, 0, textureData.Width, textureData.Height);
    }

    protected RenderableGameObject(int textureId, TextureData info)
    {
        TextureId = textureId;
        TextureInformation = info;
        TextureSource = new Rectangle<int>(0, 0, info.Width, info.Height);
        TextureDestination = new Rectangle<int>(0, 0, info.Width, info.Height);
    }

    public virtual void Render(GameRenderer renderer)
    {
        renderer.RenderTexture(TextureId, TextureSource, TextureDestination);
    }

    public virtual bool Update(double msSinceLastFrame)
    {
        return true;
    }
}
