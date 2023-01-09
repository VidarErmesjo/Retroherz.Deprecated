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

public struct Ray
{
	public Vector Position;
	public Vector Direction;

	public Ray(Vector position, Vector direction) { Position = position; Direction = direction; }

	public override string ToString()  => "[" + $"Position:{Position} Direction:{Direction}" + "]";
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

public struct Contact
{
	public int Id;
	public Vector ContactPoint;
	public Vector ContactNormal;
	public double ContactTime;

	public Contact(
		int id,
		Vector contactPoint,
		Vector contactNormal,
		double contactTime)
	{
		Id = id;
		ContactPoint = contactPoint;
		ContactNormal = contactNormal;
		ContactTime = contactTime;
	}
}

// Arch
// readonly?
public struct Collider// : IScalable
{
	private readonly Dictionary<int, Contact> _contacts = new();
	public Vector Velocity { get; set; }
	public Vector Scale { get; set; }
	public Vector Origin { get => Scale / 2; }

	public void Add(int id, Contact contact) => _contacts.Add(id, contact);
	public void Clear() => _contacts.Clear();

	public Collider(Vector velocity, Vector scale) { Velocity = velocity; Scale = scale; }

	public override string ToString()  => "[" + $"Velocity:{Velocity} Scale:{Scale} Origin:{Origin}" + "]";
}

public class ColliderComponent
{
	private Vector _size = default(Vector);
	private Vector _deltaSize = default(Vector);

	//public readonly Dictionary<int, (Vector ContactPoint, Vector ContactNormal, float ContactTime)> Contacts = new();//_contacts = new();
	public readonly Bag<Constraint> Constraints = new();
	public readonly Bag<Contact> Contacts = new();//_contacts = new();

	public (Vector Position, Vector Direction)[] Rays = new (Vector, Vector)[4];

	public Vector Velocity { get; set; }
	public Vector Size
	{
		get => _size;
		set
		{
			_deltaSize = _size;
			_size = Vector.Clamp(value, Vector.One, value);
 		}
	}

	public Vector DeltaSize
	{
		get => _size - _deltaSize;
		private set => _deltaSize = Vector2.Clamp(value, Vector.One, value);
	}

	public Vector Origin { get => Size / 2; }
	public Vector DeltaOrigin { get => DeltaSize / 2; }

	public ColliderComponentType Type { get; set; }

	//public void Add(int id, (Vector, Vector, float) contact) => _contacts.Add(id, contact);
	//public void Clear() => _contacts.Clear();
	//public bool Contains(int id) => _contacts.ContainsKey(id);

	/*public Span<(int ID, Vector ContactPoint, Vector ContactNormal, float ContactTime)> Contacts
	{
		get
		{
			// vs new Span<Contact>() ??
			var items = _contacts.Values.ToArray().AsSpan();
			items.Sort((a, b) => a.ContactTime < b.ContactTime ? -1 : 1);
			items.BinarySearch((x) => x);

			return (4, items);
		}
	}*/

	public ColliderComponent(
		Vector velocity = default(Vector),
		Vector size = default(Vector),
		ColliderComponentType type = default(ColliderComponentType))
	{
		Velocity = velocity;
		Size = size;
		DeltaSize = size;
		Type = type;
	}

	~ColliderComponent() {}
}