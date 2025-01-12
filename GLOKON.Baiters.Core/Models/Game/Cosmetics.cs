namespace GLOKON.Baiters.Core.Models.Game
{
    public sealed class Cosmetics
    {
        public required string Title { get; set; }

        public required string Eye { get; set; }

        public required string Nose { get; set; }

        public required string Mouth { get; set; }

        public required string Undershirt { get; set; }

        public required string Overshirt { get; set; }

        public required string Legs { get; set; }

        public required string Hat { get; set; }

        public required string Species { get; set; }

        public required string[] Accessory { get; set; }

        public required string Pattern { get; set; }

        public required string PrimaryColor { get; set; }

        public required string SecondaryColor { get; set; }

        public required string Tail { get; set; }

        public string? Bobber { get; set; }
    }
}
