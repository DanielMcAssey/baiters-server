using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Metal(Vector3 position) : MovableActor(ActorType.Metal, position)
    {
    }
}
