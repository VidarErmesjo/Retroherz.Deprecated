using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz
{
	internal partial class Utils
	{
		// Needs a better name
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static RectangleF ToRectangleF(in Vector2 a, in Vector2 b)
		{
			var start = a;
			var end = b;
			Utils.MinMax(ref start, ref end);

			return new(start, (end - start).ToSize());
		}		
	}
}