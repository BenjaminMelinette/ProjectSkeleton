# TheAdventure

A top-down arena survivor built on the [dotNETUVT/ProjectSkeleton](https://github.com/dotNETUVT/ProjectSkeleton) starter, written in C# / .NET 10 using Silk.NET + SDL2.

---

## Gameplay

You are a lone knight dropped into a grassy arena. Enemies spawn at the edges of the screen and home in on you. Survive long enough to kill **30 enemies** and win. Lose all your lives and it's game over.

### Controls

| Key / Button | Action |
|---|---|
| Arrow keys | Move |
| Space | Dash |
| Left click | Place a **bomb** at the cursor position |
| R | Restart after game over / victory |
| Escape | Quit |

### Enemy roster

| Enemy | Sprite | Speed | HP | Notes |
|---|---|---|---|---|
| Basic Slime | red slime | slow | 1 | Spawns from the start |
| Fast Slime | orange slime | fast | 1 | Appears after 10 kills |
| Tank Knight | knight | very slow | 3 | Appears after 20 kills |

Difficulty scales as your kill count rises: more enemies on screen, shorter spawn intervals, and tougher enemy mixes.

### Power-ups

Enemies have a chance to drop one of these on death:

| Icon | Power-up | Effect |
|---|---|---|
| ❤️ | Heart | +1 life (max 5) |
| ⚡ | Speed Boost | +32 movement speed (max 256) |
| 💣 | Extra Bomb | +1 maximum bomb slot |
| ⏳ | Fast Recharge | −200 ms bomb recharge time (min 400 ms) |

### HUD

First line represents lives (read = alive, dark = lost).
Second line represents the bombs available (dark = on cooldown).
Third line is the the dash.

Window title always shows current kills, win target, and all-time best score.

---

## Win / Lose conditions

- **Win** - reach 30 kills. The screen flashes green. Press **R** to play again.
- **Lose** - all lives are gone. The screen flashes red. Press **R** to restart.

The best score from all runs is saved to `highscore.json` beside the executable and reloaded at startup.

---

## Build & run

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (LTS)
- SDL2 is bundled as a NuGet runtime asset.

### Steps

```bash
dotnet run
```

That's it. Works from a clean clone on Windows, Linux, and macOS.

---

## Project structure

```
TheAdventure/
├── Assets/             # Sprites and tile-map files
├── Exceptions/         # GameException, InvalidLevelException
├── Interfaces/         # IDamageable, IPickupable
├── Models/
│   ├── Data/           # Level, Layer, Tile, TileSet (JSON models)
│   ├── Enemies/        # EnemyObject (abstract) + Basic/Fast/TankEnemy
│   ├── Items/          # ItemObject (abstract) + Heart/Speed/Bomb/Recharge items
│   ├── AnimatedGameObject.cs
│   ├── BombObject.cs
│   ├── GameObject.cs
│   ├── PlayerObject.cs
│   ├── RenderableGameObject.cs
│   └── TextureData.cs
├── Services/
│   └── ScoreService.cs # async high-score persistence (JSON)
├── GameCamera.cs
├── GameLogic.cs        # Core game loop & state machine
├── GameRenderer.cs
├── GameWindow.cs
├── InputLogic.cs
├── Program.cs
├── AI_USAGE.md
└── README.md
```

---

## C# / .NET features demonstrated

| Feature | Where |
|---|---|
| **LINQ** | `GetObjects<T>()`, `.OfType<>()`, `.Where()`, `.ToList()`, `.Distinct()`, `.Count()` in `GameLogic` |
| **Generics** | `Dictionary<int, GameObject>`, `GetObjects<T>() where T : GameObject` |
| **Interfaces** | `IDamageable` (enemies & player), `IPickupable` (items), `IDisposable` (ScoreService, GameLogic) |
| **Inheritance** | `EnemyObject` → `BasicEnemy` / `FastEnemy` / `TankEnemy` · `ItemObject` → four item types |
| **async/await** | `ScoreService.LoadAsync` / `SaveAsync`, `Program.Main` |
| **Pattern matching** | `switch` expressions for difficulty scaling, enemy type selection, and pickup labels |
| **IDisposable** | `ScoreService`, `GameLogic` |
| **Custom exceptions** | `GameException`, `InvalidLevelException` |
| **Records** | `ScoreData` record in `ScoreService` |

---

## AI usage summary

See [AI_USAGE.md](AI_USAGE.md) for the full disclosure. In short:

I used claude in order to generate the different assets used in the project.
