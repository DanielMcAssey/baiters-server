using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class RequestPingHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            server.SendPacket(new("send_ping")
            {
                ["time"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ["from"] = server.ServerId.ToString(),
            }, sender);
        }
    }
}
