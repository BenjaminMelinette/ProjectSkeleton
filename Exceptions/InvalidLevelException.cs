namespace TheAdventure.Exceptions;

public sealed class InvalidLevelException : GameException
{
    public InvalidLevelException(string message) : base(message) { }
}
