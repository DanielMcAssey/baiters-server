namespace GLOKON.Baiters.Core.Models.Game
{
    public struct PlayerBan
    {
        public DateTime CreatedAt { get; set; }

        public string PlayerName { get; set; }

        public string? Reason { get; set; }
    }
}
