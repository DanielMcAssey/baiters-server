﻿using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.GodotInterop.Models;

namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class Fish : MovableActor
    {
        public Fish(string type, Vector3 position): base(type, position)
        {
            DespawnTime = type == ActorType.Fish ? 80 : 120;
        }
    }
}
