namespace GLOKON.Baiters.Server.Responses
{
    internal struct ServerInfoResponse
    {
        public required string ServerSteamId { get; set; }

        public required string LobbyCode { get; set; }

        public required int PlayerCount { get; set; }

        public required int MaxPlayers { get; set; }
    }
}
