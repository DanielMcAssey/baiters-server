using System.Numerics;

namespace GLOKON.Baiters.Core.Models.Game
{
    public sealed class ChalkCanvas()
    {
        // No need for concurrent dictionary, as messages are handled sequentially
        public Dictionary<int, ChalkCanvasPoint> Cells { get; set; } = [];

        public float? MinX { get; set; }

        public float? MinY { get; set; }

        public float? MaxX { get; set; }

        public float? MaxY { get; set; }

        public void Draw(Vector2 position, int color)
        {
            MinX = MinX.HasValue ? MathF.Min(position.X, MinX.Value) : position.X;
            MaxX = MaxX.HasValue ? MathF.Max(position.X, MaxX.Value) : position.X;
            MinY = MinY.HasValue ? MathF.Min(position.Y, MinY.Value) : position.Y;
            MaxY = MaxY.HasValue ? MathF.Max(position.Y, MaxY.Value) : position.Y;

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
