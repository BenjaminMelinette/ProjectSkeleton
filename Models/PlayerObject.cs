using Silk.NET.Maths;
using TheAdventure.Exceptions;

namespace TheAdventure.Models;

public class PlayerObject : GameObject
{
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;

    public int Lives { get; private set; } = 3;
    public const int MaxLives = 5;
    public int MaxBombs { get; private set; } = 3;
    public int CurrentBombs { get; private set; } = 3;
    public double BombRechargeMs { get; private set; } = 1000.0;
    private const double MinBombRechargeMs = 400.0;
    public int Speed { get; private set; } = 128;
    private const int MaxSpeed = 256;

    public int CenterX => X + 48;
    public int CenterY => Y - 18;

    public bool IsAlive => Lives > 0;
    public bool IsInvincible => _invincibilityMs > 0;

    private double _invincibilityMs;
    private const double InvincibilityDuration = 2000.0;
    private readonly List<double> _bombTimers = new();

    private double _dashMs;
    private double _dashCooldownMs;
    private double _lastDX = 1;
    private double _lastDY;
    private const double DashDuration  = 220.0;
    private const double DashCooldown  = 1500.0;
    private const int    DashSpeed     = 480;

    public bool IsDashing  => _dashMs > 0;
    public bool CanDash    => _dashCooldownMs <= 0;
    public double DashCooldownFraction => Math.Clamp(_dashCooldownMs / DashCooldown, 0, 1);

    private readonly Rectangle<int> _source = new(0, 0, 48, 48);
    private Rectangle<int> _target;
    private readonly int _textureId;

    private double _blinkTimer;
    private bool _blinkVisible = true;

    public PlayerObject(GameRenderer renderer)
    {
        _textureId = renderer.LoadTexture(Path.Combine("Assets", "player.png"), out _);
        if (_textureId < 0)
            throw new GameException("Failed to load player texture.");
        _target = new Rectangle<int>(X + 24, Y - 42, 48, 48);
    }

    public bool TryDash()
    {
        if (_dashCooldownMs > 0) return false;
        _dashMs = DashDuration;
        _dashCooldownMs = DashCooldown;
        return true;
    }

    public void UpdatePosition(double up, double down, double left, double right, int time)
    {
        double dx = right - left, dy = down - up;
        if (dx != 0 || dy != 0)
        {
            var len = Math.Sqrt(dx * dx + dy * dy);
            _lastDX = dx / len;
            _lastDY = dy / len;
        }

        double pixels;
        if (IsDashing)
        {
            pixels = DashSpeed * (time / 1000.0);
            X += (int)(pixels * _lastDX);
            Y += (int)(pixels * _lastDY);
        }
        else
        {
            pixels = Speed * (time / 1000.0);
            Y -= (int)(pixels * up);
            Y += (int)(pixels * down);
            X -= (int)(pixels * left);
            X += (int)(pixels * right);
        }

        _target = new Rectangle<int>(X + 24, Y - 42, 48, 48);
    }

    public bool TryPlaceBomb()
    {
        if (CurrentBombs <= 0) return false;
        CurrentBombs--;
        _bombTimers.Add(BombRechargeMs);
        return true;
    }

    public void UpdateTimers(double ms)
    {
        for (int i = _bombTimers.Count - 1; i >= 0; i--)
        {
            _bombTimers[i] -= ms;
            if (_bombTimers[i] <= 0)
            {
                _bombTimers.RemoveAt(i);
                CurrentBombs = Math.Min(CurrentBombs + 1, MaxBombs);
            }
        }

        if (_dashMs > 0) _dashMs -= ms;
        if (_dashCooldownMs > 0) _dashCooldownMs -= ms;

        if (_invincibilityMs > 0)
        {
            _invincibilityMs -= ms;
            _blinkTimer += ms;
            if (_blinkTimer >= 150)
            {
                _blinkTimer = 0;
                _blinkVisible = !_blinkVisible;
            }
        }
        else
        {
            _blinkVisible = true;
        }
    }

    public void TakeDamage()
    {
        if (IsInvincible) return;
        Lives--;
        _invincibilityMs = InvincibilityDuration;
        _blinkTimer = 0;
    }

    public void AddLife() => Lives = Math.Min(Lives + 1, MaxLives);
    public void AddSpeed(int amount) => Speed = Math.Min(Speed + amount, MaxSpeed);
    public void AddMaxBomb() { MaxBombs++; CurrentBombs++; }
    public void ReduceRecharge(double amountMs) =>
        BombRechargeMs = Math.Max(BombRechargeMs - amountMs, MinBombRechargeMs);

    public void Reset()
    {
        X = 100; Y = 100;
        Lives = 3; MaxBombs = 3; CurrentBombs = 3;
        BombRechargeMs = 1000.0; Speed = 128;
        _invincibilityMs = 0; _blinkTimer = 0; _blinkVisible = true;
        _dashMs = 0; _dashCooldownMs = 0; _lastDX = 1; _lastDY = 0;
        _bombTimers.Clear();
        _target = new Rectangle<int>(X + 24, Y - 42, 48, 48);
    }

    public void Render(GameRenderer renderer)
    {
        if (!_blinkVisible) return;
        renderer.RenderTexture(_textureId, _source, _target);
    }
}
