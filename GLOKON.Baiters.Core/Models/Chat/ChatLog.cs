using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Chat
{
    public sealed class ChatLog
    {
        public required DateTime SentAt { get; set; }

        public required ulong SenderId { get; set; }

        public required string SenderName { get; set; }

        public ulong? SendToId { get; set; }

        public required string Message { get; set; }

        public required string Colour { get; set; }

        public required bool IsLocal { get; set; }

        public required Vector3 Position { get; set; }

        public required string Zone { get; set; }

        public required long ZoneOwner { get; set; }
    }
}
