using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class RequestPingHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Dictionary<string, object> data)
        {
            Dictionary<string, object> pongPkt = new()
            {
                ["type"] = "send_ping",
                ["time"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ["from"] = SteamClient.SteamId.ToString(),
            };

            server.SendPacket(pongPkt, sender);
        }
    }
}
