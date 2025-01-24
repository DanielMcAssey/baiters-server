using GLOKON.Baiters.Core.Models.Game;

namespace GLOKON.Baiters.Server.Responses
{
    internal struct ChalkCanvasResponse(long id, ChalkCanvas chalkCanvas)
    {
        public string Id { get; set; } = id.ToString();

        public ChalkCanvasPoint[] Points { get; set; } = chalkCanvas.Cells.Values.ToArray();

        public int PointsCount { get; set; } = chalkCanvas.Cells.Count;

        public float? MinX { get; set; } = chalkCanvas.MinX;

        public float? MinY { get; set; } = chalkCanvas.MinY;

        public float? MaxX { get; set; } = chalkCanvas.MaxX;

        public float? MaxY { get; set; } = chalkCanvas.MaxY;
    }
}
