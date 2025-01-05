using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class HandshakeRequestHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Dictionary<string, object> data)
        {
            Dictionary<string, object> handshakePkt = new()
            {
                ["type"] = "handshake",
                ["user_id"] = SteamClient.SteamId.ToString()
            };
            server.SendPacket(handshakePkt, sender);
        }
    }
}
