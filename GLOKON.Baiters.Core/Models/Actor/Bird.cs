using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Bird(Vector3 position) : MovableActor(ActorType.Bird, position)
    {
    }
}
