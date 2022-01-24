using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoGame
{
    public class RainfallComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Point2 ImpactPoint { get; set; }
        public float Size { get; set; }

        public RainfallComponent(Point2 impactPoint = default(Point2), Vector2 position = default(Vector2), Vector2 velocity = default(Vector2), float size = 1f)
        {
            Position = position;
            Velocity = velocity;
            ImpactPoint = ImpactPoint;
            Size = size;
        }        
    }
}