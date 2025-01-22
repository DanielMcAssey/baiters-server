namespace GLOKON.Baiters.Core.Exceptions
{
    public sealed class LobbyGoneException : GameShutdownException
    {
        public LobbyGoneException() { }

        public LobbyGoneException(string message)
            : base(message) { }

        public LobbyGoneException(string message, Exception inner)
            : base(message, inner) { }
    }
}
