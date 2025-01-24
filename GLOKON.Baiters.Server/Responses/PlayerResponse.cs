using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Models.Game;
using System.Numerics;

namespace GLOKON.Baiters.Server.Responses
{
    internal struct PlayerResponse(ulong id, Player player)
    {
        public DateTimeOffset SpawnedAt { get; set; } = player.SpawnedAt;

        public string Id { get; set; } = id.ToString();

        public string Type { get; set; } = player.Type;

        public string OwnerId { get; set; } = player.OwnerId.ToString();

        public Vector3 Position { get; set; } = player.Position;

        public Vector3 Rotation { get; set; } = player.Rotation;

        public string Zone { get; set; } = player.Zone;

        public uint? DespawnTime { get; set; } = player.DespawnTime;

        public string FisherName { get; set; } = player.FisherName;

        public bool IsAdmin { get; set; } = player.IsAdmin;

        public string? ActorId { get; set; } = player.ActorId?.ToString() ?? null;

        public string? LastEmote { get; set; } = player.LastEmote;

        public Cosmetics? Cosmetics { get; set; } = player.Cosmetics;

        public HeldItem? HeldItem { get; set; } = player.HeldItem;
    }
}
