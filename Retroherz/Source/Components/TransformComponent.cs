using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Components
{
    public class TransformComponent
    {
        private readonly Transform2 _transform;

        public Vector2 Position { get => _transform.Position; set => _transform.Position = value; }
        public float Rotation { get => _transform.Rotation; set => _transform.Rotation = value; }
        public Vector2 Scale { get => _transform.Scale; set => _transform.Scale = value; }

        public TransformComponent(
            Vector2 position = default(Vector2),
            float rotation = 0.0f,
            Vector2 scale = default(Vector2))
        {
            _transform = new Transform2(position, rotation, scale);
        }

        ~TransformComponent() {}

        public override string ToString() => _transform.ToString();
    }
}