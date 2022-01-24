using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Components
{
    public class PhysicsComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Direction { get; set; }

        public PhysicsComponent(
            Vector2 position = default(Vector2),
            Vector2 velocity = default(Vector2),
            Vector2 direction = default(Vector2))
        {
            Position = position;
            Velocity = velocity;
            Direction = direction;
        }

        ~PhysicsComponent() {}
    }
}