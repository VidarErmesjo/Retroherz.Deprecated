using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Math;

/// <summary>
/// Describes a single precision vector in two dimensions.
/// </summary>
#if XNADESIGNPROVIDED
    [System.ComponentModel.TypeConverter(typeof(Microsoft.Xna.Framework.Design.Vector2TypeConverter))]
#endif
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Vector : IEquatable<Vector>
{
	#region Private Fields
	private static readonly Vector zeroVector = new(0, 0);
	private static readonly Vector unitVector = new(1, 1);
	private static readonly Vector unitXVector = new(1, 0);
	private static readonly Vector unitYVector = new(0, 1);

	#endregion

	#region Public Fields
	public float X;
	public float Y;

	#endregion

	#region Constructors
	public Vector(int x = default(int), int y = default(int)) { X = x; Y = y; }
	public Vector(float x = default(float), float y = default(float)) { X = x; Y = y; }
	public Vector(double x = default(double), double y = default(double)) { X = ((float)x); Y = ((float)y); }
	public Vector(Vector vector = default(Vector)) { X = vector.X; Y = vector.Y; }

	#endregion

	#region Operators
	// Negation
	public static Vector operator -(Vector vector) => new(-vector.X, -vector.Y);

	// Addiction
	public static Vector operator +(Vector u, Vector v) => new(u.X + v.X, u.Y + v.Y);
	public static Vector operator +(Vector2 u, Vector v) => new(u.X + v.X, u.Y + v.Y);
	public static Vector operator +(Vector u, Vector2 v) => new(u.X + v.X, u.Y + v.Y);
	public static Vector operator +(Vector u, Point2 point) => new(u.X + point.X, u.Y + point.Y);
	public static Vector operator +(Point2 point, Vector u) => new(point.X + u.X, point.Y + u.Y);
	public static Vector operator +(Vector vector, int value) => new(vector.X + value, vector.Y + value);
	public static Vector operator +(Vector vector, float value) => new(vector.X + value, vector.Y + value);
	public static Vector operator +(Vector vector, double value) => new(vector.X + ((float)value),vector.Y + ((float)value));
	public static Vector operator +(int value, Vector vector) => new(value + vector.X, value + vector.Y);
	public static Vector operator +(float value, Vector vector) => new(value + vector.X, value + vector.Y);
	public static Vector operator +(double value, Vector vector) => new(
		((float)value) + vector.X,
		((float)value) + vector.Y
	);

	// Subtraction
	public static Vector operator -(Vector u, Vector v) => new(u.X - v.X, u.Y - v.Y);
	public static Vector operator -(Vector2 u, Vector v) => new(u.X - v.X, u.Y - v.Y);
	public static Vector operator -(Vector u, Vector2 v) => new(u.X - v.X, u.Y - v.Y);
	public static Vector operator -(Vector u, Point2 point) => new(u.X - point.X, u.Y - point.Y);
	public static Vector operator -(Point2 point, Vector u) => new(point.X - u.X, point.Y - u.Y);
	public static Vector operator -(Vector vector, int value) => new(vector.X - value, vector.Y - value);
	public static Vector operator -(Vector vector, float value) => new(vector.X - value, vector.Y - value);
	public static Vector operator -(Vector vector, double value) => new(vector.X - ((float)value), vector.Y - ((float)value));
	public static Vector operator -(int value, Vector vector) => new(value - vector.X, value - vector.Y);
	public static Vector operator -(float value, Vector vector) => new(value - vector.X, value - vector.Y);
	public static Vector operator -(double value, Vector vector) => new(((float)value) - vector.X, ((float)value) - vector.Y);

	// Multiplication
	public static Vector operator *(Vector u, Vector v) => new(u.X * v.X, u.Y * v.Y);
	public static Vector operator *(Vector2 u, Vector v) => new(u.X * v.X, u.Y * v.Y);
	public static Vector operator *(Vector u, Vector2 v) => new(u.X * v.X, u.Y * v.Y);
	public static Vector operator *(Vector vector, int value) => new(vector.X * value, vector.Y * value);
	public static Vector operator *(Vector vector, float value) => new(vector.X * value, vector.Y * value);
	public static Vector operator *(Vector vector, double value) => new(vector.X * ((float)value), vector.Y * ((float)value));
	public static Vector operator *(int value, Vector vector) => new(value * vector.X, value * vector.Y);
	public static Vector operator *(float value, Vector vector) => new(value * vector.X, value * vector.Y);
	public static Vector operator *(double value, Vector vector) => new(((float)value) * vector.X, ((float)value) * vector.Y);

	// Divisiion
	public static Vector operator /(Vector u, Vector v) => new(u.X / v.X, u.Y / v.Y);
	public static Vector operator /(Vector2 u, Vector v) => new(u.X / v.X, u.Y / v.Y);
	public static Vector operator /(Vector u, Vector2 v) => new(u.X / v.X, u.Y / v.Y);
	public static Vector operator /(Vector vector, int value) => new(vector.X / value, vector.Y / value);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector operator /(Vector vector, float value) => new(vector.X / value, vector.Y / value);
	public static Vector operator /(Vector vector, double value) => new(vector.X / ((float)value), vector.Y / ((float)value));
	public static Vector operator /(int value, Vector vector) => new(value / vector.X, value / vector.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector operator /(float value, Vector vector) => new(value / vector.X, value / vector.Y);
	public static Vector operator /(double value, Vector vector) => new(((float)value) / vector.X, ((float)value) / vector.Y);

	// Modulus
	public static Vector operator %(Vector u, Vector v) => new(u.X % v.X, u.Y % v.Y);
	public static Vector operator %(Vector2 u, Vector v) => new(u.X % v.X, u.Y % v.Y);
	public static Vector operator %(Vector u, Vector2 v) => new(u.X % v.X, u.Y % v.Y);
	public static Vector operator %(Vector vector, int value) => new(vector.X % value, vector.Y % value);
	public static Vector operator %(Vector vector, float value) => new(vector.X % value, vector.Y % value);
	public static Vector operator %(Vector vector, double value) => new(vector.X % ((float)value), vector.Y % ((float)value));
	public static Vector operator %(int value, Vector vector) => new(value % vector.X, value % vector.Y);
	public static Vector operator %(float value, Vector vector) => new(value % vector.X, value % vector.Y);
	public static Vector operator %(double value, Vector vector) => new(((float)value) % vector.X, ((float)value) % vector.Y);

	// Comparators
	public static bool operator ==(Vector u, Vector v) => u.Equals(v);
	public static bool operator !=(Vector u, Vector v) => !(u == v);
	public static bool operator <(Vector u, Vector v) => u.X < v.X && u.Y < v.Y;
	public static bool operator <(Vector vector, float value) => vector.X < value && vector.Y < value;
	public static bool operator <(float value, Vector vector) => value < vector.X && value < vector.Y;
	public static bool operator >(Vector u, Vector v) => u.X > v.X && u.Y > v.Y;
	public static bool operator >(Vector vector, float value) => vector.X > value && vector.Y > value;
	public static bool operator >(float value, Vector vector) => value > vector.X && value > vector.Y;
	public static bool operator <=(Vector u, Vector v) => u.X <= v.X && u.Y <= v.Y;
	public static bool operator >=(Vector u, Vector v) => u.X >= v.X && u.Y >= v.Y;

	// Implicit conversion (MonoGame)
	public static implicit operator Size2(Vector vector) => new(vector.X, vector.Y);
	public static implicit operator Point(Vector vector) => new(((int)vector.X), ((int)vector.Y));
	public static implicit operator Point2(Vector vector) => new(vector.X, vector.Y);
	public static implicit operator Vector(Point point) => new(point.X, point.Y);
	public static implicit operator Vector(Point2 point) => new(point.X, point.Y);
	public static implicit operator Vector(Size2 size) => new(size.Width, size.Height);
	public static implicit operator Vector(Vector2 vector) => new(vector.X, vector.Y);
	public static implicit operator Vector2(Vector vector) => new(vector.X, vector.Y);
	public static implicit operator (float X, float Y)(Vector vector) => new(vector.X, vector.Y);
	public static implicit operator Vector((float X, float Y) tuple) => new(tuple.X, tuple.Y);

	#endregion

	#region Properties
	public static Vector Zero => zeroVector;
	public static Vector One => unitVector;
	public static Vector UnitX => unitXVector;
	public static Vector UnitY => unitYVector;

	#endregion

	#region Public Methods
	/// <summary>
	/// Returns the angle between this and vector v in radians.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Angle(in Vector v) => MathF.Atan2(v.Y - this.Y, v.X - this.X);

	/// <summary>
	/// Returns the magnitude (length) of the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Magnitude() => MathF.Sqrt(this.X * this.X + this.Y * this.Y);

	/// <summary>
	/// Normalizes the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Normalize()
	{
		float magnitude = (1 / this.Magnitude());
		this.X *= magnitude;
		this.Y *= magnitude;
	}

	// Static methods
	/// <summary>
	/// Returns an absolute valued copy of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Abs(in Vector vector) => new(MathF.Abs(vector.X), MathF.Abs(vector.Y));

	/// <summary>
	/// Returns the angle between vectors u and v in radians.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Angle(in Vector u, in Vector v) => MathF.Atan2(v.Y - u.Y, v.X - u.X);

	/// <summary>
	/// Returns a clamped copy of the <see cref="Vector"/>.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Clamp(in Vector value, in Vector min, in Vector max) => new(
		MathHelper.Clamp(value.X, min.X, max.X),
		MathHelper.Clamp(value.Y, min.Y, max.Y)
	);

	/// <summary>
	/// Returns the cross product of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Cross(in Vector u, in Vector v) => (u.X * v.Y - u.Y * v.X);

	/// <summary>
	/// Returns the distance between vectors u and v.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(in Vector u, in Vector v)
	{
		float dx = u.X - v.X;
		float dy = u.Y - v.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	/// <summary>
	/// Returns the squared distance between vectors u and v.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(in Vector u, in Vector v)
	{
		float dx = u.X - v.X;
		float dy = u.Y - v.Y;
		return dx * dx + dy * dy;
	}

	/// <summary>
	/// Returns the dot product of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dot(in Vector u, in Vector v) => (u.X * v.X + u.Y * v.Y);

	/// <summary>
	/// Returns the hypothenuse of the vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Hypothenuse(in Vector u, in Vector v) => Dot(u - v, u - v);

	/// <summary>
	/// Returns a slightly shortened version of the vector.
	/// </summary>      
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Interpolate(in Vector u, in Vector v, in float value) => new(u * (1 - value) + v * value);

	/// <summary>
	/// Returns the intersection point of the line p1-p2 with p3-p4.
	/// </summary>        
	public static Vector Intersection(in Vector p1, in Vector p2, in Vector p3, in Vector p4)
	{
		// From http://paulbourke.net/geometry/lineline2d/
		float s = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X))
				/ ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));

		return new Vector(p1.X + s * (p2.X - p1.X), p1.Y + s * (p2.Y - p1.Y));
	}

	/// <summary>
	/// Returns true if the point is 'left' of the line p1-p2.
	/// </summary>        
	public static bool LeftOf(in Vector p1, in Vector p2, in Vector point) => Cross(p2 - p1, point - p1) < 0;

	/// <summary>
	/// Returns the longest of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Max(in Vector u, in Vector v) => new(MathF.Max(u.X, v.X), MathF.Max(u.Y, v.Y));

	/// <summary>
	/// Returns the shortest of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Mix(in Vector u, in Vector v) => new(MathF.Min(u.X, v.X), MathF.Min(u.Y, v.Y));

	/// <summary>
	///	Returns the product from projecting vector u onto vector v.
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Project(in Vector u, in Vector v)
	{
		float k = Dot(u, v) / Dot(v, v);
		return new Vector(k * v.X, k * v.Y);
	}

	/// <summary>
	/// Returns a random unit vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Random()
	{
		Vector2 random;
		System.Random.Shared.NextUnitVector(out random);

		return random;		
	}

	/// <summary>
	/// Rounds the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Round(in Vector vector) => new(MathF.Round(vector.X), MathF.Round(vector.Y));

	// Implementations
	/// <summary>
	/// Returns true if the vector equals the other vector.
	/// </summary>
	public bool Equals(Vector other) => (
		(X == other.X) &&
		(Y == other.Y)
	);

	// Overrides
	/// <summary>
	/// Returns true if the vector equals the object.
	/// </summary>
	public override bool Equals(object obj) => (
		(obj is Vector other) &&
		(other.X, other.Y).Equals((X, Y))
	);

	/// <summary>
	/// Returns the HashCode of the vector.
	/// </summary>
	/// <returns>
	///	A 32-bit signed integer that is the hash code for this instance.
	///	</returns>
	public override int GetHashCode() => (X, Y).GetHashCode();

	/// <summary>
	/// Returns the vector information.
	/// </summary>
	public override string ToString() => "{" + $"X:{X} Y:{Y}" + "}";

	#endregion
}