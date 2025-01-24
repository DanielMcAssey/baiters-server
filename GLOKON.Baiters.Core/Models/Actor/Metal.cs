using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Metal : Actor
    {
        public Metal(Vector3 position, ulong ownerId) : base(ActorType.Metal, ownerId)
        {
            Position = position;
        }
    }
}
