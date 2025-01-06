using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class RequestActorsHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
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
