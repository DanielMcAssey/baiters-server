using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class VoidPortal : Actor
    {
        public VoidPortal(Vector3 position) : base(ActorType.VoidPortal)
        {
            Position = position;
            DespawnTime = 600;
        }
    }
}
