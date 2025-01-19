using GLOKON.Baiters.Core.Models.Actor;
using System.Numerics;

namespace GLOKON.Baiters.Server.Responses
{
    internal struct ActorResponse(long id, Actor actor)
    {
        public DateTimeOffset SpawnedAt { get; set; } = actor.SpawnedAt;

        public long Id { get; set; } = id;

        public string Type { get; set; } = actor.Type;

        public ulong OwnerId { get; set; } = actor.OwnerId;

        public Vector3 Position { get; set; } = actor.Position;

        public Vector3 Rotation { get; set; } = actor.Rotation;

        public string Zone { get; set; } = actor.Zone;

        public uint? DespawnTime { get; set; } = actor.DespawnTime;
    }
}
