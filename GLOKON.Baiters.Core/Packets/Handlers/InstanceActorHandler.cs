using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;
using System.Numerics;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class InstanceActorHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            var pktParams = (Dictionary<string, object>)data["params"];
            string type = (string)pktParams["actor_type"];

            if (ActorType.ServerOnly.Contains(type))
            {
                if (server.TryGetPlayer(sender, out var playerToKick) && playerToKick != null)
                {
                    // Kick the player because the spawned in a actor that only the server should be able to spawn!
                    server.KickPlayer(sender);
                    server.SendMessage($"{playerToKick.FisherName} was kicked for trying to spawn fish & items", MessageColour.Error);
                }
            }
            else
            {
                long actorId = (long)pktParams["actor_id"];

                switch (type)
                {
                    case ActorType.Player:
                        if (server.TryGetPlayer(sender, out var playerToSpawn) && playerToSpawn != null)
                        {
                            server.JoinPlayer(sender, actorId, playerToSpawn);
                        }
                        else
                        {
                            Log.Error("No fisher to spawn for player {sender}", sender);
                        }
                        break;
                    default:
                        // TODO: Do we need to instantiate all the different types or just use the generic actor?
                        server.AddActor(actorId, new GenericActor(type, sender)
                        {
                            Position = (Vector3)pktParams["at"],
                            Rotation = (Vector3)pktParams["rot"],
                            Zone = (string)pktParams["zone"],
                            ZoneOwnerId = (long)pktParams["zone_owner"]
                        });
                        break;
                }
            }
        }
    }
}
