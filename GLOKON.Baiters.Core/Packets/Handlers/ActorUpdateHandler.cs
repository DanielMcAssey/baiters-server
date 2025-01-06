using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Models.Networking;
using GLOKON.Baiters.GodotInterop.Models;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ActorUpdateHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Packet data)
        {
            long actorId = (long)data["actor_id"];
            if (server.TryGetActor(actorId, out var actor) && actor is Player player)
            {
                player.Position = (Vector3)data["pos"];
            }
        }
    }
}
