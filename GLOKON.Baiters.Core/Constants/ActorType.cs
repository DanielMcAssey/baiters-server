namespace GLOKON.Baiters.Core.Constants
{
    public sealed class ActorType
    {
        public const string Player = "player";
        public const string Fish = "fish_spawn";
        public const string Meteor = "fish_spawn_alien";
        public const string RainCloud = "raincloud";
        public const string ChalkCanvas = "chalkcanvas";
        public const string Bird = "ambient_bird";
        public const string VoidPortal = "void_portal";
        public const string Metal = "metal_spawn";

        internal static readonly string[] ServerOnly = [
            Fish,
            Meteor,
            RainCloud,
            ChalkCanvas,
            Bird,
            VoidPortal,
            Metal,
        ];
    }
}
