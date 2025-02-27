﻿using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public abstract class Actor(string type, ulong ownerId)
    {
        private DateTimeOffset? _despawnAt = null;
        private uint? _despawnTime = null;
        private bool _syncRequired = true; // Sync it initially

        public DateTimeOffset SpawnedAt { get; } = DateTimeOffset.UtcNow;

        public string Type { get; } = type;

        public Vector3 Position { get; set; } = Vector3.Zero;

        public Vector3 Rotation { get; set; } = Vector3.Zero;

        public ulong OwnerId { get; } = ownerId;

        public string Zone { get; set; } = "main_zone";

        public long ZoneOwnerId { get; set; } = -1;

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
            if (_despawnAt.HasValue && !IsDespawned && _despawnAt.Value <= DateTimeOffset.UtcNow)
            {
                IsDespawned = true; // Server tick will clean this actor up
            }
        }
    }
}
