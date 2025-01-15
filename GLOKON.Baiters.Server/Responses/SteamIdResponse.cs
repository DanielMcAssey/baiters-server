namespace GLOKON.Baiters.Server.Responses
{
    internal struct SteamIdResponse
    {
        public required string SteamId { get; set; }

        public required string Name { get; set; }

        public required bool IsAdmin { get; set; }
    }
}
