using System;
using System.Collections.Generic;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class ShadowsSystem
{
	internal struct VisibilityPolygonPoint
	{
		public required float Theta { get; set; }
		public required Vector Position { get; set; }
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
			if (Object.ReferenceEquals(a, b)) return true;

			//Check whether any of the compared objects is null.
			if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
				return false;

			//Check whether the objects properties are within tolerances.
			return Vector.Abs(a.Position - b.Position) < _tolerance;
		}

		// If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.
		public int GetHashCode(VisibilityPolygonPoint point)
		{
			//Check whether the object is null
			if (Object.ReferenceEquals(point, null)) return 0;

			//Get hash code for the Position field if it is not null.
			int hashProductName = point.Position == null ? 0 : point.Position.GetHashCode();

			//Get hash code for the Code field.
			int hashProductCode = point.Position.GetHashCode();

			//Calculate the hash code for the product.
			return hashProductName ^ hashProductCode;
		}
	}
}