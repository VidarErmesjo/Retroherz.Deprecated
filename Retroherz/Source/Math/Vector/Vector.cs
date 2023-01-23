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
		public static implicit operator Point2(Vector vector) => new(vector.X, vector.Y);
		public static implicit operator Vector(Point2 point) => new(point.X, point.Y);
		public static implicit operator Vector(Size2 size) => new(size.Width, size.Height);
		public static implicit operator Vector(Vector2 vector) => new(vector.X, vector.Y);
		public static implicit operator Vector2(Vector vector) => new(vector.X, vector.Y);
	
	#endregion

	#region Properties

		public static Vector Zero => zeroVector;
		public static Vector One => unitVector;
		public static Vector UnitX => unitXVector;
		public static Vector UnitY => unitYVector;

	#endregion

	#region Public Methods

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
		public static Vector Abs(Vector vector) => new(MathF.Abs(vector.X), MathF.Abs(vector.Y));

		/// <summary>
		/// Returns the angle between vectors u and v in radians.
		/// </summary>    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Angle(Vector u, Vector v) => (MathF.Atan2(v.Y - u.Y, v.X - u.X));

		/// <summary>
		/// Returns a clamped copy of the <see cref="Vector"/>.
		/// </summary> 
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector Clamp(Vector value, Vector min, Vector max) => new(
			MathHelper.Clamp(value.X, min.X, max.X),
			MathHelper.Clamp(value.Y, min.Y, max.Y)
		);

		/// <summary>
		/// Returns the cross product of vectors u and v.
		/// </summary>    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cross(Vector u, Vector v) => (u.X * v.Y - u.Y * v.X);

		/// <summary>
		/// Returns the distance between vectors u and v.
		/// </summary> 
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Distance(Vector u, Vector v)
		{
			float dx = u.X - v.X;
			float dy = u.Y - v.Y;
			return MathF.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>
		/// Returns the squared distance between vectors u and v.
		/// </summary> 
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DistanceSquared(Vector u, Vector v) => DistanceSquared(ref u, ref v);

		/// <summary>
		/// Returns the squared distance between vectors u and v.
		/// </summary> 
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float DistanceSquared(ref Vector u, ref Vector v)
		{
			float dx = u.X - v.X;
			float dy = u.Y - v.Y;
			return dx * dx + dy * dy;
		}

		/// <summary>
		/// Returns the dot product of vectors u and v.
		/// </summary>    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Dot(Vector u, Vector v) => (u.X * v.X + u.Y * v.Y);

		/// <summary>
		/// Returns a slightly shortened version of the vector.
		/// </summary>      
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector Interpolate(Vector u, Vector v, float value) => new(u * (1 - value) + v * value);

		/// <summary>
		/// Returns the intersection point of the line p1-p2 with p3-p4.
		/// </summary>        
		public static Vector Intersection(Vector p1, Vector p2, Vector p3, Vector p4)
		{
			// From http://paulbourke.net/geometry/lineline2d/
			float s = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X))
					/ ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));
			return new(p1.X + s * (p2.X - p1.X), p1.Y + s * (p2.Y - p1.Y));
		}

		/// <summary>
		/// Returns true if the point is 'left' of the line p1-p2.
		/// </summary>        
		public static bool LeftOf(Vector p1, Vector p2, Vector point) => (Cross(p2 - p1, point - p1) < 0);

		/// <summary>
		/// Returns the longest of vectors u and v.
		/// </summary>    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector Max(Vector u, Vector v) => new(MathF.Max(u.X, v.X), MathF.Max(u.Y, v.Y));

		/// <summary>
		/// Returns the shortest of vectors u and v.
		/// </summary>    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector Mix(Vector u, Vector v) => new(MathF.Min(u.X, v.X), MathF.Min(u.Y, v.Y));

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
		public static Vector Round(Vector vector) => new(MathF.Round(vector.X), MathF.Round(vector.Y));

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
		public override int GetHashCode() => (X, Y).GetHashCode();

		/// <summary>
		/// Returns the vector information.
		/// </summary>
		public override string ToString() => "{" + $"X:{X} Y:{Y}" + "}";

	#endregion
}