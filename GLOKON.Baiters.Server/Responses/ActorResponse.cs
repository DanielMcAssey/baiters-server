using GLOKON.Baiters.Core.Models.Actor;

namespace GLOKON.Baiters.Server.Responses
{
    internal struct ActorResponse(long id, Actor actor)
    {
        public DateTimeOffset SpawnedAt { get; set; } = actor.SpawnedAt;

        public long Id { get; set; } = id;

        public string Type { get; set; } = actor.Type;

        public ulong OwnerId { get; set; } = actor.OwnerId;

        public string Zone { get; set; } = actor.Zone;

        public uint? DespawnTime { get; set; } = actor.DespawnTime;
    }
}
