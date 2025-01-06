using GLOKON.Baiters.Core.Models.Networking;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class RequestActorsHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Packet data)
        {
            foreach (var actor in server.Actors)
            {
                server.SendActor(actor.Key, actor.Value, sender);
                server.SendPacket(new("actor_request_send")
                {
                    ["list"] = new Dictionary<int, object>(),
                }, sender);
            }
        }
    }
}
