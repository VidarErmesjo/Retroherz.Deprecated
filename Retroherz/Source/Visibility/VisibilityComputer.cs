using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Collections;
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
	private Sekk<PolyMap> _edges;
	private Sekk<PolyMap> _tiledMap; // Depricated
	private Sekk<VisibilityPolygonPoint> _visibilityPolygonPoints;

	private Illumer _illumer;

	private const string INIT_ERROR = "SetIllumer() must be called prior to AddOccluder() and CalculateVisibilityPolygon().";
	private Exception IllumerNotSetException = new InvalidOperationException(INIT_ERROR);

	#endregion

	#region Singleton
	private static readonly Lazy<VisibilityComputer> lazy = new Lazy<VisibilityComputer>(
		() => new VisibilityComputer()
	);

	private VisibilityComputer()
	{
		_edges = new Sekk<PolyMap>(1000);
		_visibilityPolygonPoints = new Sekk<VisibilityPolygonPoint>(1000);
	}

	public static VisibilityComputer GetInstance() => lazy.Value;
	public static bool IsAlive => lazy.IsValueCreated;

	#endregion

	#region Public Methods
	public ReadOnlySpan<PolyMap> GetPolyMap() => _tiledMap;

	/// <summary>
	///	Adds a rectangular "Occluder" to the edges pool if within "Illumer" radius.
	/// </summary>
	public void AddOccluder(Vector position, Vector size) => AddOccluder(new RectangleF(position, size));

	/// <summary>
	///	Adds a rectangular "Occluder" to the edges pool if within "Illumer" radius.
	/// </summary>
	public void AddOccluder(RectangleF occluder)
	//public void AddOccluder(BoundingRectangle occluder)
	{
		if (_illumer == null) throw IllumerNotSetException;
		
		ReadOnlySpan<Vector2> corners = occluder.GetCorners().AsSpan();

		// Calculate the distances from "Illumer" to "Occluder" edge end-points.
		Span<(Vector Point, float Distance)> sorted = stackalloc (Vector, float)[corners.Length];
		for (int i = 0; i < corners.Length; i++)
		{
			sorted[i].Point = corners[i];
			sorted[i].Distance = Vector.DistanceSquared(corners[i], _illumer.Origin);
		}

		// Sort to get the two nearest edges.
		sorted.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);

		// Add the two nearest edges if their shared point is contained within "Illumer" radius.
		/*CircleF circle = new CircleF(_illumer.Origin, _illumer.Radius);
		if (circle.Contains(sorted[0].Point))
		{
			//_edges.Add(new PolyMap(sorted[0].Point, sorted[1].Point));
			//_edges.Add(new PolyMap(sorted[0].Point, sorted[2].Point));
			_edges.Add(new PolyMap(corners[0], corners[1]));
			_edges.Add(new PolyMap(corners[1], corners[2]));
			_edges.Add(new PolyMap(corners[2], corners[3]));
			_edges.Add(new PolyMap(corners[3], corners[0]));
		}*/

		Vector u, v;

		Circle circle = new Circle(_illumer.Origin, _illumer.Radius);
		//if (circ.Intersects(sorted[0].Point, sorted[1].Point, out u, out v) || circ.Intersects(sorted[0].Point, sorted[2].Point, out u, out v))
		//	Console.WriteLine($"u:{u} v:{v}");
		if (circle.Intersects(sorted[0].Point, sorted[1].Point, out u, out v))
			_edges.Add(new PolyMap(sorted[0].Point, sorted[1].Point));
			//_edges.Add(new PolyMap(u, v));

		if (circle.Intersects(sorted[0].Point, sorted[2].Point, out u, out v))
			_edges.Add(new PolyMap(sorted[0].Point, sorted[2].Point));
			//_edges.Add(new PolyMap(u, v));

		/*if (circle.Contains(corners[0]) || circle.Contains(corners[1]))
			_edges.Add(new PolyMap(corners[0], corners[1]));

		if (circle.Contains(corners[1]) || circle.Contains(corners[2]))
			_edges.Add(new PolyMap(corners[1], corners[2]));

		if (circle.Contains(corners[2]) || circle.Contains(corners[3]))
			_edges.Add(new PolyMap(corners[2], corners[3]));

		if (circle.Contains(corners[3]) || circle.Contains(corners[0]))
			_edges.Add(new PolyMap(corners[3], corners[0]));*/
	}

	/// <summary>
	///	Sets the "Illumer" object and resets the system.
	/// </summary>
	public void SetIllumer(Vector origin, float radius) => SetIllumer(new Illumer(origin, radius));

	/// <summary>
	///	Sets the "Illumer" object and resets the system.
	/// </summary>
	public void SetIllumer(Illumer illumer)
	{
		_edges.Clear();
		_illumer = illumer;
	}

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, string layerName) =>	_tiledMap = tiledMap.CreatePolyMap(layerName);

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, ushort layer = 0) => _tiledMap = tiledMap.CreatePolyMap(layer);

	/// <summary>
	///	Computes the "visibility" polygon from the current edges pool.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<Vector> CalculateVisibilityPolygon()
	{
		if (_illumer == null) throw IllumerNotSetException;

		_visibilityPolygonPoints.Clear();

		Vector u, v;

		//CircleF circle = new CircleF(_illumer.Origin, _illumer.Radius);
		/*Circle circle = new Circle(_illumer.Origin, _illumer.Radius);
		foreach (PolyMap edge in _tiledMap)
			if (circle.Intersects(edge.Start, edge.End, out u, out v))
				_edges.Add(new PolyMap(u, v));*/

		//_edges.Add(new PolyMap(Vector.One * 16, Vector.One * 64));

		//AddCircularFallbackBoundary(16); // Glitchy on statics. Perfect on dynamics?
		AddCircularFallbackBoundary(3, 2); // vs. 16, 1 ?? What is best
		
		//AddCircularFallbackBoundary(16);
		// 9 edges is the magic number?? Glitchy around Origin < 32 && 80 (rad 96)
		// 96 / 2 = 48. 48 + 32 = 80! also 48 + 80 = 128!
		// 18 edges. Starts at eighteenth tile: 8 * 16 = 128. 8 => Positive Infiinity?
		// 9 edges. Starts at 2: 2 * 16 = 32, happens at 5: 5 * 16 = 80
		//AddQuadraticFallbackBoundary(0.5f);	// "Perfect" on statics. Glitchy on dynamic.

		//_edges.Add(new PolyMap(Vector.Zero, Vector.One * 128));	// Diagnonal edge

		foreach (PolyMap p in _edges)
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
					foreach (PolyMap q in _edges)
					{
						// Create line segment vector
						Vector segment = q.End - q.Start;


						if (Vector.Abs(segment - ray.Direction) > Vector.Zero)
						{
							// t2 is normalised distance from line segment start to line segment end of intersect point.
							float t2 = (ray.Direction.X * (q.Start.Y - _illumer.Origin.Y) +
										ray.Direction.Y * (_illumer.Origin.X - q.Start.X)) /
										Vector.Cross(segment, ray.Direction);

						if (Vector.Cross(segment, ray.Direction) == 0)
							Console.WriteLine("Zero!");

						if (float.IsNaN(Vector.Cross(segment, ray.Direction)))
							Console.WriteLine("IsNaN!");

						if (float.IsPositiveInfinity(Vector.Cross(segment, ray.Direction)))
							Console.WriteLine("IsPositiveInfinity");

						if (float.IsNegativeInfinity(Vector.Cross(segment, ray.Direction)))
							Console.WriteLine("IsNegativeInfinity");

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
						var visibilityPolygonPoint = new VisibilityPolygonPoint()
						{
							Position = point.Position,
							Theta = point.Theta
						};

						_visibilityPolygonPoints.Add(visibilityPolygonPoint);

						if (point.Position == Vector.Zero)
							Console.WriteLine("Zero!");

						if (float.IsNaN(point.Position.X) || float.IsNaN(point.Position.Y))
							Console.WriteLine("IsNaN!");

						if (float.IsPositiveInfinity(point.Position.X) || float.IsPositiveInfinity(point.Position.Y))
							Console.WriteLine("IsPositiveInfinity");

						if (float.IsNegativeInfinity(point.Position.X) || float.IsNegativeInfinity(point.Position.Y))
							Console.WriteLine("IsNegativeInfinity");


					}
				}
			}
		}

		// Remove duplicate (or simply similar) points from polygon.
		Span<VisibilityPolygonPoint> unique = VisibilityPolygonPoint.Unique(_visibilityPolygonPoints);
		//unique = _visibilityPolygonPoints;

		//Console.WriteLine($"Edges:{_edges.Count} Points:{_visibilityPolygonPoints.Count} Unique:{unique.Length} Origin:{_illumer.Origin}");

		// Sort perimeter points by angle from source. 
		// This will allow us to draw a triangle fan.
		unique.Sort((a, b) => a.Theta < b.Theta ? -1 : 1);

		// Return only the points.
		Span<Vector> sorted = new Vector[unique.Length];	// Heap alloc here. Opt?? - VE

		for (int i = 0; i < sorted.Length; i++)
			sorted[i] = unique[i].Position;

		// Reset illumer.
		_illumer = null;

		return sorted;
	}

	#endregion

	#region Private Methods
	/// <summary>
	///	Adds a circular "fallback" boundary with "Illumer" radius.
	/// </summary>
	private void AddCircularFallbackBoundary(int edges = 16, float multiplier = 1)
	{
		float delta = MathHelper.TwoPi / edges;
		float angle = 0;
		float radius = _illumer.Radius * multiplier;

		Vector start, end;

		for (int i = 0; i < edges; i++)
		{
			start = new Vector(_illumer.Origin.X - radius * MathF.Cos(angle), _illumer.Origin.Y - radius * MathF.Sin(angle));
			angle += delta;
			end = new Vector(_illumer.Origin.X - radius * MathF.Cos(angle), _illumer.Origin.Y - radius * MathF.Sin(angle));
			_edges.Add(new PolyMap(start, end));
		}
	}

	/// <summary>
	///	Adds a quadratic "fallback" boundary. Edges have a length of twice the Illumer radius.
	/// </summary>
	private void AddQuadraticFallbackBoundary(float multiplier = 1)
	{
		Vector start, end;
		float x, y;
		float length = _illumer.Radius * multiplier;

		// Left to right (top)
		y = _illumer.Origin.Y - length;
		start = new Vector(_illumer.Origin.X - length, y);
		end = new Vector(_illumer.Origin.X + length, y);
		_edges.Add(new PolyMap(start, end));

		// Left to right (bottom)
		y = _illumer.Origin.Y + length;
		start = new Vector(_illumer.Origin.X - length, y);
		end = new Vector(_illumer.Origin.X + length, y);
		_edges.Add(new PolyMap(start, end));

		// Top to bottom (left)
		x = _illumer.Origin.X - length;
		start = new Vector(x, _illumer.Origin.Y - length);
		end = new Vector(x, _illumer.Origin.Y + length);
		_edges.Add(new PolyMap(start, end));

		// Top to bottom (right)
		x = _illumer.Origin.X + length;
		start = new Vector(x, _illumer.Origin.Y - length);
		end = new Vector(x, _illumer.Origin.Y + length);
		_edges.Add(new PolyMap(start, end));
	}

	#endregion
}