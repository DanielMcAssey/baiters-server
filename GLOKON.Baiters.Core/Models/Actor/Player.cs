using GLOKON.Baiters.GodotInterop.Models;
using Steamworks;
using Steamworks.Data;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Player(SteamId steamId, string fisherName, bool isAdmin = false) : MovableActor("player", Vector3.Zero, null, steamId.Value)
    {
        public SteamId SteamId { get; } = steamId;

        public string FisherName { get; } = fisherName;

        public IList<SteamId> BlockedPlayers { get; } = [];

        public bool IsAdmin { get; } = isAdmin;
    }
}
