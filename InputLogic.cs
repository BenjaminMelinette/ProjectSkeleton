using Silk.NET.SDL;

namespace TheAdventure;

public class InputLogic
{
    private readonly Sdl _sdl;
    private readonly GameLogic _gameLogic;

    private int _mouseX;
    private int _mouseY;
    private DateTimeOffset _lastUpdate = DateTimeOffset.Now;

    public InputLogic(Sdl sdl, GameLogic gameLogic)
    {
        _sdl = sdl;
        _gameLogic = gameLogic;
    }

    public unsafe bool ProcessInput()
    {
        var currentTime = DateTimeOffset.Now;

        ReadOnlySpan<byte> keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
        Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];

        var ev = new Event();
        while (_sdl.PollEvent(ref ev) != 0)
        {
            if (ev.Type == (uint)EventType.Quit) return true;

            switch (ev.Type)
            {
                case (uint)EventType.Mousebuttondown:
                    _mouseX = ev.Motion.X;
                    _mouseY = ev.Motion.Y;
                    mouseButtonStates[ev.Button.Button] = 1;
                    break;

                case (uint)EventType.Mousebuttonup:
                    mouseButtonStates[ev.Button.Button] = 0;
                    break;

                case (uint)EventType.Fingerdown:
                    mouseButtonStates[(byte)MouseButton.Primary] = 1;
                    break;

                case (uint)EventType.Fingerup:
                    mouseButtonStates[(byte)MouseButton.Primary] = 0;
                    break;

                case (uint)EventType.Windowevent:
                    if (ev.Window.Event == (byte)WindowEventID.TakeFocus)
                        _sdl.SetWindowInputFocus(_sdl.GetWindowFromID(ev.Window.WindowID));
                    break;
            }
        }

        var timeSinceLastFrame = (int)currentTime.Subtract(_lastUpdate).TotalMilliseconds;
        _lastUpdate = currentTime;

        if (keyboardState[(int)KeyCode.Escape] == 1) return true;

        if (keyboardState[(int)KeyCode.R] == 1)
        {
            _gameLogic.Restart();
            return false;
        }

        if (keyboardState[(int)KeyCode.Space] == 1)
            _gameLogic.TryDash();

        var up = 0.0;
        var down = 0.0;
        var left = 0.0;
        var right = 0.0;

        if (keyboardState[(int)KeyCode.Up] == 1) up = 1.0;
        if (keyboardState[(int)KeyCode.Down] == 1) down = 1.0;
        if (keyboardState[(int)KeyCode.Left] == 1) left = 1.0;
        if (keyboardState[(int)KeyCode.Right] == 1) right = 1.0;

        _gameLogic.UpdatePlayerPosition(up, down, left, right, timeSinceLastFrame);

        if (mouseButtonStates[(byte)MouseButton.Primary] == 1)
            _gameLogic.PlaceBomb(_mouseX, _mouseY);

        return false;
    }
}
