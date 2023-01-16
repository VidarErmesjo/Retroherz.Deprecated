using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MathD = System.Math;

namespace Retroherz.Math;

/// <summary>
/// Double precision vector.
/// </summary>
public struct DoubleVector : IEquatable<DoubleVector>
{
	public double X;
	public double Y;

	// Constructors
	public DoubleVector(int x = default(int), int y = default(int)) { X = x; Y = y; }
	public DoubleVector(float x = default(float), float y = default(float)) { X = x; Y = y; }
	public DoubleVector(double x = default(double), double y = default(double)) { X = x; Y = y; }
	public DoubleVector(DoubleVector vector = default(DoubleVector)) { X = vector.X; Y = vector.Y; }

	// Negation
	public static DoubleVector operator -(DoubleVector vector) => new(-vector.X, -vector.Y);

	// Addiction
	public static DoubleVector operator +(DoubleVector u, DoubleVector v) => new(u.X + v.X, u.Y + v.Y);
	public static DoubleVector operator +(Vector u, DoubleVector v) => new(u.X + v.X, u.Y + v.Y);
	public static DoubleVector operator +(DoubleVector u, Vector v) => new(u.X + v.X, u.Y + v.Y);
	public static DoubleVector operator +(Vector2 u, DoubleVector v) => new(u.X + v.X, u.Y + v.Y);
	public static DoubleVector operator +(DoubleVector u, Vector2 v) => new(u.X + v.X, u.Y + v.Y);
	public static DoubleVector operator +(DoubleVector vector, int value) => new(vector.X + value, vector.Y + value);
	public static DoubleVector operator +(DoubleVector vector, float value) => new(vector.X + value, vector.Y + value);
	public static DoubleVector operator +(DoubleVector vector, double value) => new(vector.X + value, vector.Y + value);
	public static DoubleVector operator +(int value, DoubleVector vector) => new(value + vector.X, value + vector.Y);
	public static DoubleVector operator +(float value, DoubleVector vector) => new(value + vector.X, value + vector.Y);
	public static DoubleVector operator +(double value, DoubleVector vector) => new(value + vector.X, value + vector.Y);

	// Subtraction
	public static DoubleVector operator -(DoubleVector u, DoubleVector v) => new(u.X - v.X, u.Y - v.Y);
	public static DoubleVector operator -(Vector u, DoubleVector v) => new(u.X - v.X, u.Y - v.Y);
	public static DoubleVector operator -(DoubleVector u, Vector v) => new(u.X - v.X, u.Y - v.Y);
	public static DoubleVector operator -(Vector2 u, DoubleVector v) => new(u.X - v.X, u.Y - v.Y);
	public static DoubleVector operator -(DoubleVector u, Vector2 v) => new(u.X - v.X, u.Y - v.Y);
	public static DoubleVector operator -(DoubleVector vector, int value) => new(vector.X - value, vector.Y - value);
	public static DoubleVector operator -(DoubleVector vector, float value) => new(vector.X - value, vector.Y - value);
	public static DoubleVector operator -(DoubleVector vector, double value) => new(vector.X - value, vector.Y - value);
	public static DoubleVector operator -(int value, DoubleVector vector) => new(value - vector.X, value - vector.Y);
	public static DoubleVector operator -(float value, DoubleVector vector) => new(value - vector.X, value - vector.Y);
	public static DoubleVector operator -(double value, DoubleVector vector) => new(value - vector.X, value - vector.Y);

	// Multiplication
	public static DoubleVector operator *(DoubleVector u, DoubleVector v) => new(u.X * v.X, u.Y * v.Y);
	public static DoubleVector operator *(Vector u, DoubleVector v) => new(u.X * v.X, u.Y * v.Y);
	public static DoubleVector operator *(DoubleVector u, Vector v) => new(u.X * v.X, u.Y * v.Y);
	public static DoubleVector operator *(Vector2 u, DoubleVector v) => new(u.X * v.X, u.Y * v.Y);
	public static DoubleVector operator *(DoubleVector u, Vector2 v) => new(u.X * v.X, u.Y * v.Y);
	public static DoubleVector operator *(DoubleVector vector, int value) => new(vector.X * value, vector.Y * value);
	public static DoubleVector operator *(DoubleVector vector, float value) => new(vector.X * value, vector.Y * value);
	public static DoubleVector operator *(DoubleVector vector, double value) => new(vector.X * value, vector.Y * value);
	public static DoubleVector operator *(int value, DoubleVector vector) => new(value * vector.X, value * vector.Y);
	public static DoubleVector operator *(float value, DoubleVector vector) => new(value * vector.X, value * vector.Y);
	public static DoubleVector operator *(double value, DoubleVector vector) => new(value * vector.X, value * vector.Y);

