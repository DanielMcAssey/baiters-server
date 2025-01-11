using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Bird : MovableActor
    {
        public Bird(Vector3 position) : base(ActorType.Bird, position)
        {
            DespawnTime = 60;
        }
    }
}
