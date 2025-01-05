using GLOKON.Baiters.Core.Models.Actor;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class NewPlayerJoinHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Dictionary<string, object> data)
        {
            if (!string.IsNullOrEmpty(server.Options.JoinMessage))
            {
                server.SendMessage(server.Options.JoinMessage, steamId: sender);
            }

            Dictionary<string, object> handshakePkt = new()
            {
                ["type"] = "recieve_host",
                ["host_id"] = SteamClient.SteamId.ToString()
            };
            server.SendPacket(handshakePkt);

            if (server.IsAdmin(sender))
            {
                server.SendMessage("!!You are an admin!!", "0f0f0f", sender);
            }

            Task.Run(async () =>
            {
                await SendChalkPacketsAsync(sender);
            });
        }

        private async Task SendChalkPacketsAsync(SteamId steamId)
        {
            try
            {
                foreach (KeyValuePair<long, Actor> actor in server.GetActorsByType("chalkcanvas"))
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
                        Dictionary<string, object> chalkPacket = new() {
                            ["type"] = "chalk_packet",
                            ["canvas_id"] = actor.Key,
                            ["data"] = chunks[index],
                        };
                        server.SendPacket(chalkPacket, steamId);
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
