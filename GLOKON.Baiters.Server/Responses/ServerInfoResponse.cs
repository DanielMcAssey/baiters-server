﻿namespace GLOKON.Baiters.Server.Responses
{
    public class ServerInfoResponse
    {
        public required ulong ServerSteamId { get; set; }

        public required string LobbyCode { get; set; }

        public required int PlayerCount { get; set; }

        public required int MaxPlayers { get; set; }
    }
}
