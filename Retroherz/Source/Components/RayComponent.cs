using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Components
{
    public class RayComponent
    {
        public Vector2 Origin { get; set; }
        public Vector2 Direction { get; set; }

        public RayComponent(Vector2 origin = default(Vector2), Vector2 direction = default(Vector2))
        {
            Origin = origin;
            Direction = direction;
        }

        ~RayComponent() {}        
    }
}