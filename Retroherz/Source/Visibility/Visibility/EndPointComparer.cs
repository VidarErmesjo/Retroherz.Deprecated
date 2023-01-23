using System.Collections.Generic;

namespace Retroherz.Visibility.Deprecated;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip


internal class EndPointComparer : IComparer<EndPoint>
{
	internal EndPointComparer() {}
	
	// Helper: comparison function for sorting points by angle.
	public int Compare(EndPoint a, EndPoint b)
	{          
		// Traverse in angle order.
		if (a.Angle > b.Angle) { return 1; }
		if (a.Angle < b.Angle) { return -1; }

		// But for ties we want Begin nodes before End nodes.
		if (!a.Begin && b.Begin) { return 1; }
		if (a.Begin && !b.Begin) { return -1; }

		return 0;
	}
}