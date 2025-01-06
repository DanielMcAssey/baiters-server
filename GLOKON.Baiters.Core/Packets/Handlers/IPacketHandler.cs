using GLOKON.Baiters.Core.Models.Networking;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal interface IPacketHandler
    {
        void Handle(SteamId sender, Packet packet);
    }
}
