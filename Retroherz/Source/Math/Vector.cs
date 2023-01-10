using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MathD = System.Math;

namespace Retroherz.Math;

public struct Vector : IEquatable<object>
{
	public double X;
	public double Y;
	public Vector() { this.X = 0; this.Y = 0; }
	public Vector(int x, int y) { this.X = x; this.Y = y; }
	public Vector(float x, float y) { this.X = x; this.Y = y; }
	public Vector(double x, double y) { this.X = x; this.Y = y; }

	// Negation
	public static Vector operator -(Vector vector) => new(-vector.X, -vector.Y);

	// Addiction
	public static Vector operator +(Vector u, Vector v) => new(u.X + v.X, u.Y + v.Y);
	public static Vector operator +(Vector u, Vector2 v) => new(u.X + v.X, u.Y + v.Y);
	public static Vector operator +(Vector2 u, Vector v) => new(u.X + v.X, u.Y + v.Y);
	public static Vector operator +(Vector vector, int value) => new(vector.X + value, vector.Y + value);
	public static Vector operator +(Vector vector, float value) => new(vector.X + value, vector.Y + value);
	public static Vector operator +(Vector vector, double value) => new(vector.X + value, vector.Y + value);
	public static Vector operator +(int value, Vector vector) => new(value + vector.X, value + vector.Y);
	public static Vector operator +(float value, Vector vector) => new(value + vector.X, value + vector.Y);
	public static Vector operator +(double value, Vector vector) => new(value + vector.X, value + vector.Y);

	// Subtraction
	public static Vector operator -(Vector u, Vector v) => new(u.X - v.X, u.Y - v.Y);
	public static Vector operator -(Vector u, Vector2 v) => new(u.X - v.X, u.Y - v.Y);
	public static Vector operator -(Vector2 u, Vector v) => new(u.X - v.X, u.Y - v.Y);
	public static Vector operator -(Vector vector, int value) => new(vector.X - value, vector.Y - value);
	public static Vector operator -(Vector vector, float value) => new(vector.X - value, vector.Y - value);
	public static Vector operator -(Vector vector, double value) => new(vector.X - value, vector.Y - value);
	public static Vector operator -(int value, Vector vector) => new(value - vector.X, value - vector.Y);
	public static Vector operator -(float value, Vector vector) => new(value - vector.X, value - vector.Y);
	public static Vector operator -(double value, Vector vector) => new(value - vector.X, value - vector.Y);

	// Multiplication
	public static Vector operator *(Vector u, Vector v) => new(u.X * v.X, u.Y * v.Y);
	public static Vector operator *(Vector u, Vector2 v) => new(u.X * v.X, u.Y * v.Y);
	public static Vector operator *(Vector2 u, Vector v) => new(u.X * v.X, u.Y * v.Y);
	public static Vector operator *(Vector vector, int value) => new(vector.X * value, vector.Y * value);
	public static Vector operator *(Vector vector, float value) => new(vector.X * value, vector.Y * value);
	public static Vector operator *(Vector vector, double value) => new(vector.X * value, vector.Y * value);
	public static Vector operator *(int value, Vector vector) => new(value * vector.X, value * vector.Y);
	public static Vector operator *(float value, Vector vector) => new(value * vector.X, value * vector.Y);
	public static Vector operator *(double value, Vector vector) => new(value * vector.X, value * vector.Y);

	// Divisiion
	public static Vector operator /(Vector u, Vector v) => new(u.X / v.X, u.Y / v.Y);
	public static Vector operator /(Vector u, Vector2 v) => new(u.X / v.X, u.Y / v.Y);
	public static Vector operator /(Vector2 u, Vector v) => new(u.X / v.X, u.Y / v.Y);
	public static Vector operator /(Vector vector, int value) => new(vector.X / value, vector.Y / value);
	public static Vector operator /(Vector vector, float value) => new(vector.X / value, vector.Y / value);
	public static Vector operator /(Vector vector, double value) => new(vector.X / value, vector.Y / value);
	public static Vector operator /(int value, Vector vector) => new(value / vector.X, value / vector.Y);
	public static Vector operator /(float value, Vector vector) => new(value / vector.X, value / vector.Y);
	public static Vector operator /(double value, Vector vector) => new(value / vector.X, value / vector.Y);

