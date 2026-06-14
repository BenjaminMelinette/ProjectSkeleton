using Silk.NET.SDL;

namespace TheAdventure;

public class GameWindow
{
    private readonly Sdl _sdl;
    private readonly IntPtr _window;

    public unsafe GameWindow(Sdl sdl)
    {
        _sdl = sdl;
        _window = (IntPtr)sdl.CreateWindow(
            "The Adventure", Sdl.WindowposUndefined, Sdl.WindowposUndefined, 640, 400,
            (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
        );

        if (_window == IntPtr.Zero)
        {
            var ex = sdl.GetErrorAsException();
            if (ex != null) throw ex;
            throw new Exception("Failed to create window.");
        }
    }

    public unsafe (int Width, int Height) Size
    {
        get
        {
            int width = 0, height = 0;
            _sdl.GetWindowSize((Window*)_window, ref width, ref height);
            return (width, height);
        }
    }

    public unsafe IntPtr CreateRenderer()
    {
        var renderer = (IntPtr)_sdl.CreateRenderer((Window*)_window, -1, (uint)RendererFlags.Accelerated);
        _sdl.RenderSetVSync((Renderer*)renderer, 1);
        return renderer;
    }

    public unsafe void SetTitle(string title)
    {
        _sdl.SetWindowTitle((Window*)_window, title);
    }

    public unsafe void Destroy()
    {
        _sdl.DestroyWindow((Window*)_window);
    }
}
