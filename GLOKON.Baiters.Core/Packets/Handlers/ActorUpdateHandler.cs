using GLOKON.Baiters.Core.Models.Networking;
using System.Numerics;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ActorUpdateHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            long actorId = (long)data["actor_id"];
            if (server.TryGetActor(actorId, out var actor) && actor != null)
            {
                actor.Position = (Vector3)data["pos"];
                actor.Rotation = (Vector3)data["rot"];
            }
        }
    }
}
