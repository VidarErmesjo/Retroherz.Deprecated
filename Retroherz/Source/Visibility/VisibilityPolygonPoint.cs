using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Retroherz.Math;

namespace Retroherz.Visibility;

internal struct VisibilityPolygonPoint
{
	public required float Theta { get; set; }
	public required Vector Position { get; set; }

	public VisibilityPolygonPoint(Vector position, float theta)
	{
		Position = position;
		Theta = theta;
	}

	///	<summary>
	///	Returns a slice of span containing no "duplicates".
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<VisibilityPolygonPoint> Unique(Span<VisibilityPolygonPoint> visibilityPolygonPoints, float tolerance = 0.1f)
	{
		int k = 0;
		for (int i = 0; i < visibilityPolygonPoints.Length; i++)
		{
			// Compare all elements to each other (once)
			bool isDuplicate = false;
			for (int j = i + 1; j < visibilityPolygonPoints.Length; j++)
			{
				if (Vector.Abs(visibilityPolygonPoints[i].Position - visibilityPolygonPoints[j].Position) < tolerance)
				{
					isDuplicate = true;
					break;
				}
			}

			// Add if not considered "duplicate".
			if (!isDuplicate)
			{
				visibilityPolygonPoints[k] = visibilityPolygonPoints[i];
				k++;
			}
		}

		return visibilityPolygonPoints.Slice(0, k);
	}

	public override string ToString() => "{" + $"Position:{Position} Theta:{Theta}" + "}";
}

// Custom comparer for the class
internal class VisibilityPolygonPointComparer : IEqualityComparer<VisibilityPolygonPoint>
{
	private float _tolerance;

	public VisibilityPolygonPointComparer(float tolerance = 0.1f) => _tolerance = tolerance;

	// Objects are equal if their names and product numbers are equal.
	public bool Equals(VisibilityPolygonPoint a, VisibilityPolygonPoint b)
	{

		//Check whether the compared objects reference the same data.
		if (object.Equals(a, b)) return true;

		//Check whether any of the compared objects is null.
		if (object.Equals(a, null) || object.Equals(b, null))
			return false;

		//Check whether the objects properties are within tolerances.
		return Vector.Abs(a.Position - b.Position) < _tolerance;
	}

	// If Equals() returns true for a pair of objects
	// then GetHashCode() must return the same value for these objects.
	public int GetHashCode(VisibilityPolygonPoint point)
	{
		//Check whether the object is null
		if (object.Equals(point, null)) return 0;

		//Calculate the hash code for the product.
		return point.Position.GetHashCode() ^ point.Position.GetHashCode();
	}
}