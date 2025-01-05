using GLOKON.Baiters.Core.Models.Actor;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class InstanceActorHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Dictionary<string, object> data)
        {
            string type = (string)((Dictionary<string, object>)data["params"])["actor_type"];
            long actorId = (long)((Dictionary<string, object>)data["params"])["actor_id"];

            if (Actor.ServerOnly.Contains(type) && server.TryGetPlayer(sender, out var playerToKick) && playerToKick != null)
            {
                // Kick the player because the spawned in a actor that only the server should be able to spawn!
                Dictionary<string, object> kickPkt = new()
                {
                    ["type"] = "kick"
                };

                server.SendPacket(kickPkt, sender);
                server.SendMessage($"{playerToKick.FisherName} was kicked for trying to spawn fish & items");
            }
            else if (type == "player")
            {
                if (server.TryGetPlayer(sender, out var playerToSpawn) && playerToSpawn != null)
                {
                    server.JoinPlayer(actorId, playerToSpawn);
                }
                else
                {
                    Log.Error($"No fisher to spawn for player {sender}");
                }
            }
            else
            {
                server.AddActor(actorId, new Actor(type, sender));
            }
        }
    }
}
