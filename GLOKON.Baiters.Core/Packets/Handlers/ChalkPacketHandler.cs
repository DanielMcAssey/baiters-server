using GLOKON.Baiters.Core.Models.Actor;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ChalkPacketHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(SteamId sender, Dictionary<string, object> data)
        {
            long canvasID = (long)data["canvas_id"];
            ChalkCanvas canvas;

            if (server.TryGetActor(canvasID, out var actor) && actor is ChalkCanvas foundCanvas)
            {
                canvas = foundCanvas;
            }
            else
            {
                Log.Debug($"Creating new canvas: {canvasID}");
                canvas = new ChalkCanvas();
                server.AddActor(canvasID, canvas);
            }

            canvas.UpdateFromPacket((Dictionary<int, object>)data["data"]);
        }
    }
}
