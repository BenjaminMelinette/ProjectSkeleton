using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Exceptions;
using TheAdventure.Models;

namespace TheAdventure;

public unsafe class GameRenderer
{
    private readonly Sdl _sdl;
    private readonly Renderer* _renderer;
    private readonly GameWindow _window;
    private readonly GameCamera _camera;

    private readonly Dictionary<int, IntPtr> _texturePointers = new();
    private readonly Dictionary<int, TextureData> _textureData = new();
    private readonly Dictionary<string, int> _fileTextureCache = new();
    private int _textureId;

    public int CameraWidth => _camera.Width;
    public int CameraHeight => _camera.Height;

    public GameRenderer(Sdl sdl, GameWindow window)
    {
        _sdl = sdl;
        _window = window;
        _renderer = (Renderer*)window.CreateRenderer();
        _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);

        var windowSize = window.Size;
        _camera = new GameCamera(windowSize.Width, windowSize.Height);
    }

    public int LoadTexture(string fileName, out TextureData textureInfo)
    {
        if (_fileTextureCache.TryGetValue(fileName, out var cached))
        {
            textureInfo = _textureData[cached];
            return cached;
        }

        using var fStream = new FileStream(fileName, FileMode.Open);
        var image = Image.Load<Rgba32>(fStream);
        textureInfo = new TextureData { Width = image.Width, Height = image.Height };
        var rawData = new byte[textureInfo.Width * textureInfo.Height * 4];
        image.CopyPixelDataTo(rawData.AsSpan());
        fixed (byte* data = rawData)
        {
            var surface = _sdl.CreateRGBSurfaceWithFormatFrom(data,
                textureInfo.Width, textureInfo.Height,
                8, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
            if (surface == null)
                throw new GameException("Failed to create surface from image data.");

            var texture = _sdl.CreateTextureFromSurface(_renderer, surface);
            if (texture == null)
            {
                _sdl.FreeSurface(surface);
                throw new GameException("Failed to create texture from surface.");
            }

            _sdl.FreeSurface(surface);
            _textureData[_textureId] = textureInfo;
            _texturePointers[_textureId] = (IntPtr)texture;
        }
        _fileTextureCache[fileName] = _textureId;
        return _textureId++;
    }

    public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst,
        RendererFlip flip = RendererFlip.None, double angle = 0.0, Silk.NET.SDL.Point center = default)
    {
        if (_texturePointers.TryGetValue(textureId, out var imageTexture))
        {
            var translatedDst = _camera.ToScreenCoordinates(dst);
            _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture,
                in src, in translatedDst, angle, in center, flip);
        }
    }

    public void FillRect(int x, int y, int w, int h)
    {
        var rect = new Rectangle<int>(x, y, w, h);
        _sdl.RenderFillRect(_renderer, in rect);
    }

    public void SetWindowTitle(string title) => _window.SetTitle(title);

    public void SetDrawColor(byte r, byte g, byte b, byte a) => _sdl.SetRenderDrawColor(_renderer, r, g, b, a);
    public void ClearScreen() => _sdl.RenderClear(_renderer);
    public void PresentFrame() => _sdl.RenderPresent(_renderer);
    public void SetWorldBounds(Rectangle<int> bounds) => _camera.SetWorldBounds(bounds);
    public void CameraLookAt(int x, int y) => _camera.LookAt(x, y);
    public Vector2D<int> ToWorldCoordinates(int x, int y) => _camera.ToWorldCoordinates(new Vector2D<int>(x, y));
}
