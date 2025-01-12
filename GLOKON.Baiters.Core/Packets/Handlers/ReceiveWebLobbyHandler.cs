using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ReceiveWebLobbyHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet packet)
        {
            // Nothing to do
        }
    }
}
