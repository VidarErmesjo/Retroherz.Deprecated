using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
namespace Retroherz.Math;

/// <summary>
/// Mathematical extension methods for single precision vectors.
/// </summary>
public static class VectorExtensions
{
	/// <summary>
	/// Returns an absolute valued copy of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Abs(this Vector vector) => new(MathF.Abs(vector.X), MathF.Abs(vector.Y));

	/// <summary>
	/// Returns the angle between vectors u and v in radians.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Angle(this Vector u, Vector v) => (MathF.Atan2(v.Y - u.Y, v.X - u.X));

	/// <summary>
	/// Returns a clamped copty of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Clamp(this Vector value, Vector min, Vector max) => new(
		MathHelper.Clamp(value.X, min.X, max.X),
		MathHelper.Clamp(value.Y, min.Y, max.Y)
	);
	
	/// <summary>
	/// Returns an upwards rounded copty of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Ceiling(this Vector vector) => new(
		MathF.Ceiling(vector.X),
		MathF.Ceiling(vector.Y));

	/// <summary>
	/// Returns the cross product of vectors u and v.
	/// </summary>    
	/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Cross(Vector u, Vector v) => (u.X * v.Y - u.Y * v.X);*/

	/// <summary>
	/// Returns the distance between vectors u and v.
	/// </summary> 
	/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(this Vector u, Vector v)
	{
		float dx = u.X - v.X;
		float dy = u.Y - v.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}*/

	/// <summary>
	/// Returns the dot product of vector u and v.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dot(this Vector u, Vector v) => (u.X * v.X + u.Y * v.Y); 

	/// <summary>
	/// Returns a downwards rounded copy of vector.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Floor(this Vector vector) => new(MathF.Floor(vector.X), MathF.Floor(vector.Y));

	/// <summary>
	/// Returns a slightly shortened version of the vector.
	/// </summary>      
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Interpolate(this Vector u, Vector v, float value) => new(u * (1 - value) + v * value);

	/// <summary>
	/// Returns true if vector components are NaN (Not a Number).
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNaN(this Vector vector) => float.IsNaN(vector.X) || float.IsNaN(vector.Y);

	/// <summary>
	/// Returns the magnitude (length) of the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Magnitude(this Vector vector) => MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

	/// <summary>
	/// Returns a normalized copy of the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Normalized(this Vector vector)
	{
		Vector normalized = new(vector.X, vector.Y);
		normalized.Normalize();
		return normalized;
	}

	/// <summary>
	/// Returns a rounded copty of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector Round(this Vector vector) => new(MathF.Round(vector.X), MathF.Round(vector.Y));

	/// <summary>
	/// Sets the X component of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector SetX(this Vector vector, float x) => new(x, vector.Y);

	/// <summary>
	/// Sets the Y component of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector SetY(this Vector vector, float y) => new(vector.X, y);

	/// <summary>
	/// Returns the angle of the vector in radians.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float ToAngle(this Vector vector) => MathF.Atan2(vector.Y, vector.X);

	/// <summary>
	/// Returns a floating point vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector ToVector(this Vector vector) => new(((float)vector.X), ((float)vector.Y));
}