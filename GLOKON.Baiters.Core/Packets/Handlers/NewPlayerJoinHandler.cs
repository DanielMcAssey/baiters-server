using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Enums.Networking;
using GLOKON.Baiters.Core.Models.Game;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class NewPlayerJoinHandler(BaitersServer server, string commandPrefix, string? joinMessage) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            if (!string.IsNullOrEmpty(joinMessage))
            {
                server.SendSystemMessage(joinMessage, MessageColour.Information, sender);
            }

            server.SendPacket(new("recieve_host")
            {
                ["host_id"] = server.ServerId.ToString(),
            }, DataChannel.GameState);

            if (server.IsAdmin(sender))
            {
                server.SendSystemMessage("##You are an admin##", MessageColour.Success, sender);
                server.SendSystemMessage(string.Format("Use '{0}help' to find out what commands are available", commandPrefix), MessageColour.Success, sender);
            }

            Task.Run(async () =>
            {
                await SendChalkPacketsAsync(sender);

                foreach (var actor in server.Actors)
                {
                    server.SendActorUpdate(actor.Key, actor.Value, sender);
                }

                if (server.ServerActor.ActorId.HasValue)
                {
                    server.SendActor(server.ServerActor.ActorId.Value, server.ServerActor, sender);
                }
            });
        }

        private async Task SendChalkPacketsAsync(ulong steamId)
        {
            try
            {
                foreach (KeyValuePair<long, ChalkCanvas> canvas in server.ChalkCanvases)
                {
                    server.SendCanvas(canvas.Key, canvas.Value.Cells.Values.ToList(), steamId);
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send Chalk Packets");
            }
        }
    }
}
