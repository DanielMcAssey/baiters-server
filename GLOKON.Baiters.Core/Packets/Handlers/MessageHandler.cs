using GLOKON.Baiters.Core.Models.Networking;
using System.Numerics;
using System.Text.RegularExpressions;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class MessageHandler(BaitersServer server) : IPacketHandler
    {
        private readonly Regex _messageCleanUpRegex = new(Regex.Escape("%u:"));

        public void Handle(ulong sender, Packet data)
        {
            string playerName = "UNKNOWN";

            if (server.TryGetPlayer(sender, out var player) && player != null)
            {
                playerName = player.FisherName;
            }

            server.OnPlayerChat(sender, new()
            {
                SentAt = DateTime.Now,
                SenderId = sender,
                SenderName = playerName,
                Message = _messageCleanUpRegex.Replace((string)data["message"], string.Empty, 1).Trim(),
                Colour = (string)data["color"],
                IsLocal = (bool)data["local"],
                Position = (Vector3)data["position"],
                Zone = (string)data["zone"],
                ZoneOwner = (long)data["zone_owner"],
            });
        }
    }
}
