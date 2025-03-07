﻿using GLOKON.Baiters.Core.Constants;
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

            if (server.IsAdmin(sender))
            {
                server.SendSystemMessage("##You are an admin##", MessageColour.Success, sender);
                server.SendSystemMessage(string.Format("Use '{0}help' to find out what commands are available", commandPrefix), MessageColour.Success, sender);
            }

            Task.Run(() => SendChalkPacketsAsync(sender));
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
