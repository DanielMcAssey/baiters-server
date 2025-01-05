using GLOKON.Baiters.Core.Models.Actor;

namespace GLOKON.Baiters.Core.Plugins
{
    public abstract class BaitersPlugin(BaitersServer server, string name, string description, string author, string version = "0.0.0")
    {
        protected BaitersServer Server { get; private set; } = server;

        public string ID { get; } = Guid.NewGuid().ToString();

        public string Version { get; } = version;

        public string Name { get; } = name;

        public string Description { get; } = description;

        public string Author { get; } = author;

        /// <summary>
        /// Called when plugin is initialized
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// Called when plugin is cleaned up
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// Called every time the server ticks
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Called when a message is received from a player
        /// </summary>
        /// <param name="sender">The player who sent the message</param>
        /// <param name="message">The raw message</param>
        public virtual void OnChatMessage(Player sender, string message) { }

        /// <summary>
        /// Called when a player is trying to join
        /// </summary>
        /// <param name="steamId">The steamID of the player who wants to join the server</param>
        /// <returns>True if the player can join, false if not</returns>
        public virtual bool CanPlayerJoin(ulong steamId)
        {
            return true;
        }

        /// <summary>
        /// Called when a player joins the server
        /// </summary>
        /// <param name="sender">The player who joined the server</param>
        public virtual void OnPlayerJoin(Player sender) { }

        /// <summary>
        /// Called when a player leaves the server
        /// </summary>
        /// <param name="sender">The player who left the server</param>
        public virtual void OnPlayerLeft(Player sender) { }

        /// <summary>
        /// Called when a packet is received to the server
        /// </summary>
        /// <param name="sender">The player the packet is from</param>
        /// <param name="packet">The packet contents</param>
        public virtual void OnPlayerPacket(Player sender, Dictionary<string, object> packet) { }
    }
}
