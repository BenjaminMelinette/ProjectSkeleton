namespace TheAdventure.Exceptions;

public class GameException : Exception
{
    public GameException(string message) : base(message) { }
    public GameException(string message, Exception inner) : base(message, inner) { }
}
