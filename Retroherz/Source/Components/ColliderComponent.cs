using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using Retroherz.Math;

namespace Retroherz.Components;

public enum ColliderComponentType
{
	Static,
	Dynamic,
	Border
}

public enum ResolveCollision
{
	Stop,
	Slide,
	Bounce
}

public enum Direction
{
	Up = 0,
	Right = 1,
	Down = 2,
	Left = 3
}

public enum ContactType
{
	Collision,
	Constraint
}

public struct Constraint
{
	public int Id;
	public Vector ContactPoint;
	public Vector ContactNormal;
	public double ContactTime;
	public (ColliderComponent collider, TransformComponent transform) Obstacle;

	public Constraint(
		int id,
		(ColliderComponent, TransformComponent) obstacle,
		Vector contactPoint,
		Vector contactNormal,
		double contactTime)
	{
		Id = id;
		Obstacle = obstacle;
		ContactPoint = contactPoint;
		ContactNormal = contactNormal;
		ContactTime = contactTime;
	}
}

// Arch
// readonly?
/*public struct Collider : IScalable
{
	public Vector Velocity { get; set; }
	public Vector Scale { get; set; }
	public Vector Origin { get => Scale / 2; }

	public Collider(Vector velocity, Vector scale) { Velocity = velocity; Scale = scale; }

	public override string ToString()  => "[" + $"Velocity:{Velocity} Scale:{Scale} Origin:{Origin}" + "]";
}*/

public class Collider // Constructorless
{
	public required Vector Velocity { get; set; }
}

public class ColliderComponent
{
	private Vector _size = default(Vector);
	private Vector _previousSize = default(Vector);

	public readonly Bag<Constraint> Constraints = new();
	public readonly Bag<(int Id, Vector ContactPoint, Vector ContactNormal, double ContactTime)> Contacts = new();

	public (Vector Position, Vector Direction)[] Rays = new (Vector, Vector)[4];

	public Vector Velocity { get; set; }
	public Vector Origin { get => Size / 2; }

	public Vector Size
	{
		get => _size;
		set
		{
			_previousSize = _size;
			_size = Vector.Clamp(value, Vector.One, value);
 		}
	}

	public Vector DeltaSize
	{
		get => _size - _previousSize;
		private set => _previousSize = Vector.Clamp(value, Vector.One, value);
	}

	public Vector DeltaOrigin { get => DeltaSize / 2; }

	public ColliderComponentType Type { get; set; }

	public ColliderComponent(
		Vector velocity = default(Vector),
		Vector size = default(Vector),
		ColliderComponentType type = default(ColliderComponentType
	))
	{
		Velocity = velocity;
		Size = size;
		DeltaSize = size;
		Type = type;
	}

	~ColliderComponent() {}
}