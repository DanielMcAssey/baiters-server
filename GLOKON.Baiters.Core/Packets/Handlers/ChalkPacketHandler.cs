using GLOKON.Baiters.Core.Models.Game;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ChalkPacketHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            long canvasId = (long)data["canvas_id"];
            ChalkCanvas canvas;

            if (server.TryGetChalkCanvas(canvasId, out var foundCanvas) && foundCanvas != null)
            {
                canvas = foundCanvas;
            }
            else
            {
                Log.Debug("Creating new chalk canvas({0})", canvasId);
                canvas = new();
                server.AddChalkCanvas(canvasId, canvas);
            }

            canvas.UpdateFromPacket((Array)data["data"]);
        }
    }
}
