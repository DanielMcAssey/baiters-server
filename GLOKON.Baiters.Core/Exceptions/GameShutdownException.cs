namespace GLOKON.Baiters.Core.Exceptions
{
    public abstract class GameShutdownException : Exception
    {
        public GameShutdownException() { }

        public GameShutdownException(string message)
            : base(message) { }

        public GameShutdownException(string message, Exception inner)
            : base(message, inner) { }
    }
}
