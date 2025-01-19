using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Fish : Actor
    {
        public Fish(string type, Vector3 position): base(type)
        {
            Position = position;
            DespawnTime = (uint)(type == ActorType.Fish ? 80 : 120);
        }
    }
}
