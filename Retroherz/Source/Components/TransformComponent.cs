using Microsoft.Xna.Framework;
using MonoGame.Extended;

using Retroherz.Math;

namespace Retroherz.Components;

// Arch
public struct Transform// : IMovable, IRotatable
{
	public Vector Position { get; set; }
	public float Rotation { get; set; }

	public Transform(Vector position = default(Vector)) => Position = position;
	public Transform(float x, float y) => Position = new(x, y);

	public override string ToString()  => "[" + $"Position:{Position} Rotation:{Rotation}" + "]";
}

public class TransformComponent// : IMovable
{
	public Vector Position { get; set; }

	public TransformComponent(Vector position = default(Vector)) => Position = position;
	public TransformComponent(float x, float y) => Position = new(x, y);

	~TransformComponent() {}

	public override string ToString() => "{" + string.Format("X:{0} Y:{1}", Position.X, Position.Y) + "}";
}