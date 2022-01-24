using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoGame
{
    public class RaindropComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Point2 ImpactPoint { get; set; } // Attractor
        public float Size { get; set; }
    }
}