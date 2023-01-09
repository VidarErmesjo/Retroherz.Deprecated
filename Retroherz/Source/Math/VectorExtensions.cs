using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

using MathD = System.Math;

namespace Retroherz.Math;

public static class VectorExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector SetX(this Vector vector, double x) => new(x, vector.Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector SetY(this Vector vector, double y) => new(vector.X, y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Translate(this Vector vector, double x, double y) => new(vector.X + x, vector.Y + y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Magnitude(this Vector vector) => MathD.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Normalized(this Vector vector)
	{
		Vector normalized = new(vector.X, vector.Y);
		normalized.Normalize();
		return normalized;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Abs(this Vector vector) => new(MathD.Abs(vector.X), MathD.Abs(vector.Y));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Clamp(this Vector value, Vector min, Vector max) => new(
		MathD.Clamp(value.X, min.X, max.X),
		MathD.Clamp(value.Y, min.Y, max.Y)
	);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Ceiling(this Vector vector) => new(MathD.Ceiling(vector.X), MathD.Ceiling(vector.Y));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Distance(Vector u, Vector v)
	{
		double dx = u.X - v.X;
		double dy = u.Y - v.Y;
		return System.Math.Sqrt(dx * dx + dy * dy);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Dot(this Vector u, Vector v) => (u.X * v.X + u.Y * v.Y); 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Floor(this Vector vector) => new(MathD.Floor(vector.X), MathD.Floor(vector.Y));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Round(this Vector vector) => new(MathD.Round(vector.X), MathD.Round(vector.Y));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Cross(Vector u, Vector v) => (u.X * v.Y - u.Y * v.X);

	// Conversion
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ToAngle(this Vector vector) => MathD.Atan2(vector.X, vector.Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 ToVector2(this Vector vector) => new(((float)vector.X), ((float)vector.Y));

	// Utility
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNaN(this Vector vector) => double.IsNaN(vector.X) || double.IsNaN(vector.Y);
}