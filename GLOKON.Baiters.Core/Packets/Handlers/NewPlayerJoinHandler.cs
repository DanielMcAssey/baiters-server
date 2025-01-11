using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Enums.Networking;
using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class NewPlayerJoinHandler(BaitersServer server, string? joinMessage) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            if (!string.IsNullOrEmpty(joinMessage))
            {
                server.SendMessage(joinMessage, steamId: sender);
            }

            server.SendPacket(new("recieve_host")
            {
                ["host_id"] = server.ServerId.ToString(),
            }, DataChannel.GameState);

            if (server.IsAdmin(sender))
            {
                server.SendMessage("!!You are an admin!!", "0f0f0f", sender);
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
                    Dictionary<int, object> chalkPkt = canvas.GetPacket();

                    // split the dictionary into chunks of 100
                    List<Dictionary<int, object>> chunks = [];
                    Dictionary<int, object> chunk = [];

                    int i = 0;
                    foreach (var kvp in chalkPkt)
                    {
                        if (i >= 1000)
                        {
                            chunks.Add(chunk);
                            chunk = [];
                            i = 0;
                        }

                        chunk.Add(i, kvp.Value);
                        i++;
                    }

                    chunks.Add(chunk);

                    for (int index = 0; index < chunks.Count; index++)
                    {
                        server.SendPacket(new("chalk_packet")
                        {
                            ["canvas_id"] = actor.Key,
                            ["data"] = chunks[index],
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
