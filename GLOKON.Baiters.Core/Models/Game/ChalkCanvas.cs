using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Game
{
    public sealed class ChalkCanvas()
    {
        // No need for concurrent dictionary, as messages are handled sequentially
        public Dictionary<int, ChalkCanvasPoint> Cells { get; } = [];

        public float MinX { get; set; } = 0f;

        public float MinY { get; set; } = 0f;

        public float MaxX { get; set; } = 0f;

        public float MaxY { get; set; } = 0f;

        public void Draw(Vector2 position, int color)
        {
            MinX = MathF.Min(position.X, MinX);
            MaxX = MathF.Max(position.X, MaxX);
            MinY = MathF.Min(position.Y, MinY);
            MaxY = MathF.Max(position.Y, MaxY);

            Cells[position.GetHashCode()] = new()
            {
                Colour = color,
                Position = position,
            };
        }

        public void UpdateFromPacket(Array packet)
        {
            foreach (var rawPacketPart in packet)
            {
                var packetPart = (Array)rawPacketPart;
                var pktPosition = (Vector2)(packetPart.GetValue(0) ?? Vector2.Zero);
                var cleanPosition = new Vector2((int)Math.Round(pktPosition.X), (int)Math.Round(pktPosition.Y));
                Draw(cleanPosition, (int)(packetPart.GetValue(1) ?? 0));
            }
        }
    }
}
