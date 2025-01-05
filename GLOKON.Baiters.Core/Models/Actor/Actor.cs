using Steamworks;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public class Actor(string type, ulong ownerId = 0)
    {
        public static readonly string[] ServerOnly = ["fish_spawn_alien", "fish_spawn", "raincloud"];

        public readonly DateTimeOffset SpawnTime = DateTimeOffset.UtcNow;

        private DateTimeOffset? _despawnAt = null;

        public string Type { get; } = type;

        public SteamId OwnerId { get; } = new SteamId() { Value = ownerId };

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
