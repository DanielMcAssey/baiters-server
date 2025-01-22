namespace GLOKON.Baiters.Core.Exceptions
{
    public sealed class ServerSetupFailed : GameShutdownException
    {
        public ServerSetupFailed() { }

        public ServerSetupFailed(string message)
            : base(message) { }

        public ServerSetupFailed(string message, Exception inner)
            : base(message, inner) { }
    }
}
