using System;
namespace Retroherz.Math;

/// <summary>
/// Double precision ray.
/// </summary>
public struct Ray : IEquatable<Ray>
{
	public Vector Origin;
	public Vector Direction;

	public Ray(Vector origin = default(Vector), Vector direction = default(Vector))
	{
		Direction = direction;
		Origin = origin;	
	}

	// Comparators
	public static bool operator ==(Ray u, Ray v) => u.Equals(v);
	public static bool operator !=(Ray u, Ray v) => !(u == v);

	// Implementations
	public bool Equals(Ray other) => (
		(this.Origin.X == other.Origin.X) && 
		(this.Origin.Y == other.Origin.Y) && 
		(this.Direction.X == other.Direction.X) && 
		(this.Direction.Y == other.Direction.Y)
	);

	// Overrides
	public override bool Equals(object obj) => (
		(obj is Ray other) &&
		(other.Origin, other.Direction).Equals((this.Origin, this.Direction))
	);

	public override int GetHashCode() => (this.Origin, this.Direction).GetHashCode();
	
	public override string ToString() => "{" + $"Origins:{this.Origin} Direction:{this.Direction}"+ "}";
}