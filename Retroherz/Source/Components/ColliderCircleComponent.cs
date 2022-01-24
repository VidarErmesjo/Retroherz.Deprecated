using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Components
{
    public class CircularColliderComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Radius { get; set; }

        public CircularColliderComponent(
            Vector2 position = default(Vector2),
            Vector2 velocity = default(Vector2),
            float radius = 1.0f)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
        }

        ~CircularColliderComponent() {}    
    }
}