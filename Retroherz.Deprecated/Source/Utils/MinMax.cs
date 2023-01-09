using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Retroherz
{
	internal partial class Utils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void MinMax(ref Vector2 a, ref Vector2 b)
		{
			if (a.X > b.X) Utils.Swap<float>(ref a.X, ref b.X);
			if (a.Y > b.Y) Utils.Swap<float>(ref a.Y, ref b.Y);
		}
	}
}