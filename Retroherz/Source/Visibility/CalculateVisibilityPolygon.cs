using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Math;

namespace Retroherz.Visibility;

// Bake into VisibilityComputer ??
public static partial class VisibilityExtensions
{
	/// <summary>
	///	Calculates the visibility polygon from a Tiled map "PolyMap".
	/// </summary>
	public static ReadOnlySpan<Vector> CalculateVisibilityPolygon(
		this ReadOnlySpan<PolyMap> polyMap,
		Vector origin,
		float radius
	)
	{
		//if (polyMap == null)
		//	throw new("PolyMap == null");

		//Span<VisibilityPolygonPoint> visibilityPolygonPoints = stackalloc VisibilityPolygonPoint[polyMap.Length * Unsafe.SizeOf<PolyMap>()];
		// Because of duplicate points. Three rays * per edge = 6 possible hits??
		Span<VisibilityPolygonPoint> visibilityPolygonPoints = stackalloc VisibilityPolygonPoint[polyMap.Length * 6];
		int index = 0;

		// Segment/Edge
		foreach (PolyMap p in polyMap)
		{
			// Take the start point, then the end point (we could use a pool of
			// non-duplicated points here, it would be more optimal).
			for (int i = 0; i < 2; i++)
			{
				(float Theta, Vector Direction) ray = (
					Theta: 0,
					Direction: (i == 0 ? p.Start : p.End) - origin
				);

				ray.Theta = ray.Direction.ToAngle();

				float angle = 0;

				// For each point, cast 3 rays, 1 directly at point
				// and 1 a little bit either side.
				for (int j = 0; j < 3; j++)
				{
					if (j == 0)	angle = ray.Theta - 0.0001f;
					if (j == 1)	angle = ray.Theta;
					if (j == 2)	angle = ray.Theta + 0.0001f;

					// Create ray along angle for required distance.
					ray.Direction.X = radius * MathF.Cos(angle);
					ray.Direction.Y = radius * MathF.Sin(angle);

					// Intersection point.
					(float Theta, Vector Position, float T1) point = (
						Theta: 0,
						Position: Vector.Zero,
						T1: float.PositiveInfinity
					);

					bool valid = false;

					// Check for ray intersection with all edges.
					foreach (PolyMap q in polyMap)
					{
						// Create line segment vector
						Vector segment = q.End - q.Start;

						if (Vector.Abs(segment - ray.Direction) > Vector.Zero)
						{
							// t2 is normalised distance from line segment start to line segment end of intersect point.
							float t2 = ray.Direction.X * (q.Start.Y - origin.Y) + ray.Direction.Y * (origin.X - q.Start.X);

							// Normalize.
							t2 /= (segment.X * ray.Direction.Y - segment.Y * ray.Direction.X);

							// t1 is normalised distance from source along ray to ray length of intersect point.
							float t1 = (q.Start.X + segment.X * t2 - origin.X) / ray.Direction.X;

							// If intersect point exists along ray, and along line 
							// segment then intersect point is valid
							if (t1 > 0 && t2 >= 0 && t2 <= 1)
							{
								// Check if this intersect point is closest to source. If
								// it is, then store this point and reject others
								if (t1 < point.T1)
								{
									point.T1 = t1;
									point.Position = origin + ray.Direction * t1;
									point.Theta = (point.Position - origin).ToAngle();

									valid = true;
								}
							}
						}
					}

					// Add intersection point to visibility polygon perimeter.
					if (valid)
					{
						visibilityPolygonPoints[index] = new VisibilityPolygonPoint()
						{
							Position = point.Position,
							Theta = point.Theta
						};
						index++;
					}
				}
			}
		}

		//Console.WriteLine($"Edges:{polyMap.Length} Points:{visibilityPolygonPoints.Length} Index:{index}");

/*
Edges:92 Points:552 Index:552
Edges:92 Points:552 Index:552
Edges:92 Points:552 Index:550	<= Relates to "flashing"
Edges:92 Points:552 Index:552
Edges:92 Points:552 Index:552
*/


		// Remove duplicate (or simply similar) points from polygon.
		/*var s1 = new Stopwatch();
		s1.Start();
		Span<VisibilityPolygonPoint> distinct = _visibilityPolygonPoints
			.Distinct(new VisibilityPolygonPointComparer())
			.ToArray()
			.AsSpan();
		s1.Stop();*/

		// Seems to work :)
		// Maybe do a value typle type next?

		// Remove duplicate (or simply similar) points from polygon.
		Span<VisibilityPolygonPoint> unique = VisibilityPolygonPoint.Unique(visibilityPolygonPoints.Slice(0, index)); //VisibilityPolygonPoint.Unique(_visibilityPolygonPoints.AsSpan());

		// Sort perimeter points by angle from source. 
		// This will allow us to draw a triangle fan.
		unique.Sort((a, b) => a.Theta < b.Theta ? -1 : 1);

		// Return only the points.
		Span<Vector> sorted = new Vector[unique.Length];

		for (int i = 0; i < sorted.Length; i++)
			sorted[i] = unique[i].Position;

		//Console.WriteLine($"Count:{_visibilityPolygonPoints.Count} Distinct:{s1.Elapsed.TotalNanoseconds} Unique:{s2.Elapsed.TotalNanoseconds}");
		/*
		Count:432 Distinct:3524100 Unique:971000
		Count:432 Distinct:3686700 Unique:887400
		Count:432 Distinct:3680800 Unique:801400
		Count:432 Distinct:4246700 Unique:896900
		Count:432 Distinct:4691500 Unique:779500
		Count:432 Distinct:3442400 Unique:839400
		Count:432 Distinct:3572900 Unique:891400
		Count:432 Distinct:3922400 Unique:892300
		Count:432 Distinct:4044900 Unique:981600
		Count:432 Distinct:5380900 Unique:790000
		Count:432 Distinct:4533500 Unique:953800
		Count:432 Distinct:4321800 Unique:967000
		Count:432 Distinct:3279500 Unique:952100
		Count:432 Distinct:3684200 Unique:967400
		Count:432 Distinct:3304000 Unique:947900
		Count:432 Distinct:3654200 Unique:918800
		Count:432 Distinct:4411800 Unique:919800

		Around six times as fast. :)
		*/

		return sorted;
	}
}