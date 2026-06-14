using System.Text.Json;

namespace TheAdventure.Services;

public sealed class ScoreService : IDisposable
{
    private readonly string _filePath;
    private bool _disposed;

    public int HighScore { get; private set; }

    public ScoreService(string filePath = "highscore.json")
    {
        _filePath = filePath;
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath)) return;
        try
        {
            await using var stream = File.OpenRead(_filePath);
            var data = await JsonSerializer.DeserializeAsync<ScoreData>(stream);
            HighScore = data?.HighScore ?? 0;
        }
        catch (JsonException) { }
    }

    public async Task SaveAsync(int score)
    {
        if (score > HighScore) HighScore = score;
        var data = new ScoreData(HighScore);
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, data);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}

public sealed record ScoreData(int HighScore);
