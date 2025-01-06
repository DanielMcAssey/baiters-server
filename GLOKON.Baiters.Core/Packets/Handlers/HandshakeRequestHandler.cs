using GLOKON.Baiters.Core.Models.Networking;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class HandshakeRequestHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Packet data)
        {
            server.SendPacket(new("handshake")
            {
                ["user_id"] = SteamClient.SteamId.ToString(),
            }, sender);
        }
    }
}
