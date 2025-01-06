using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class HandshakeRequestHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            server.SendPacket(new("handshake")
            {
                ["user_id"] = server.ServerId.ToString(),
            }, sender);
        }
    }
}
