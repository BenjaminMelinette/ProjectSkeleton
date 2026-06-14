using Silk.NET.SDL;
using TheAdventure.Exceptions;

namespace TheAdventure;

public static class Program
{
    public static async Task Main()
    {
        var sdl = new Sdl(new SdlContext());

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer |
                                     Sdl.InitGamecontroller | Sdl.InitJoystick);
        if (sdlInitResult < 0)
            throw new GameException("Failed to initialize SDL.");

        var gameWindow = new GameWindow(sdl);
        var gameRenderer = new GameRenderer(sdl, gameWindow);
        using var gameLogic = new GameLogic(gameRenderer);
        var inputLogic = new InputLogic(sdl, gameLogic);

        await gameLogic.InitializeGame();

        bool quit = false;
        while (!quit)
        {
            quit = inputLogic.ProcessInput();
            if (quit) break;

            gameLogic.ProcessFrame();
            gameLogic.RenderFrame();

            System.Threading.Thread.Sleep(13);
        }

        gameWindow.Destroy();
        sdl.Quit();
    }
}
