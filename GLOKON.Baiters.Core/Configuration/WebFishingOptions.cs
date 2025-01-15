using GLOKON.Baiters.Core.Enums.Configuration;

namespace GLOKON.Baiters.Core.Configuration
{
    public class WebFishingOptions
    {
        public uint AppId { get; } = 3146520; // Fixed to WebFishing Steam AppID

        public string AppVersion { get; } = "1.12"; // Update this when game updates

        public string ServerName { get; set; } = "My Baiters Server";

        public IList<ulong> Admins { get; set; } = [];

        public bool PluginsEnabled { get; set; } = false;

        public int MaxPlayers { get; set; } = 50;

        public string CommandPrefix { get; set; } = "!";

        public JoinType JoinType { get; set; } = JoinType.Public;

        public string? JoinMessage { get; set; } = null;

        public IList<string> Tags { get; set; } = [];

        public bool SaveChalkCanvases { get; set; } = false;

        public bool SteamDebug { get; set; } = false;

        public WebFishingModifiers Modifiers { get; set; } = new WebFishingModifiers();
    }
}
