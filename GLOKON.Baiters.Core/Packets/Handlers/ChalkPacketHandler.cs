using GLOKON.Baiters.Core.Models.Chat;
using GLOKON.Baiters.Core.Models.Game;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ChalkPacketHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            var canvasId = (long)data["canvas_id"];
            var canvasData = (Array)data["data"];
            ChalkCanvas canvas;

            if (server.TryGetChalkCanvas(canvasId, out var foundCanvas) && foundCanvas != null)
            {
                Log.ForContext("Scope", "ChalkCanvas").Information("[{0}] Updating chalk canvas({1}), with {2} points", sender, canvasId, canvasData.Length);
                canvas = foundCanvas;
            }
            else
            {
                Log.ForContext("Scope", "ChalkCanvas").Information("[{0}] Creating new chalk canvas({1}), with {2} points", sender, canvasId, canvasData.Length);
                canvas = new();
                server.AddChalkCanvas(canvasId, canvas);
            }

            canvas.UpdateFromPacket(canvasData);
        }
    }
}
