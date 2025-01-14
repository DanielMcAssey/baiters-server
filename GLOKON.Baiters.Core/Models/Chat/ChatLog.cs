namespace GLOKON.Baiters.Core.Models.Chat
{
    public sealed class ChatLog
    {
        public required ulong SenderId { get; set; }

        public required string SenderName { get; set; }

        public required string Colour { get; set; }

        public required string Message { get; set; }
    }
}
