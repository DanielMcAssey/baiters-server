using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Game;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Player(ulong steamId, string fisherName, bool isAdmin = false) : Actor(ActorType.Player, steamId)
    {
        public string FisherName { get; } = fisherName;

        public ISet<ulong> BlockedPlayers { get; } = new HashSet<ulong>();

        public bool IsAdmin { get; } = isAdmin;

        public long? ActorId { get; set; }

        public string? LastEmote { get; set; }

        public Cosmetics? Cosmetics { get; set; }

        public HeldItem? HeldItem { get; set; }
    }
}
