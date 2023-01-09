using System.Runtime.CompilerServices;

namespace Retroherz
{
	internal partial class Utils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);
	}

}