using GLOKON.Baiters.Core.Constants;
using System.Numerics;


namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class RainCloud : MovableActor
    {
        private readonly float _wanderDirection;

        public bool IsStatic { get; } = false;

        public RainCloud(Vector3 position): base(ActorType.RainCloud, position)
        {
            Vector3 toCenter = Vector3.Normalize(position - new Vector3(30, 40, -50));
            _wanderDirection = (float)Math.Atan2(toCenter.Z, toCenter.X);
            DespawnTime = 540;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (IsStatic)
            {
                // Dont move static rain
                return;
            }

            Vector2 dir = Vector2.Transform(new Vector2(-1, 0), Matrix3x2.CreateRotation(_wanderDirection)) * (0.17f / 6f);
            Position += new Vector3(dir.X, 0, dir.Y);
        }
    }
}
