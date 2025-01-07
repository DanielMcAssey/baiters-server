using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public class MovableActor(string type, Vector3 position, Vector3? rotation = null, ulong ownerId = 0) : Actor(type, ownerId)
    {
        public Vector3 Position { get; set; } = position;

        public Vector3 Rotation { get; set; } = rotation ?? Vector3.Zero;
    }
}
