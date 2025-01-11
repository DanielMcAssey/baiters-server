namespace GLOKON.Baiters.Core.Models.Actor
{
    public abstract class Actor(string type, ulong ownerId = 0)
    {
        private DateTimeOffset? _despawnAt = null;
        private uint? _despawnTime = null;
        private bool _syncRequired = false;

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

        public bool IsSyncRequired
        {
            get
            {
                // Works as a single call fuse, resets sync required state after property is viewed
                var currentSyncRequired = _syncRequired;
                _syncRequired = false;
                return currentSyncRequired;
            }
            protected set
            {
                _syncRequired = value;
            }
        }

        public virtual void OnUpdate()
        {
            if (_despawnAt.HasValue && !IsDespawned && _despawnAt.Value >= DateTimeOffset.UtcNow)
            {
                IsDespawned = true; // Server tick will clean this actor up
            }
        }
    }
}
