using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.GodotInterop.Models;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class RainCloud : MovableActor
    {
        private readonly float _wanderDirection;

        public bool IsStatic { get; } = false;

        public RainCloud(Vector3 position): base(ActorType.RainCloud, position)
        {
            Vector3 toCenter = (position - new Vector3(30, 40, -50)).Normalized();
            _wanderDirection = new Vector2(toCenter.x, toCenter.z).Angle();
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

            Vector2 dir = new Vector2(-1, 0).Rotate(_wanderDirection) * (0.17f / 6f);
            Position += new Vector3(dir.x, 0, dir.y);
        }
    }
}
