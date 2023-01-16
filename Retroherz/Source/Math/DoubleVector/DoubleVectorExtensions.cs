using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

using MathD = System.Math;

namespace Retroherz.Math;

/// <summary>
/// Mathematical extension methods for double precision vectors.
/// </summary>
public static class DoubleVectorExtensions
{
	/// <summary>
	/// Returns an absolute valued copy of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Abs(this DoubleVector vector) => new(MathD.Abs(vector.X), MathD.Abs(vector.Y));

	/// <summary>
	/// Returns the angle between vectors u and v in radians.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Angle(this DoubleVector u, DoubleVector v) => (MathD.Atan2(v.Y - u.Y, v.X - u.X));

	/// <summary>
	/// Returns a clamped copty of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Clamp(this DoubleVector value, DoubleVector min, DoubleVector max) => new(
		MathD.Clamp(value.X, min.X, max.X),
		MathD.Clamp(value.Y, min.Y, max.Y)
	);
	
	/// <summary>
	/// Returns an upwards rounded copty of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Ceiling(this DoubleVector vector) => new(
		MathD.Ceiling(vector.X),
		MathD.Ceiling(vector.Y));

	/// <summary>
	/// Returns the cross product of vectors u and v.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Cross(DoubleVector u, DoubleVector v) => (u.X * v.Y - u.Y * v.X);

	/// <summary>
	/// Returns the distance between vectors u and v.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Distance(this DoubleVector u, DoubleVector v)
	{
		double dx = u.X - v.X;
		double dy = u.Y - v.Y;
		return System.Math.Sqrt(dx * dx + dy * dy);
	}

	/// <summary>
	/// Returns the dot product of vector u and v.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Dot(this DoubleVector u, DoubleVector v) => (u.X * v.X + u.Y * v.Y); 

	/// <summary>
	/// Returns a downwards rounded copy of vector.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Floor(this DoubleVector vector) => new(MathD.Floor(vector.X), MathD.Floor(vector.Y));

	/// <summary>
	/// Returns a slightly shortened version of the vector.
	/// </summary>      
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Interpolate(this DoubleVector u, DoubleVector v, double value) => new(
		u * (1 - value) + v * value
	);

	/// <summary>
	/// Returns true if vector components are NaN (Not a Number).
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNaN(this DoubleVector vector) => double.IsNaN(vector.X) || double.IsNaN(vector.Y);

	/// <summary>
	/// Returns the magnitude (length) of the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Magnitude(this DoubleVector vector) => MathD.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

	/// <summary>
	/// Returns a normalized copy of the vector.
	/// </summary> 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Normalized(this DoubleVector vector)
	{
		DoubleVector normalized = new(vector.X, vector.Y);
		normalized.Normalize();
		return normalized;
	}

	/// <summary>
	/// Returns a rounded copty of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector Round(this DoubleVector vector) => new(MathD.Round(vector.X), MathD.Round(vector.Y));

	/// <summary>
	/// Sets the X component of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector SetX(this DoubleVector vector, double x) => new(x, vector.Y);

	/// <summary>
	/// Sets the Y component of the vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector SetY(this DoubleVector vector, double y) => new(vector.X, y);

	/// <summary>
	/// Returns the angle of the vector in radians.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ToAngle(this DoubleVector vector) => MathD.Atan2(vector.Y, vector.X);

	/// <summary>
	/// Returns a floating point vector.
	/// </summary>    
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoubleVector ToVector(this DoubleVector vector) => new(((float)vector.X), ((float)vector.Y));
}