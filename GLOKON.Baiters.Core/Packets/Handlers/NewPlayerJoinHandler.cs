using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Enums.Networking;
using GLOKON.Baiters.Core.Models.Actor;
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
            });
        }

        private async Task SendChalkPacketsAsync(ulong steamId)
        {
            try
            {
                foreach (KeyValuePair<long, Actor> actor in server.GetActorsByType(ActorType.ChalkCanvas))
                {
                    ChalkCanvas canvas = (ChalkCanvas)actor.Value;

                    // split the dictionary into chunks of 100
                    List<object[]> chunks = [];
                    List<object> chunk = [];

                    int chunkIndex = 0;
                    foreach (var chalkData in canvas.GetPacket())
                    {
                        if (chunkIndex >= 1000)
                        {
                            chunks.Add(chunk.ToArray());
                            chunk.Clear();
                            chunkIndex = 0;
                        }

                        chunk.Add(chalkData);
                        chunkIndex++;
                    }

                    chunks.Add(chunk.ToArray());

                    foreach (var chunkToSend in chunks)
                    {
                        server.SendPacket(new("chalk_packet")
                        {
                            ["canvas_id"] = actor.Key,
                            ["data"] = chunkToSend,
                        }, DataChannel.Chalk, steamId);
                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send Chalk Packets");
            }
        }
    }
}
