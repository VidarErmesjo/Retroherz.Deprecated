using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Components
{
    public class TransformComponent : IMovable
    {
		public Vector2 Position { get; set; }

        public TransformComponent(Vector2 position)
        {
			Position = position;
        }

		public TransformComponent(float x, float y)
        {
            Position = new(x, y);
        }

        ~TransformComponent() {}

        public override string ToString() => "{" + string.Format("X:{0} Y:{1}", Position.X, Position.Y) + "}";
    }
}