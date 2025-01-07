using Serilog;

namespace GLOKON.Baiters.Core.Plugins
{
    public abstract class BaitersPlugin(GameManager gm, string name, string description, string author, string version = "0.0.0")
    {
        protected GameManager GM { get; } = gm;

        public string ID { get; } = Guid.NewGuid().ToString();

        public string Version { get; } = version;

        public string Name { get; } = name;

        public string Description { get; } = description;

        public string Author { get; } = author;

        /// <summary>
        /// Called when plugin is initialized
        /// </summary>
        public virtual void OnInit()
        {
            Log.Debug("[{Name}] Plugin Initialized ({Version})", Name, Version);
        }

        /// <summary>
        /// Called when plugin is cleaned up
        /// </summary>
        public virtual void OnDestroy()
        {
            Log.Debug("[{Name}] Plugin Unloaded ({Version})", Name, Version);
        }

        /// <summary>
        /// Called when a player is trying to join
        /// </summary>
        /// <param name="steamId">The SteamID of the player who wants to join the server</param>
        /// <returns>True if the player can join, false if not</returns>
        public virtual bool CanPlayerJoin(ulong steamId)
        {
            return true;
        }
    }
}
