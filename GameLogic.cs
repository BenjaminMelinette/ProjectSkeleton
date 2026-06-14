using System.Text.Json;
using Silk.NET.Maths;
using TheAdventure.Exceptions;
using TheAdventure.Models;
using TheAdventure.Models.Data;
using TheAdventure.Models.Enemies;
using TheAdventure.Models.Items;
using TheAdventure.Services;

namespace TheAdventure;

public enum GameState { Playing, GameOver, Victory }

public class GameLogic : IDisposable
{
    private readonly Dictionary<int, GameObject> _gameObjects = new();
    private readonly Dictionary<string, TileSet> _loadedTileSets = new();
    private readonly Dictionary<int, Tile> _tileIdMap = new();
    private Level _currentLevel = new();

    private PlayerObject? _player;
    private readonly GameRenderer _renderer;
    private readonly ScoreService _scoreService = new();
    private readonly Random _rng = new();

    private GameState _state = GameState.Playing;
    private int _killCount;
    private const int WinTarget = 30;

    private string? _pickupMsg;
    private double _pickupMsgTimer;

    private double _spawnCooldownMs = 2000;
    private int _worldWidth = 960;
    private int _worldHeight = 640;
    private double _lastFrameMs = 16;
    private DateTimeOffset _lastUpdate = DateTimeOffset.Now;

    private int MaxEnemiesOnScreen => _killCount switch
    {
        < 10 => 3,
        < 20 => 5,
        _ => 7
    };

    private double SpawnIntervalMs => _killCount switch
    {
        < 10 => 4000,
        < 20 => 3000,
        _ => 2000
    };

    private IEnumerable<T> GetObjects<T>() where T : GameObject =>
        _gameObjects.Values.OfType<T>();

    public GameLogic(GameRenderer renderer)
    {
        _renderer = renderer;
    }

