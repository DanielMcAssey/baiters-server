using GLOKON.Baiters.Core.Models.Networking;
using System.Numerics;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class MessageHandler(BaitersServer server) : IPacketHandler
    {
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
                Message = (string)data["message"],
                Colour = (string)data["color"],
                IsLocal = (bool)data["local"],
                Position = (Vector3)data["position"],
                Zone = (string)data["zone"],
                ZoneOwner = (long)data["zone_owner"],
            });
        }
    }
}
