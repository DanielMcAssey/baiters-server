using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.GodotInterop.Models;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class RequestActorsHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Dictionary<string, object> data)
        {
            foreach (var actor in server.Actors)
            {
                Dictionary<string, object> spawnPkt = new()
                {
                    ["type"] = "instance_actor"
                };

                Dictionary<string, object> instanceSpacePrams = [];
                spawnPkt["params"] = instanceSpacePrams;

                instanceSpacePrams["actor_type"] = actor.Value.Type;

                if (actor.Value is MovableActor movableActor)
                {
                    instanceSpacePrams["at"] = movableActor.Position;
                    instanceSpacePrams["rot"] = movableActor.Rotation;
                }
                else
                {
                    instanceSpacePrams["at"] = Vector3.Zero;
                    instanceSpacePrams["rot"] = Vector3.Zero;
                }

                instanceSpacePrams["zone"] = "main_zone";
                instanceSpacePrams["zone_owner"] = -1;
                instanceSpacePrams["actor_id"] = actor.Key;
                instanceSpacePrams["creator_id"] = (long)SteamClient.SteamId.Value;

                server.SendPacket(spawnPkt, sender);

                Dictionary<string, object> createPkt = new()
                {
                    ["type"] = "actor_request_send",
                    ["list"] = new Dictionary<int, object>()
                };

                server.SendPacket(createPkt, sender);
            }
        }
    }
}
