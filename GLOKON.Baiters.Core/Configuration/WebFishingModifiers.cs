namespace GLOKON.Baiters.Core.Configuration
{
    public class WebFishingModifiers
    {
        public float FishChance { get; set; } = 0.50f;

        public float RainChance { get; set; } = 0.25f;

        public float BirdChance { get; set; } = 0.33f;

        public float MeteorChance { get; set; } = 0.01f;

        public float VoidPortalChance { get; set; } = 0.005f;

        public int MaxMetal { get; set; } = 7;

        public int MaxNpcActors { get; set; } = 50;

        public int MaxFish { get; set; } = 30;

        public int MaxBird { get; set; } = 8;

        public int TicksPerSecond { get; set; } = 24; // 24hz
    }
}
