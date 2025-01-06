using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ChalkPacketHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Packet data)
        {
            long canvasId = (long)data["canvas_id"];
            ChalkCanvas canvas;

            if (server.TryGetActor(canvasId, out var actor) && actor is ChalkCanvas foundCanvas)
            {
                canvas = foundCanvas;
            }
            else
            {
                Log.Debug("Creating new canvas({0})", canvasId);
                canvas = new();
                server.AddActor(canvasId, canvas);
            }

            canvas.UpdateFromPacket((Dictionary<int, object>)data["data"]);
        }
    }
}
