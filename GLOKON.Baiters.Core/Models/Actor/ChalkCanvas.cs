using GLOKON.Baiters.Core.Constants;
using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class ChalkCanvas() : Actor(ActorType.ChalkCanvas)
    {
        public Dictionary<Vector2, long> ChalkPoints { get; private set; } = [];

        public void Draw(Vector2 position, long color)
        {
            ChalkPoints[position] = color;
        }

        public Dictionary<int, object> GetPacket()
        {
            Dictionary<int, object> packet = [];
            ulong imageIndex = 0;

            foreach (var position in ChalkPoints.Keys)
            {
                Dictionary<int, object> packetPart = [];

                packetPart[0] = position;
                packetPart[1] = ChalkPoints[position];
                packet[(int)imageIndex] = packetPart; // Potential integer overflow here, does it matter?
                imageIndex++;
            }

            return packet;
        }

        public void UpdateFromPacket(Dictionary<int, object> packet)
        {
            foreach (var rawPacketPart in packet.Values)
            {
                var packetPart = (Dictionary<int, object>)rawPacketPart;
                ChalkPoints[(Vector2)packetPart[0]] = (long)packetPart[1];
            }
        }
    }
}
