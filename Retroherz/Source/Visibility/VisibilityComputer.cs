using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Math;

namespace Retroherz.Visibility;

/// <summary>
///	Represents a caster of light.
/// </summary>
public class Illumer
{
	public Vector Origin;
	public float Radius;

	public Illumer(Vector origin, float radius)
	{
		Origin = origin;
		Radius = radius;
	}
}

/// <summary>
///	Represents an occluder of light.
/// </summary>
public class Occluder
{
	public Vector Position;
	public Vector Size;

	public Occluder(Vector position, Vector size)
	{
		Position = position;
		Size = size;
	}
}

/// <summary>
///	Singleton that handles all visibilty related issues.
/// </summary>
public sealed class VisibilityComputer
{
	#region Private Fields
	private ReadOnlyMemory<PolyMap> _tiledMap;
	private Memory<PolyMap> _occluders;
	private Memory<PolyMap> _edges;
	private int _numEdges = 0;

	private Illumer _illumer;

	#endregion

	#region Singleton
	private static readonly Lazy<VisibilityComputer> lazy = new Lazy<VisibilityComputer>(
		() => new VisibilityComputer()
	);

	private VisibilityComputer() {}

	public static VisibilityComputer GetInstance() => lazy.Value;
	public static bool IsAlive => lazy.IsValueCreated;

	#endregion

	#region Public Methods

	public ReadOnlySpan<PolyMap> GetPolyMap() => _tiledMap.Span;

	/// <summary>
	///	Adds a rectangular occluder to the edges pool.
	/// </summary>
	public void AddOccluder(RectangleF occluder)
	{
		if (_illumer == null)
			throw new InvalidOperationException("Illumer not set.");
		
		ReadOnlySpan<Vector2> corners = occluder.GetCorners().AsSpan();

		// Add all edges.
		//_occluders.Add(new PolyMap(new Vector(corners[0]), new Vector(corners[1])));
		//_occluders.Add(new PolyMap(new Vector(corners[1]), new Vector(corners[2])));
		//_occluders.Add(new PolyMap(new Vector(corners[2]), new Vector(corners[3])));
		//_occluders.Add(new PolyMap(new Vector(corners[3]), new Vector(corners[0])));

		/*Span<(Vector Point, float Distance)> sorted = stackalloc (Vector, float)[corners.Length];

		// Calculate distance from illumer to occluder corners
		for (int i = 0; i < corners.Length; i++)
		{
			sorted[i].Point = corners[i];
			sorted[i].Distance = Vector.DistanceSquared(corners[i], _illumer.Origin);
		}

		// Sort ascending.
		sorted.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);

		// Add the two closest edges and ignore rest.
		_occluders.Add(new PolyMap(sorted[0].Point, sorted[1].Point));
		_occluders.Add(new PolyMap(sorted[0].Point, sorted[2].Point));*/

		//Span<PolyMap> occluders = stackalloc PolyMap[_occluders.Length + 4];
		Span<PolyMap> occluders = stackalloc PolyMap[4];

		// Add all four edges.
		occluders[0] = new PolyMap(new Vector(corners[0]), new Vector(corners[1]));
		occluders[1] = new PolyMap(new Vector(corners[1]), new Vector(corners[2]));
		occluders[2] = new PolyMap(new Vector(corners[2]), new Vector(corners[3]));
		occluders[3] = new PolyMap(new Vector(corners[3]), new Vector(corners[0]));

		// Try to copy to memory. Resize if necessary.
		if (!occluders.TryCopyTo(_occluders.Span))
		{
			Span<PolyMap> temp = stackalloc PolyMap[_occluders.Length + occluders.Length];

			// Copy new.
			occluders.CopyTo(temp);

			// Append old after new.
			_occluders.Span.CopyTo(temp.Slice(4));

			// Resize memory and copy to from temporary.
			_occluders = new PolyMap[_occluders.Length + occluders.Length];
			temp.CopyTo(_occluders.Span);

			Console.WriteLine($"Occluders:{_occluders.Length}");
		}
		else
		{
			occluders.CopyTo(_occluders.Span);
		}
	}

	/// <summary>
	///	Sets the illumer object.
	/// </summary>
	public void SetIllumer(Vector origin, float radius) => SetIllumer(new Illumer(origin, radius));