    public async Task InitializeGame()
    {
        await _scoreService.LoadAsync();

        _player = new PlayerObject(_renderer);

        var levelContent = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));
        var level = JsonSerializer.Deserialize<Level>(levelContent)
            ?? throw new InvalidLevelException("Failed to deserialize level.");

        if (level.Width == null || level.Height == null)
            throw new InvalidLevelException("Level is missing width/height.");
        if (level.TileWidth == null || level.TileHeight == null)
            throw new InvalidLevelException("Level is missing tile dimensions.");

        foreach (var tileSetRef in level.TileSets)
        {
            var tileSetContent = File.ReadAllText(Path.Combine("Assets", tileSetRef.Source));
            var tileSet = JsonSerializer.Deserialize<TileSet>(tileSetContent)
                ?? throw new InvalidLevelException("Failed to deserialize tile set.");

            foreach (var tile in tileSet.Tiles)
            {
                tile.TextureId = _renderer.LoadTexture(Path.Combine("Assets", tile.Image), out _);
                _tileIdMap.Add(tile.Id!.Value, tile);
            }

            _loadedTileSets[tileSet.Name ?? "default"] = tileSet;
        }

        _worldWidth = level.Width.Value * level.TileWidth.Value;
        _worldHeight = level.Height.Value * level.TileHeight.Value;
        _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _worldWidth, _worldHeight));
        _currentLevel = level;

        for (int i = 0; i < MaxEnemiesOnScreen; i++)
            TrySpawnEnemy();

        UpdateWindowTitle();
    }

    public void Restart()
    {
        _gameObjects.Clear();
        _killCount = 0;
        _state = GameState.Playing;
        _spawnCooldownMs = 3000;
        _lastUpdate = DateTimeOffset.Now;
        _player?.Reset();

        for (int i = 0; i < MaxEnemiesOnScreen; i++)
            TrySpawnEnemy();

        UpdateWindowTitle();
    }

    public void PlaceBomb(int screenX, int screenY)
    {
        if (_state != GameState.Playing || _player == null) return;
        if (!_player.TryPlaceBomb()) return;

        var world = _renderer.ToWorldCoordinates(screenX, screenY);
        var bomb = BombObject.Create(_renderer, world.X, world.Y);
        _gameObjects.Add(bomb.Id, bomb);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int ms)
    {
        if (_state != GameState.Playing) return;
        _player?.UpdatePosition(up, down, left, right, ms);
    }

    public void TryDash()
    {
        if (_state != GameState.Playing) return;
        _player?.TryDash();
    }

    public void ProcessFrame()
    {
        var now = DateTimeOffset.Now;
        _lastFrameMs = (now - _lastUpdate).TotalMilliseconds;
        _lastUpdate = now;

        if (_state != GameState.Playing || _player == null) return;

        var ms = _lastFrameMs;
        _player.UpdateTimers(ms);

        var snapshot = _gameObjects.Values.ToList();
        var toRemove = new List<int>();
        var pendingExplosions = new List<(int X, int Y)>();

        foreach (var bomb in snapshot.OfType<BombObject>())
        {
            if (!bomb.Update(ms))
            {
                toRemove.Add(bomb.Id);
                if (bomb.HasExploded)
                    pendingExplosions.Add((bomb.X, bomb.Y));
            }
        }

        foreach (var anim in snapshot.OfType<AnimatedGameObject>())
        {
            if (!anim.Update(ms))
                toRemove.Add(anim.Id);
        }

        foreach (var enemy in snapshot.OfType<EnemyObject>())
            enemy.MoveToward(_player.CenterX, _player.CenterY, ms);

        if (_pickupMsgTimer > 0)
            _pickupMsgTimer -= ms;

        foreach (var (ex, ey) in pendingExplosions)
            TriggerExplosion(ex, ey);

        foreach (var enemy in snapshot.OfType<EnemyObject>().Where(e => e.IsDead))
        {
            toRemove.Add(enemy.Id);
            _killCount++;
            var drop = enemy.GetDrop(_rng, _renderer);
            if (drop != null)
                _gameObjects[drop.Id] = drop;
        }

        foreach (var id in toRemove.Distinct())
            _gameObjects.Remove(id);

        CheckPlayerEnemyCollision();
        CheckItemCollection();

        _spawnCooldownMs -= ms;
        if (_spawnCooldownMs <= 0 && GetObjects<EnemyObject>().Count() < MaxEnemiesOnScreen)
        {
            TrySpawnEnemy();
            _spawnCooldownMs = SpawnIntervalMs;
        }

        if (!_player.IsAlive)
        {
            _state = GameState.GameOver;
            _ = _scoreService.SaveAsync(_killCount);
        }
        else if (_killCount >= WinTarget)
        {
            _state = GameState.Victory;
            _ = _scoreService.SaveAsync(_killCount);
        }

        UpdateWindowTitle();
    }

    private void TriggerExplosion(int x, int y)
    {
        var anim = new AnimatedGameObject(
            Path.Combine("Assets", "BombExploding.png"),
            _renderer, 1, 13, 13, 1, x, y);
        _gameObjects[anim.Id] = anim;

        const int r = BombObject.BlastRadius;
        const int rSq = r * r;

        foreach (var enemy in GetObjects<EnemyObject>().ToList())
        {
            var dx = enemy.X - x;
            var dy = enemy.Y - y;
            if (dx * dx + dy * dy <= rSq)
                enemy.TakeDamage(1);
        }

        if (_player != null)
        {
            var dx = _player.CenterX - x;
            var dy = _player.CenterY - y;
            if (dx * dx + dy * dy <= rSq)
                _player.TakeDamage();
        }
    }

    private void CheckPlayerEnemyCollision()
    {
        if (_player == null) return;
        const int collisionRadius = 22;

        foreach (var enemy in GetObjects<EnemyObject>())
        {
            int threshold = collisionRadius + enemy.Size / 2;
            var dx = _player.CenterX - enemy.X;
            var dy = _player.CenterY - enemy.Y;
            if (dx * dx + dy * dy <= threshold * threshold)
            {
                _player.TakeDamage();
                return;
            }
        }
    }

    private static string PickupLabel(ItemObject item) => item switch
    {
        HeartItem        => "+1 Life",
        SpeedBoostItem   => "Speed Up!",
        ExtraBombItem    => "+1 Bomb",
        FastRechargeItem => "Faster Recharge!",
        _                => "Power-Up!"
    };

    private void CheckItemCollection()
    {
        if (_player == null) return;

        var picked = GetObjects<ItemObject>()
            .Where(item => item.IsInRange(_player.CenterX, _player.CenterY))
            .ToList();

        foreach (var item in picked)
        {
            _pickupMsg = PickupLabel(item);
            _pickupMsgTimer = 2000;
            item.Apply(_player);
            _gameObjects.Remove(item.Id);
        }
    }

    private void TrySpawnEnemy()
    {
        if (_player == null) return;

        const int edge = 60;
        int cw = _renderer.CameraWidth;
        int ch = _renderer.CameraHeight;

        int spawnX, spawnY;
        int side = _rng.Next(4);
        switch (side)
        {
            case 0:
                spawnX = _player.CenterX + _rng.Next(-cw / 2, cw / 2);
                spawnY = _player.CenterY - ch / 2 - edge;
                break;
            case 1:
                spawnX = _player.CenterX + _rng.Next(-cw / 2, cw / 2);
                spawnY = _player.CenterY + ch / 2 + edge;
                break;
            case 2:
                spawnX = _player.CenterX - cw / 2 - edge;
                spawnY = _player.CenterY + _rng.Next(-ch / 2, ch / 2);
                break;
            default:
                spawnX = _player.CenterX + cw / 2 + edge;
                spawnY = _player.CenterY + _rng.Next(-ch / 2, ch / 2);
                break;
        }

        spawnX = Math.Clamp(spawnX, 16, _worldWidth - 16);
        spawnY = Math.Clamp(spawnY, 16, _worldHeight - 16);

        EnemyObject enemy = (_killCount, _rng.Next(5)) switch
        {
            (< 10, _)  => BasicEnemy.Create(_renderer, spawnX, spawnY),
            (< 20, 0)  => FastEnemy.Create(_renderer, spawnX, spawnY),
            (< 20, _)  => BasicEnemy.Create(_renderer, spawnX, spawnY),
            (_, 0)     => TankEnemy.Create(_renderer, spawnX, spawnY),
            (_, 1)     => FastEnemy.Create(_renderer, spawnX, spawnY),
            _          => BasicEnemy.Create(_renderer, spawnX, spawnY)
        };

        _gameObjects[enemy.Id] = enemy;
    }

    public void RenderFrame()
    {
        _renderer.SetDrawColor(0, 0, 0, 255);
        _renderer.ClearScreen();

        if (_player != null)
            _renderer.CameraLookAt(_player.CenterX, _player.CenterY);

        RenderTerrain();
        RenderGameObjects();
        _player?.Render(_renderer);
        RenderHud();

        _renderer.PresentFrame();
    }

    private void RenderGameObjects()
    {
        foreach (var item in GetObjects<ItemObject>())
            item.Render(_renderer);

        foreach (var enemy in GetObjects<EnemyObject>())
            enemy.Render(_renderer);

        foreach (var bomb in GetObjects<BombObject>())
            bomb.Render(_renderer);

        foreach (var anim in GetObjects<AnimatedGameObject>())
            anim.Render(_renderer);
    }

    private void RenderHud()
    {
        if (_player == null) return;

        for (int i = 0; i < PlayerObject.MaxLives; i++)
        {
            if (i < _player.Lives)
                _renderer.SetDrawColor(210, 40, 40, 255);
            else
                _renderer.SetDrawColor(70, 15, 15, 180);
            _renderer.FillRect(10 + i * 22, 10, 18, 18);
        }

        for (int i = 0; i < _player.MaxBombs; i++)
        {
            if (i < _player.CurrentBombs)
                _renderer.SetDrawColor(35, 35, 35, 255);
            else
                _renderer.SetDrawColor(110, 110, 110, 120);
            _renderer.FillRect(10 + i * 22, 34, 18, 18);
        }

        {
            _renderer.SetDrawColor(20, 20, 50, 200);
            _renderer.FillRect(10, 58, 18, 18);
            if (_player.CanDash)
            {
                _renderer.SetDrawColor(40, 210, 200, 255);
                _renderer.FillRect(10, 58, 18, 18);
            }
            else
            {
                int filled = (int)((1.0 - _player.DashCooldownFraction) * 18);
                if (filled > 0)
                {
                    _renderer.SetDrawColor(40, 210, 200, 180);
                    _renderer.FillRect(10, 58 + (18 - filled), 18, filled);
                }
            }
        }

        if (_state == GameState.GameOver)
        {
            _renderer.SetDrawColor(180, 0, 0, 130);
            _renderer.FillRect(0, 0, _renderer.CameraWidth, _renderer.CameraHeight);
        }
        else if (_state == GameState.Victory)
        {
            _renderer.SetDrawColor(0, 160, 0, 130);
            _renderer.FillRect(0, 0, _renderer.CameraWidth, _renderer.CameraHeight);
        }
    }

    private void UpdateWindowTitle()
    {
        var pickup = (_pickupMsgTimer > 0 && _pickupMsg != null) ? $"  ✦ {_pickupMsg}" : "";
        var title = _state switch
        {
            GameState.Playing =>
                $"TheAdventure  |  Kills: {_killCount}/{WinTarget}  |  Best: {_scoreService.HighScore}{pickup}",
            GameState.GameOver =>
                $"GAME OVER  |  Score: {_killCount}/{WinTarget}  |  Best: {_scoreService.HighScore}  |  R = restart",
            GameState.Victory =>
                $"YOU WIN!  |  Score: {_killCount}  |  Best: {_scoreService.HighScore}  |  R = restart",
            _ => "TheAdventure"
        };
        _renderer.SetWindowTitle(title);
    }

    private void RenderTerrain()
    {
        foreach (var layer in _currentLevel.Layers)
        {
            for (int i = 0; i < _currentLevel.Width; ++i)
            {
                for (int j = 0; j < _currentLevel.Height; ++j)
                {
                    int? dataIndex = j * layer.Width + i;
                    if (dataIndex == null) continue;
                    var tileId = layer.Data[dataIndex.Value] - 1;
                    if (tileId < 0) continue;
                    if (!_tileIdMap.TryGetValue(tileId, out var tile)) continue;

                    var tw = tile.ImageWidth ?? 0;
                    var th = tile.ImageHeight ?? 0;
                    _renderer.RenderTexture(tile.TextureId,
                        new Rectangle<int>(0, 0, tw, th),
                        new Rectangle<int>(i * tw, j * th, tw, th));
                }
            }
        }
    }

    public void Dispose()
    {
        _scoreService.Dispose();
    }
}
