namespace GLOKON.Baiters.Core.Models.Actor
{
    public abstract class Actor(string type, ulong ownerId = 0)
    {
        private DateTimeOffset? _despawnAt = null;
        private uint? _despawnTime = null;

        public DateTimeOffset SpawnTime { get; } = DateTimeOffset.UtcNow;

        public string Type { get; } = type;

        public ulong OwnerId { get; } = ownerId;

        public uint? DespawnTime
        {
            get { return _despawnTime; }
            protected set
            {
                _despawnTime = value;

                if (_despawnTime.HasValue)
                {
                    _despawnAt = DateTimeOffset.UtcNow.AddSeconds(_despawnTime.Value);
                }
                else
                {
                    _despawnAt = null;
                }
            }
        }

        public bool IsDespawned { get; private set; } = false;

        public virtual void OnUpdate()
        {
            if (_despawnAt.HasValue && !IsDespawned && _despawnAt.Value >= DateTimeOffset.UtcNow)
            {
                IsDespawned = true; // Server tick will clean this actor up
            }
        }
    }
}
