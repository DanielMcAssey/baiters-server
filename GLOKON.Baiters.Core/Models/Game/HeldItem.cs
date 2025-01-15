using GLOKON.Baiters.Core.Enums.Game;

namespace GLOKON.Baiters.Core.Models.Game
{
    public struct HeldItem
    {
        public HeldItem()
        {
        }

        public required string Id { get; set; }

        public required double Size { get; set; }

        public ItemQuality Quality { get; set; } = ItemQuality.Normal;
    }
}
