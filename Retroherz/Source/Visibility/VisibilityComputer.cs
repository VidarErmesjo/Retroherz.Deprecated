using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
//using Microsoft.Xna.Framework;
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
public struct Occluder
{
	public Vector Position;
	public Vector Size;
	public float Radius;
	public float Distance;
	public bool IsOccluded = false;

	public Occluder(Vector position, Vector size, float radius)
	{
		Position = position;
		Size = size;
		Radius = radius;
	}
}

/// <summary>
///	Singleton that handles all visibilty related issues.
/// </summary>
public sealed class VisibilityComputer
{
	#region Private Fields
	private readonly Sekk<Edge> _edges;
	private Sekk<Edge> _polyMap;
	private TiledMap _tiledMap;
	private readonly Sekk<VisibilityPolygonPoint> _visibilityPolygonPoints;

	private Illumer _illumer;

	private const string ILLUMER_ERROR = "SetIllumer() must be called prior to AddOccluder() and CalculateVisibilityPolygon().";
	private const string TILEDMAP_ERROR = "SetTiledMap() must be called prior to SetIllumer().";
	private Exception IllumerNotSetException = new InvalidOperationException(ILLUMER_ERROR);
	private Exception TiledMapNotSetException = new InvalidOperationException(TILEDMAP_ERROR);

	#endregion

	#region Singleton
	private static readonly Lazy<VisibilityComputer> lazy = new Lazy<VisibilityComputer>(
		() => new VisibilityComputer()
	);

	private VisibilityComputer()
	{
		_edges = new Sekk<Edge>(32);
		_polyMap = new Sekk<Edge>(32);
		_visibilityPolygonPoints = new Sekk<VisibilityPolygonPoint>(128);
	}

	public static VisibilityComputer GetInstance() => lazy.Value;
	public static bool IsAlive => lazy.IsValueCreated;

	#endregion

	#region Public Methods
	public ReadOnlySpan<Edge> GetPolyMap() => _polyMap;

	/// <summary>
	///	Adds an "Occluder" to the edges pool if within "Illumer" radius.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddOccluder(in Vector position, in Vector size, in float radius) => AddOccluder(new Occluder(position, size, radius));

	/// <summary>
	///	Adds an "Occluder" to the edges pool if within "Illumer" radius.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddOccluder(in Occluder occluder)
	// HOWDO: Without sort?
	{
		if (_illumer == null) throw IllumerNotSetException;

		// Check if "Occluder" is within "Illumer" radius.
		if (!Visible(occluder.Position + occluder.Size / 2, occluder.Radius)) return;

		// Define corners in an anti-clockwise fashion,
		// and calculate the distances from "Illumer" to "Occluder" edge end-points.
		Span<(Vector Point, float Distance)> corners = stackalloc (Vector Point, float Distance)[4];
	
		// Top left.
		corners[0].Point = occluder.Position;
		corners[0].Distance = Vector.DistanceSquared(corners[0].Point, _illumer.Origin);

		// Top right.
		corners[1].Point = new Vector(occluder.Position.X +  occluder.Size.X,  occluder.Position.Y);
		corners[1].Distance = Vector.DistanceSquared(corners[1].Point, _illumer.Origin);

		// Bottom right.
		corners[2].Point =  occluder.Position +  occluder.Size;
		corners[2].Distance = Vector.DistanceSquared(corners[2].Point, _illumer.Origin);

		// Bottom left.
		corners[3].Point = new Vector(occluder.Position.X,  occluder.Position.Y +  occluder.Size.Y);
		corners[3].Distance = Vector.DistanceSquared(corners[3].Point, _illumer.Origin);

		// Sort edges by distance.
		corners.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);

