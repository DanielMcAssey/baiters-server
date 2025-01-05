using GLOKON.Baiters.Core.Models.Actor;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ActorActionHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Dictionary<string, object> data)
        {
            switch ((string)data["action"])
            {
                case "_sync_create_bubble":
                    string chatMssage = (string)((Dictionary<int, object>)data["params"])[0];
                    server.OnPlayerChat(sender, chatMssage);
                    break;
                case "_wipe_actor":
                    long wipeActorId = (long)((Dictionary<int, object>)data["params"])[0];
                    if (server.TryGetActor(wipeActorId, out var actor) && actor != null)
                    {
                        if (Actor.ServerOnly.Contains(actor.Type))
                        {
                            return;
                        }

                        Log.Debug($"Player asked to remove {actor.Type} actor");
                        server.RemoveActor(wipeActorId);
                    }
                    break;
            }
        }
    }
}
