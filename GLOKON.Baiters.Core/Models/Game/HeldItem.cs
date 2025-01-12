using GLOKON.Baiters.Core.Enums.Game;

namespace GLOKON.Baiters.Core.Models.Game
{
    public sealed class HeldItem
    {
        public required string Id { get; set; }

        public required float Size { get; set; }

        public ItemQuality Quality { get; set; } = ItemQuality.Normal;
    }
}
