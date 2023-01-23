using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Retroherz.Math;
namespace Retroherz.Visibility;

public struct PolyMap
{
	public Vector Start;
	public Vector End;

	public PolyMap(Vector start, Vector end)
	{
		Start = start;
		End = end;
	}


	///	<summary>
	///	Returns a slice of span containing no "duplicates".
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<PolyMap> Unique(Span<PolyMap> polyMap, float tolerance = 0.1f)
	{
		int k = 0;
		for (int i = 0; i < polyMap.Length; i++)
		{
			// Compare all elements to each other (once)
			bool isDuplicate = false;
			for (int j = i + 1; j < polyMap.Length; j++)
			{
				if (polyMap[i].Start == polyMap[j].Start || polyMap[i].End == polyMap[j].End)
				{
					isDuplicate = true;
					break;
				}
			}

			// Add if not considered "duplicate".
			if (!isDuplicate)
			{
				polyMap[k] = polyMap[i];
				k++;
			}
		}

		return polyMap.Slice(0, k);
	}
}