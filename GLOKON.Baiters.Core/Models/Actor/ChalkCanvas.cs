using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class ChalkCanvas() : Actor(ActorType.ChalkCanvas)
    {
        // No need for concurrent dictionary, as messages are handled sequentially
        public Dictionary<Vector2, int> ChalkPoints { get; private set; } = [];

        public void Draw(Vector2 position, int color)
        {
            ChalkPoints[position] = color;
        }

        public Array GetPacket()
        {
            Array packet = Array.CreateInstance(typeof(object), ChalkPoints.Count);
            int imageIndex = 0;

            foreach (var position in ChalkPoints.Keys)
            {
                packet.SetValue(new object[] { position, ChalkPoints[position] }, imageIndex);
                imageIndex++;
            }

            return packet;
        }

        public void UpdateFromPacket(Array packet)
        {
            foreach (var rawPacketPart in packet)
            {
                var packetPart = (Array)rawPacketPart;
                var pktPosition = (Vector2)(packetPart.GetValue(0) ?? Vector2.Zero);
                var position = new Vector2((int)Math.Round(pktPosition.X), (int)Math.Round(pktPosition.Y));
                ChalkPoints[position] = (int)(packetPart.GetValue(1) ?? 0);
            }
        }
    }
}
