using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal interface IPacketHandler
    {
        void Handle(ulong sender, Packet packet);
    }
}
