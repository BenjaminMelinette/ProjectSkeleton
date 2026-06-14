using Silk.NET.Maths;

namespace TheAdventure.Models;

public class AnimatedGameObject : RenderableGameObject
{
    private readonly int _durationInSeconds;
    private readonly int _numberOfFrames;
    private readonly int _numberOfColumns;
    private readonly int _columnWidth;
    private readonly int _rowHeight;
    private readonly int _timePerFrame;

    private double _timeSinceAnimationStart = 0;
    private int _currentRow = 0;
    private int _currentColumn = 0;

    public AnimatedGameObject(string fileName, GameRenderer renderer, int durationInSeconds,
        int numberOfFrames, int numberOfColumns, int numberOfRows, int x, int y)
        : base(fileName, renderer)
    {
        _durationInSeconds = durationInSeconds;
        _numberOfFrames = numberOfFrames;
        _numberOfColumns = numberOfColumns;
        _columnWidth = TextureInformation.Width / numberOfColumns;
        _rowHeight = TextureInformation.Height / numberOfRows;

        _timePerFrame = (durationInSeconds * 1000) / numberOfFrames;

        var halfRow = _rowHeight / 2;
        var halfColumn = _columnWidth / 2;
        TextureDestination = new Rectangle<int>(x - halfColumn, y - halfRow, _columnWidth, _rowHeight);
        TextureSource = new Rectangle<int>(0, 0, _columnWidth, _rowHeight);
    }

    public override bool Update(double msSinceLastFrame)
    {
        _timeSinceAnimationStart += msSinceLastFrame;

        var currentFrame = _timeSinceAnimationStart / _timePerFrame;

        if (_timeSinceAnimationStart > _durationInSeconds * 1000) return false;

        _currentRow = (int)(currentFrame / _numberOfColumns);
        _currentColumn = (int)(currentFrame % _numberOfColumns);

        TextureSource = new Rectangle<int>(
            _currentColumn * _columnWidth, _currentRow * _rowHeight,
            _columnWidth, _rowHeight);

        return true;
    }
}
