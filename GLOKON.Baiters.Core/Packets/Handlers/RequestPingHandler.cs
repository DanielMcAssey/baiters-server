using GLOKON.Baiters.Core.Models.Networking;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class RequestPingHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Packet data)
        {
            server.SendPacket(new("send_ping")
            {
                ["time"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ["from"] = SteamClient.SteamId.ToString(),
            }, sender);
        }
    }
}
