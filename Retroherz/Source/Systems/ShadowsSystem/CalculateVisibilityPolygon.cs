using System;
using System.Linq;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using Retroherz.Math;
namespace Retroherz.Systems;

public partial class ShadowsSystem
{
	/// <summary>
	///	Calculates the visibility polygon from a Tiled map "PolyMap".
	/// </summary>
	private void CalculateVisibilityPolygon(Vector origin, float radius)
	{
		// Get rid of existing polygons.
		visibilityPolygonPoints.Clear();

		// Create "PolyMap" from Tiled map.
		ReadOnlySpan<PolyMapEdge> polyMap = CreatePolyMap(_tiledMap); //_tiledMap.CreatePolyMap();

		foreach (PolyMapEdge p in polyMap)
		{
			// Take the start point, then the end point (we could use a pool of
			// non-duplicated points here, it would be more optimal).
			for (int i = 0; i < 2; i++)
			{
				(float Theta, Vector Direction) ray = new(
					0,
					(i == 0 ? p.Start : p.End) - origin
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
					(float Theta, Vector Position, float t1) point = new(
						0,
						Vector.Zero,
						float.MaxValue
					);

					bool valid = false;

					// Check for ray intersection with all edges.
					foreach (PolyMapEdge q in polyMap)
					{
						// Create line segment vector
						Vector segment = q.End - q.Start;

						if (Vector.Abs(segment - ray.Direction) > Vector.Zero)
						{
							// t2 is normalised distance from line segment start to line segment end of intersect point.
							float t2 = ray.Direction.X * (q.Start.Y - origin.Y) + ray.Direction.Y * (origin.X - q.Start.X);
							t2 /= (segment.X * ray.Direction.Y - segment.Y * ray.Direction.X);

							// t1 is normalised distance from source along ray to ray length of intersect point.
							float t1 = (q.Start.X + segment.X * t2 - origin.X) / ray.Direction.X;

							// If intersect point exists along ray, and along line 
							// segment then intersect point is valid
							if (t1 > 0 && t2 >= 0 && t2 <= 1.0f)
							{
								// Check if this intersect point is closest to source. If
								// it is, then store this point and reject others
								if (t1 < point.t1)
								{
									point.t1 = t1;
									point.Position = origin + ray.Direction * t1;
									point.Theta = (point.Position - origin).ToAngle();

									valid = true;
								}
							}
						}
					}

					// Add intersection point to visibility polygon perimeter.
					if (valid)
							visibilityPolygonPoints.Add(
								new VisibilityPolygonPoint()
								{
									Position = point.Position,
									Theta = point.Theta
								}
							);
				}
			}
		}

		// Remove duplicate (or simply similar) points from polygon.
		VisibilityPolygon = new(
			visibilityPolygonPoints
			.Distinct(new VisibilityPolygonPointComparer())
			.ToArray()
		);

		// Sort perimeter points by angle from source. 
		// This will allow us to draw a triangle fan.
		VisibilityPolygon.Span.Sort((a, b) => a.Theta < b.Theta ? -1 : 1);
	}
}