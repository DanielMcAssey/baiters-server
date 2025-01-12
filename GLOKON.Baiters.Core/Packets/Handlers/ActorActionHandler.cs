using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ActorActionHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            switch ((string)data["action"])
            {
                case "_sync_create_bubble":
                    string chatMssage = (string)(((Array)data["params"]).GetValue(0) ?? -1);
                    server.OnPlayerChat(sender, chatMssage);
                    break;
                case "_wipe_actor":
                    long wipeActorId = (long)(((Array)data["params"]).GetValue(0) ?? -1);
                    if (server.TryGetActor(wipeActorId, out var actor) && actor != null)
                    {
                        if (ActorType.ServerOnly.Contains(actor.Type))
                        {
                            return;
                        }

                        Log.Debug("Player asked to remove {0} actor", actor.Type);
                        server.RemoveActor(wipeActorId);
                    }
                    break;
            }
        }
    }
}