	// Divisiion
	public static DoubleVector operator /(DoubleVector u, DoubleVector v) => new(u.X / v.X, u.Y / v.Y);
	public static DoubleVector operator /(Vector u, DoubleVector v) => new(u.X / v.X, u.Y / v.Y);
	public static DoubleVector operator /(DoubleVector u, Vector v) => new(u.X / v.X, u.Y / v.Y);
	public static DoubleVector operator /(Vector2 u, DoubleVector v) => new(u.X / v.X, u.Y / v.Y);
	public static DoubleVector operator /(DoubleVector u, Vector2 v) => new(u.X / v.X, u.Y / v.Y);
	public static DoubleVector operator /(DoubleVector vector, int value) => new(vector.X / value, vector.Y / value);
	public static DoubleVector operator /(DoubleVector vector, float value) => new(vector.X / value, vector.Y / value);
	public static DoubleVector operator /(DoubleVector vector, double value) => new(vector.X / value, vector.Y / value);
	public static DoubleVector operator /(int value, DoubleVector vector) => new(value / vector.X, value / vector.Y);
	public static DoubleVector operator /(float value, DoubleVector vector) => new(value / vector.X, value / vector.Y);
	public static DoubleVector operator /(double value, DoubleVector vector) => new(value / vector.X, value / vector.Y);

	// Modulus
	public static DoubleVector operator %(DoubleVector u, DoubleVector v) => new(u.X % v.X, u.Y % v.Y);
	public static DoubleVector operator %(Vector u, DoubleVector v) => new(u.X % v.X, u.Y % v.Y);
	public static DoubleVector operator %(DoubleVector u, Vector v) => new(u.X % v.X, u.Y % v.Y);
	public static DoubleVector operator %(Vector2 u, DoubleVector v) => new(u.X % v.X, u.Y % v.Y);
	public static DoubleVector operator %(DoubleVector u, Vector2 v) => new(u.X % v.X, u.Y % v.Y);
	public static DoubleVector operator %(DoubleVector vector, int value) => new(vector.X % value, vector.Y % value);
	public static DoubleVector operator %(DoubleVector vector, float value) => new(vector.X % value, vector.Y % value);
	public static DoubleVector operator %(DoubleVector vector, double value) => new(vector.X % value, vector.Y % value);
	public static DoubleVector operator %(int value, DoubleVector vector) => new(value % vector.X, value % vector.Y);
	public static DoubleVector operator %(float value, DoubleVector vector) => new(value % vector.X, value % vector.Y);
	public static DoubleVector operator %(double value, DoubleVector vector) => new(value % vector.X, value % vector.Y);

	// Comparators
	public static bool operator ==(DoubleVector u, DoubleVector v) => u.Equals(v);
	public static bool operator !=(DoubleVector u, DoubleVector v) => !(u == v);
	public static bool operator <(DoubleVector u, DoubleVector v) => u.X < v.X && u.Y < v.Y;
	public static bool operator >(DoubleVector u, DoubleVector v) => u.X > v.X && u.Y > v.Y;
	public static bool operator <=(DoubleVector u, DoubleVector v) => u.X <= v.X && u.Y <= v.Y;
	public static bool operator >=(DoubleVector u, DoubleVector v) => u.X >= v.X && u.Y >= v.Y;

	// Implicit conversion (MonoGame)
	public static implicit operator Size2(DoubleVector vector) => new(((float)vector.X), ((float)vector.Y));
	public static implicit operator Point2(DoubleVector vector) => new(((float)vector.X), ((float)vector.Y));
	public static implicit operator DoubleVector(Point2 point) => new(point.X, point.Y);
	public static implicit operator DoubleVector(Size2 size) => new(size.Width, size.Height);
	public static implicit operator Vector2(DoubleVector vector) => new(((float)vector.X), ((float)vector.Y));
	public static implicit operator DoubleVector(Vector2 vector) => new(vector.X, vector.Y);

