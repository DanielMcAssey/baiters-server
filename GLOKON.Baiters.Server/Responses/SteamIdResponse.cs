namespace GLOKON.Baiters.Server.Responses
{
    internal sealed class SteamIdResponse
    {
        public required string SteamId { get; set; }

        public required string Name { get; set; }

        public required bool IsAdmin { get; set; }
    }
}
