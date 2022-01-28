using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Components
{
    // ActorComponent?
    public class PhysicsComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Direction { get; set; }

        //public BoundingRectangle Size { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Origin { get; set; }
        public PhysicsComponent[] Contact = new PhysicsComponent[4];

        public PhysicsComponent(
            Vector2 position = default(Vector2),
            Vector2 velocity = default(Vector2),
            Vector2 direction = default(Vector2),
            Vector2 size = default(Vector2))
        {
            Position = position;
            Velocity = velocity;
            Direction = direction;
            Size = size;
            Origin = new Vector2(size.X / 2, size.Y / 2);

            Contact[0] = null;
            Contact[1] = null;
            Contact[2] = null;
            Contact[3] = null;
        }

        ~PhysicsComponent() {}
    }
}