	// Helpers
	public static DoubleVector Zero => new(0, 0);
	public static DoubleVector One => new(1, 1);
	public static DoubleVector UnitX => new(1, 0);
	public static DoubleVector UnitY => new(0, 1);

	// Methods

	/// <summary>
	/// Normalizes the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Normalize()
	{
		double magnitude = (1 / this.Magnitude());
		this.X *= magnitude;
		this.Y *= magnitude;
	}

	// Static methods
	/// <summary>
	/// Returns the angle between vectors u and v in radians.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Angle(DoubleVector u, DoubleVector v) => (MathD.Atan2(v.Y - u.Y, v.X - u.X));

	/// <summary>
	/// Returns a clamped copy of the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Clamp(DoubleVector value, DoubleVector min, DoubleVector max) => new(
		MathD.Clamp(value.X, min.X, max.X),
		MathD.Clamp(value.Y, min.Y, max.Y)
	);

	/// <summary>
	/// Returns the cross product of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Cross(DoubleVector u, DoubleVector v) => (u.X * v.Y - u.Y * v.X);

	/// <summary>
	/// Returns the distance between vectors u and v.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Distance(DoubleVector u, DoubleVector v)
	{
		double dx = u.X - v.X;
		double dy = u.Y - v.Y;
		return MathD.Sqrt(dx * dx + dy * dy);
	}

	/// <summary>
	/// Returns the dot product of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Dot(DoubleVector u, DoubleVector v) => (u.X * v.X + u.Y * v.Y);

	/// <summary>
	/// Returns a slightly shortened version of the vector.
	/// </summary>      
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Interpolate(DoubleVector u, DoubleVector v, double value) => new(u * (1 - value) + v * value);

	/// <summary>
	/// Returns the intersection point of the line p1-p2 with p3-p4.
	/// </summary>        
	public static DoubleVector Intersection(DoubleVector p1, DoubleVector p2, DoubleVector p3, DoubleVector p4)
	{
		// From http://paulbourke.net/geometry/lineline2d/
		double s = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X))
				/ ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));
		return new(p1.X + s * (p2.X - p1.X), p1.Y + s * (p2.Y - p1.Y));
	}

	/// <summary>
	/// Returns true if the point is 'left' of the line p1-p2.
	/// </summary>        
	public static bool LeftOf(DoubleVector p1, DoubleVector p2, DoubleVector point) => (Cross(p2 - p1, point - p1) < 0);

	/// <summary>
	/// Returns the longest of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Max(DoubleVector u, DoubleVector v) => new(MathD.Max(u.X, v.X), MathD.Max(u.Y, v.Y));

	/// <summary>
	/// Returns the shortest of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Mix(DoubleVector u, DoubleVector v) => new(MathD.Min(u.X, v.X), MathD.Min(u.Y, v.Y));

	/// <summary>
	/// Returns a random unit vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Random()
	{
		Vector2 random;
		System.Random.Shared.NextUnitVector(out random);

		return random;		
	}

	/// <summary>
	/// Rounds the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Round(DoubleVector vector) => new(MathD.Round(vector.X), MathD.Round(vector.Y));

	// Implementations
    /// <summary>
    /// Returns true if the vector equals the other vector.
    /// </summary>
	public bool Equals(DoubleVector other) => (
		(X == other.X) &&
		(Y == other.Y)
	);

	// Overrides
    /// <summary>
    /// Returns true if the vector equals the object.
    /// </summary>
	public override bool Equals(object obj) => (
		(obj is DoubleVector other) &&
		(other.X, other.Y).Equals((X, Y))
	);

    /// <summary>
    /// Returns the HashCode of the vector.
    /// </summary>
	public override int GetHashCode() => (X, Y).GetHashCode();

    /// <summary>
    /// Returns the vector information.
    /// </summary>
	public override string ToString() => "{" + $"X:{X} Y:{Y}" + "}";
}