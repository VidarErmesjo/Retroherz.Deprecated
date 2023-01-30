using System.Runtime.CompilerServices;

namespace Retroherz.Math;

public static class MathExtensions
{
	///	<summary>
	///	Calculates the "fast" square root of 'x' as of
	/// popularized by John Carmack ("Quake Arena" - iD Software).
	/// DO NOT USE!
	///	</summary>
	///	<returns>An approximation of the square root of 'x'.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Sqrt(float x, int steps = 1) => InvSqrt(x, steps) * x;

	///	<summary>
	///	Calculates the "fast" inverse square root of 'x' as of
	/// popularized by John Carmack ("Quake Arena" - iD Software).
	/// DO NOT USE!
	///	</summary>
	///	<returns>An approximation of the inverse square root of 'x'.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	unsafe public static float InvSqrt(float x, int steps = 1)
	// http://www.lomont.org/papers/2003/InvSqrt.pdf
	{
		float xHalf = 0.5f * x;

		// Get bits for floating value.
		int i = *(int*) &x;

		// Give initial guess y0 (Newtons Method).
		i = 0x5f375a86 - (i>>1);

		// Convert bits back to float.
		x = *(float*) &i;

		// Newton step, repeating increases accuracy.
		for (int j = 0; j < steps; j++)
			x = x * (1.5f - xHalf * x * x);

		return x;
	}
}