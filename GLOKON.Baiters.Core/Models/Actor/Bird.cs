using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Bird : Actor
    {
        public Bird(Vector3 position) : base(ActorType.Bird)
        {
            Position = position;
            DespawnTime = 60;
        }
    }
}
