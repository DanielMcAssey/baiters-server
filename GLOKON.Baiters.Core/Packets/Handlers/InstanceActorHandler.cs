using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class InstanceActorHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Packet data)
        {
            var pktParams = (Dictionary<string, object>)data["params"];
            string type = (string)pktParams["actor_type"];

            if (ActorType.ServerOnly.Contains(type))
            {
                if (server.TryGetPlayer(sender, out var playerToKick) && playerToKick != null)
                {
                    // Kick the player because the spawned in a actor that only the server should be able to spawn!
                    server.SendPacket(new("kick"), sender);
                    server.SendMessage($"{playerToKick.FisherName} was kicked for trying to spawn fish & items");
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
                        // TODO: Do we need to instantiate all the different types or just use the actor?
                        server.AddActor(actorId, new GenericActor(type, sender));
                        break;
                }
            }
        }
    }
}
