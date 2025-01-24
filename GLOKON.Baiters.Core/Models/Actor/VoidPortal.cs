using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class VoidPortal : Actor
    {
        public VoidPortal(Vector3 position, ulong ownerId) : base(ActorType.VoidPortal, ownerId)
        {
            Position = position;
            DespawnTime = 600;
        }
    }
}
