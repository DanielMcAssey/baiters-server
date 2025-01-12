using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class LetterWasAcceptedHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet packet)
        {
            // TODO: Do we need to do anything?
        }
    }
}
