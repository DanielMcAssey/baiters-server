namespace GLOKON.Baiters.Core.Exceptions
{
    public sealed class LostLobbyHost : GameShutdownException
    {
        public LostLobbyHost() { }

        public LostLobbyHost(string message)
            : base(message) { }

        public LostLobbyHost(string message, Exception inner)
            : base(message, inner) { }
    }
}
