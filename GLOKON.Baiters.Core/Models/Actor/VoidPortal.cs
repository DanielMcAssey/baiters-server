using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.GodotInterop.Models;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class VoidPortal : MovableActor
    {
        public VoidPortal(Vector3 position) : base(ActorType.VoidPortal, position)
        {
            DespawnTime = 600;
        }
    }
}