	// Modulus
	public static Vector operator %(Vector u, Vector v) => new(u.X % v.X, u.Y % v.Y);
	public static Vector operator %(Vector u, Vector2 v) => new(u.X % v.X, u.Y % v.Y);
	public static Vector operator %(Vector2 u, Vector v) => new(u.X % v.X, u.Y % v.Y);
	public static Vector operator %(Vector vector, int value) => new(vector.X % value, vector.Y % value);
	public static Vector operator %(Vector vector, float value) => new(vector.X % value, vector.Y % value);
	public static Vector operator %(Vector vector, double value) => new(vector.X % value, vector.Y % value);
	public static Vector operator %(int value, Vector vector) => new(value % vector.X, value % vector.Y);
	public static Vector operator %(float value, Vector vector) => new(value % vector.X, value % vector.Y);
	public static Vector operator %(double value, Vector vector) => new(value % vector.X, value % vector.Y);

	// Comparators
	public static bool operator ==(Vector u, Vector v) => u.X == v.X && u.Y == v.Y;
	public static bool operator !=(Vector u, Vector v) => u.X != v.X && u.Y != v.Y;
	public static bool operator <(Vector u, Vector v) => u.X < v.X && u.Y < v.Y;
	public static bool operator >(Vector u, Vector v) => u.X > v.X && u.Y > v.Y;
	public static bool operator <=(Vector u, Vector v) => u.X <= v.X && u.Y <= v.Y;
	public static bool operator >=(Vector u, Vector v) => u.X >= v.X && u.Y >= v.Y;

	// Implicit conversion (MonoGame)
	public static implicit operator Size2(Vector vector) => new(((float)vector.X), ((float)vector.Y));
	public static implicit operator Point2(Vector vector) => new(((float)vector.X), ((float)vector.Y));
	public static implicit operator Vector(Point2 point) => new(point.X, point.Y);
	public static implicit operator Vector(Size2 size) => new(size.Width, size.Height);
	public static implicit operator Vector2(Vector vector) => new(((float)vector.X), ((float)vector.Y));
	public static implicit operator Vector(Vector2 vector) => new(vector.X, vector.Y);

	// Helpers
	public static Vector Zero => new(0, 0);
	public static Vector One => new(1, 1);
	public static Vector UnitX => new(1, 0);
	public static Vector UnitY => new(0, 1);

	// Methods
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Normalize()
	{
		double magnitude = (1 / this.Magnitude());
		this.X *= magnitude;
		this.Y *= magnitude;
	}

	// Static methods
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Angle(Vector u, Vector v) => (MathD.Atan2(v.Y - u.Y, v.X - u.X));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Clamp(Vector value, Vector min, Vector max) => new(
		MathD.Clamp(value.X, min.X, max.X),
		MathD.Clamp(value.Y, min.Y, max.Y)
	);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Cross(Vector u, Vector v) => (u.X * v.Y - u.Y * v.X);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Distance(Vector u, Vector v)
	{
		double dx = u.X - v.X;
		double dy = u.Y - v.Y;
		return MathD.Sqrt(dx * dx + dy * dy);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Dot(Vector u, Vector v) => (u.X * v.X + u.Y * v.Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Max(Vector u, Vector v) => new(MathD.Min(u.X, v.X), MathD.Min(u.Y, v.Y));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Mix(Vector u, Vector v) => new(MathD.Min(u.X, v.X), MathD.Min(u.Y, v.Y));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Normalize(Vector vector)
	{
		double magnitude = 1 / vector.Magnitude();
		return new(vector.X *= magnitude, vector.Y *= magnitude);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Random()
	{
		Vector2 random;
		System.Random.Shared.NextUnitVector(out random);

		return random;		
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Round(Vector vector) => new(MathD.Round(vector.X), MathD.Round(vector.Y));

	// Overrides
	public override bool Equals(object obj)
	{
		if (obj is Vector other) return this.Equals(other);		
		return false;
	}

	public override int GetHashCode() => new { this.X, this.Y }.GetHashCode();

	public override string ToString() => "{" + $"X:{this.X} Y:{this.Y}" + "}";
}