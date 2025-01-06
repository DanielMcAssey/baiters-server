using Steamworks;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public abstract class Actor(string type, ulong ownerId = 0)
    {
        private DateTimeOffset? _despawnAt = null;

        public DateTimeOffset SpawnTime { get; } = DateTimeOffset.UtcNow;

        public string Type { get; } = type;

        public ulong OwnerId { get; } = ownerId;

        public int DespawnTime { get; protected set; } = -1;

        public bool IsDespawned { get; private set; } = false;

        public virtual void OnUpdate()
        {
            if (_despawnAt.HasValue && !IsDespawned && _despawnAt.Value >= DateTimeOffset.UtcNow)
            {
                IsDespawned = true; // Server tick will clean this actor up
            }
        }

        public virtual void Despawn()
        {
            if (DespawnTime >= 0)
            {
                _despawnAt = DateTimeOffset.UtcNow.AddSeconds(DespawnTime);
            }
        }
    }
}