	/// <summary>
	///	Sets the illumer object.
	/// </summary>
	public void SetIllumer(Illumer illumer) => _illumer = illumer;

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, string layerName) =>	_tiledMap = tiledMap.CreatePolyMap(layerName);

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, ushort layer = 0) => _tiledMap = tiledMap.CreatePolyMap(layer);

	/// <summary>
	///	Calculates the visibility polygon from the current edges pool.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<Vector> CalculateVisibilityPolygon()	// Compute ??
	{

		if (_tiledMap.Length < 1)
			throw new InvalidOperationException("TiledMap not set.");

		if (_illumer == null)
			throw new InvalidOperationException("Illumer not set.");

		// Merge "PolyMaps"
		Span<PolyMap> edges = stackalloc PolyMap[_tiledMap.Length + _occluders.Length];
		_tiledMap.Span.CopyTo(edges);
		_occluders.Span.CopyTo(edges.Slice(_tiledMap.Length));

		// Because of duplicate points. Three rays * per edge = 6 possible hits??
		Span<VisibilityPolygonPoint> visibilityPolygonPoints = stackalloc VisibilityPolygonPoint[edges.Length * 6];
		int index = 0;

		// Segment/Edge
		foreach (PolyMap p in edges)
		{
			// Take the start point, then the end point (we could use a pool of
			// non-duplicated points here, it would be more optimal).
			for (int i = 0; i < 2; i++)
			{
				(float Theta, Vector Direction) ray = (
					Theta: 0,
					Direction: (i == 0 ? p.Start : p.End) - _illumer.Origin
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
					ray.Direction.X = _illumer.Radius * MathF.Cos(angle);
					ray.Direction.Y = _illumer.Radius * MathF.Sin(angle);

					// Intersection point.
					(float Theta, Vector Position, float T1) point = (
						Theta: 0,
						Position: Vector.Zero,
						T1: float.PositiveInfinity
					);

					bool valid = false;

					// Check for ray intersection with all edges.
					foreach (PolyMap q in edges)
					{
						// Create line segment vector
						Vector segment = q.End - q.Start;

						if (Vector.Abs(segment - ray.Direction) > Vector.Zero)
						{
							// t2 is normalised distance from line segment start to line segment end of intersect point.
							float t2 = ray.Direction.X * (q.Start.Y - _illumer.Origin.Y) + ray.Direction.Y * (_illumer.Origin.X - q.Start.X);

							// Normalize.
							t2 /= (segment.X * ray.Direction.Y - segment.Y * ray.Direction.X);

							// t1 is normalised distance from source along ray to ray length of intersect point.
							float t1 = (q.Start.X + segment.X * t2 - _illumer.Origin.X) / ray.Direction.X;

							// If intersect point exists along ray, and along line 
							// segment then intersect point is valid
							if (t1 > 0 && t2 >= 0 && t2 <= 1)
							{
								// Check if this intersect point is closest to source. If
								// it is, then store this point and reject others
								if (t1 < point.T1)
								{
									point.T1 = t1;
									point.Position = _illumer.Origin + ray.Direction * t1;
									point.Theta = (point.Position - _illumer.Origin).ToAngle();

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

		// Clear "dynamic" occluders.
		_occluders.Span.Clear();

		// Reset illumer.
		_illumer = null;

		//Console.WriteLine($"Edges:{edges.Length} Points:{visibilityPolygonPoints.Length} Index:{index}");

/*
Edges:92 Points:552 Index:552
Edges:92 Points:552 Index:552
Edges:92 Points:552 Index:550	<= Relates to "flashing"
Edges:92 Points:552 Index:552
Edges:92 Points:552 Index:552
*/

		// Remove duplicate (or simply similar) points from polygon.
		Span<VisibilityPolygonPoint> unique = VisibilityPolygonPoint.Unique(visibilityPolygonPoints.Slice(0, index));

		// Sort perimeter points by angle from source. 
		// This will allow us to draw a triangle fan.
		unique.Sort((a, b) => a.Theta < b.Theta ? -1 : 1);

		// Return only the points.
		Span<Vector> sorted = new Vector[unique.Length];

		for (int i = 0; i < sorted.Length; i++)
			sorted[i] = unique[i].Position;

		return sorted;
	}

	#endregion

	#region Private Methods



	#endregion
}