		// Add the two nearest edges to edges pool.
		_edges.Add(new Edge(start: corners[0].Point, end: corners[1].Point));
		_edges.Add(new Edge(start: corners[0].Point, end: corners[2].Point));
	}

	/// <summary>
	///	Computes the "visibility" polygon from the current edges pool.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<Vector> CalculateVisibilityPolygon()
	{
		if (_illumer == null) throw IllumerNotSetException;

		_visibilityPolygonPoints.Clear();

		foreach (ref readonly Edge p in _edges)
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
					if (j == 0)	angle = ray.Theta - VisibilityPolygonPoint.Tolerance;
					if (j == 1)	angle = ray.Theta;
					if (j == 2)	angle = ray.Theta + VisibilityPolygonPoint.Tolerance;

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
					foreach (ref readonly Edge q in _edges)
					{
						// Create line segment vector
						Vector segment = q.End - q.Start;

						if (Vector.Abs(segment - ray.Direction) > Vector.Zero)
						{
							// t2 is normalised distance from line segment start to line segment end of intersect point.
							float t2 = (ray.Direction.X * (q.Start.Y - _illumer.Origin.Y) +
										ray.Direction.Y * (_illumer.Origin.X - q.Start.X));
							
							t2 /= Vector.Cross(segment, ray.Direction);

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
					}
				}
			}
		}

		// Remove duplicate (or simply similar) points from polygon.
		Span<VisibilityPolygonPoint> unique = VisibilityPolygonPoint.Unique(_visibilityPolygonPoints);

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

	/// <summary>
	///	Sets the "Illumer" object and resets the system.
	/// </summary>
	public void SetIllumer(in Vector origin, in float radius) => SetIllumer(new Illumer(origin, radius));

	/// <summary>
	///	Sets the "Illumer" object and resets the system.
	/// </summary>
	public void SetIllumer(in Illumer illumer)
	{
		if (_polyMap == null) throw TiledMapNotSetException;

		// Set the "Illumer".
		_illumer = illumer;

		// Clear edges.
		_edges.Clear();

		// Add edges.
		AddBoundary();
		AddPolyMap();
	}

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, in string layerName)
	{
		_tiledMap = tiledMap;
		_polyMap = tiledMap.CreatePolyMap(layerName);
	}

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, in ushort layer = 0)
	{
		_tiledMap = tiledMap;
		_polyMap = tiledMap.CreatePolyMap(layer);
	}

	#endregion

	#region Private Methods
	///	<summary>
	///	Adds a map boundary to the edges pool.
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddBoundary()
	{
		// Edge vectors.
		Span<Vector> edge = stackalloc Vector[2];

		// Top (top left to top right).
		edge[0] = new Vector(0, 0);
		edge[1] = new Vector(_tiledMap.WidthInPixels, 0);
		_edges.Add(new Edge(start: edge[0], end: edge[1]));

		// Right (top right to bottom right).
		edge[0] = new Vector(_tiledMap.WidthInPixels, 0);
		edge[1] = new Vector(_tiledMap.WidthInPixels, _tiledMap.HeightInPixels);
		_edges.Add(new Edge(start: edge[0], end: edge[1]));

		// Bottom (bottom right to bottom left).
		edge[0] = new Vector(_tiledMap.WidthInPixels, _tiledMap.HeightInPixels);
		edge[1] = new Vector(0, _tiledMap.HeightInPixels);
		_edges.Add(new Edge(start: edge[0], end: edge[1]));

		// Left (bottom right to top left).
		edge[0] = new Vector(0, _tiledMap.HeightInPixels);
		edge[1] = new Vector(0, 0);
		_edges.Add(new Edge(start: edge[0], end: edge[1]));
	}

	/// <summary>
	///	Evaluates and adds the "PolyMap" edges to the edge pool.
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddPolyMap()
	{
		// Circle that represents the "Illumer".
		Span<Circle> circle = stackalloc Circle[] { new Circle(_illumer.Origin, _illumer.Radius) };

		// Edges vs "Illumer".
		foreach (ref readonly Edge edge in _polyMap)
			if (circle[0].Intersects(edge.Start, edge.End))
				_edges.Add(new Edge(edge.Start, edge.End));
	}

	/// <summary>
	///	Evaluates the visibility of an "Occluder".
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Visible(in Vector origin, in float radius)
	{
		// Circles that represent the "Illumer" and the "Occluder".
		Span<Circle> circle = stackalloc Circle[]
		{
			new Circle(_illumer.Origin, _illumer.Radius),
			new Circle(origin, radius)
		};

		// "Illumer" vs "Occluder".
		return circle[0].Intersects(circle[1]) ? true : false;
	}

	#endregion
}