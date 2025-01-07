using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.GodotInterop.Models;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Player(ulong steamId, string fisherName, bool isAdmin = false) : MovableActor(ActorType.Player, Vector3.Zero, null, steamId)
    {
        public string FisherName { get; } = fisherName;

        public ISet<ulong> BlockedPlayers { get; } = new HashSet<ulong>();

        public bool IsAdmin { get; } = isAdmin;

        public long? ActorId { get; private set; }

        public void SetActorId(long actorId)
        {
            ActorId = actorId;
        }
    }
}
