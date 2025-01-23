using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Enums.Networking;
using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class RequestActorsHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            foreach (var actor in server.OwnedActors)
            {
                server.SendActor(actor.Key, actor.Value, sender);
            }

            server.SendPacket(new(PacketType.ActorRequestSend)
            {
                ["list"] = Array.Empty<object>(),
            }, DataChannel.GameState, sender);
        }
    }
}
