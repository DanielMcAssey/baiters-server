using GLOKON.Baiters.Core.Models.Game;

namespace GLOKON.Baiters.Server.Responses
{
    internal struct BanResponse(ulong steamId, PlayerBan playerBan)
    {
        public DateTime CreatedAt { get; set; } = playerBan.CreatedAt;

        public ulong SteamId { get; set; } = steamId;

        public string PlayerName { get; set; } = playerBan.PlayerName;

        public string? Reason { get; set; } = playerBan.Reason;
    }
}
