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
	private Sekk<PolyMap> _edges;
	private Sekk<BlockMap> _blockMap;
	private TiledMap _tiledMap;
	private Sekk<Occluder> _occluders;
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
		_occluders = new Sekk<Occluder>(64);
		_visibilityPolygonPoints = new Sekk<VisibilityPolygonPoint>(1000);
	}

	public static VisibilityComputer GetInstance() => lazy.Value;
	public static bool IsAlive => lazy.IsValueCreated;

	#endregion

	#region Public Methods
	//public ReadOnlySpan<PolyMap> GetPolyMap() => _tileMap;

	/// <summary>
	///	Adds an "Occluder" to the edges pool if within "Illumer" radius.
	/// </summary>
	public void AddOccluder(Vector position, Vector size, float radius) => AddOccluder(new Occluder(position, size, radius));

	/// <summary>
	///	Adds an "Occluder" to the edges pool if within "Illumer" radius.
	/// </summary>
	public void AddOccluder(Occluder occluder)
	// IDEA: "adding fake edges in the shape you need around your light source before casting."
	// TODO: Figure out how to not add occluded "Occluders", like walls behind the closest walls.
	// QuadTree?
	// Creating a "BlockMap" - only bordering tiles are actual size - the rest as big as allowed.
	// Currently hitting 52 objects on current map.
	{
		if (_illumer == null) throw IllumerNotSetException;

		// Check if "Occluder" is within "Illumer" radius.
		if (!Visible(occluder.Position + occluder.Size / 2, occluder.Radius)) return;
		_occluders.Add(occluder);

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
		_edges.Add(new PolyMap(corners[0].Point, corners[1].Point));
		_edges.Add(new PolyMap(corners[0].Point, corners[2].Point));
	}

	/// <summary>
	///	Computes the "visibility" polygon from the current edges pool.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<Vector> CalculateVisibilityPolygon()
	{
		if (_illumer == null) throw IllumerNotSetException;

		//EvaluateOccluders();
		/*Circle cirlce = new Circle(_illumer.Origin, _illumer.Radius);
		foreach (PolyMap edge in _blockMap)
		private TiledMap _tiledMap;
			if (cirlce.Intersects(edge.Start, edge.End))
				_edges.Add(edge);*/

		_visibilityPolygonPoints.Clear();

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
					foreach (PolyMap q in _edges)
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

		//Console.WriteLine($"Edges:{_edges.Count} Points:{_visibilityPolygonPoints.Count} Unique:{unique.Length} Origin:{_illumer.Origin} Visible:{_occluders.Count}");

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
	public void SetIllumer(Vector origin, float radius) => SetIllumer(new Illumer(origin, radius));

	/// <summary>
	///	Sets the "Illumer" object and resets the system.
	/// </summary>
	public void SetIllumer(Illumer illumer)
	{
		_edges.Clear();
		_illumer = illumer;
		_occluders.Clear();
		AddBoundary();
		//AddFallbackBoundary();
		//AddPolyMap();	// PolyMap is a bad idea. Need to break down tiles into bigger rectangles (as planned) => enttities.
	}

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, string layerName) => _tiledMap = tiledMap.GetTiledMap();//	_blockMap = tiledMap.CreateBlockMap(layerName);

	/// <summary>
	///	Sets and adds the Tiled map to the edges pool.
	/// </summary>
	public void SetTiledMap(TiledMap tiledMap, ushort layer = 0) => _tiledMap = tiledMap.GetTiledMap();// _blockMap = tiledMap.CreateBlockMap(layer);

	#endregion

	#region Private Methods
	// EXP
	public void AddBoundary()
	{
		Vector start, end;

		// Top (top left to top right).
		start = new Vector(0, 0);
		end = new Vector(_tiledMap.WidthInPixels, 0);
		_edges.Add(new PolyMap(start, end));

		// Right (top right to bottom right).
		start = new Vector(_tiledMap.WidthInPixels, 0);
		end = new Vector(_tiledMap.WidthInPixels, _tiledMap.HeightInPixels);
		_edges.Add(new PolyMap(start, end));

		// Bottom (bottom right to bottom left).
		start = new Vector(_tiledMap.WidthInPixels, _tiledMap.HeightInPixels);
		end = new Vector(0, _tiledMap.HeightInPixels);
		_edges.Add(new PolyMap(start, end));

		// Left (bottom right to top left).
		start = new Vector(0, _tiledMap.HeightInPixels);
		end = new Vector(0, 0);
		_edges.Add(new PolyMap(start, end));
	}

	// EXP
	private void AddPolyMap()
	{
		Span<PolyMap> polyMap = _tiledMap.CreatePolyMap();

		foreach (PolyMap edge in polyMap)
			_edges.Add(new PolyMap(edge.Start, edge.End));
	}

	///	<summary>
	///	Adds a "fallback" boundary to the edge pool.
	/// "Less is more"
	///	</summary>
	private void AddFallbackBoundary() => AddPolygonalBoundary(3, 2);

	/// <summary>
	///	Adds a polygonal "fallback" boundary with "Illumer" radius.
	/// </summary>
	private void AddPolygonalBoundary(int edges = 3, float multiplier = 2)
	{
		float deltaTheta =  2 * MathF.PI / edges;
		float theta = 0;
		float radius = _illumer.Radius * multiplier;

		Vector start, end;

		// TODO: Optimize by caching / precalculating cosine and sine. - VE
		for (int i = 0; i < edges; i++)
		{
			start = new Vector(
				_illumer.Origin.X - radius * MathF.Cos(theta),
				_illumer.Origin.Y - radius * MathF.Sin(theta)
			);

			theta += deltaTheta;
			end = new Vector(
				_illumer.Origin.X - radius * MathF.Cos(theta),
				_illumer.Origin.Y - radius * MathF.Sin(theta)
			);

			_edges.Add(new PolyMap(start, end));
		}
	}

	private void EvaluateOccluders()
	{
		// Sort "Occluders" so we can start with nearest.
		_occluders.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);
	
		// Define corners in an anti-clockwise fashion,
		// and calculate the distances from "Illumer" to "Occluder" edge end-points.
		Span<(Vector Point, float Distance)> corners = stackalloc (Vector Point, float Distance)[4];

		// "Cone" angles
		Span<float> angle = stackalloc float[3];

		Span<Vector> triangle = stackalloc Vector[3];

		int count = 0;

		for (int i = 0; i < _occluders.Count; i++)
		{
			// Top left.
			corners[0].Point = _occluders[i].Position;
			corners[0].Distance = Vector.DistanceSquared(corners[0].Point, _illumer.Origin);

			// Top right.
			corners[1].Point = new Vector( _occluders[i].Position.X +  _occluders[i].Size.X,  _occluders[i].Position.Y);
			corners[1].Distance = Vector.DistanceSquared(corners[1].Point, _illumer.Origin);

			// Bottom right.
			corners[2].Point =  _occluders[i].Position +  _occluders[i].Size;
			corners[2].Distance = Vector.DistanceSquared(corners[2].Point, _illumer.Origin);

			// Bottom left.
			corners[3].Point = new Vector( _occluders[i].Position.X,  _occluders[i].Position.Y +  _occluders[i].Size.Y);
			corners[3].Distance = Vector.DistanceSquared(corners[3].Point, _illumer.Origin);

			// Sort edges by distance.
			corners.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);

			// Add the two nearest edges to edges pool.
			_edges.Add(new PolyMap(corners[0].Point, corners[1].Point));
			_edges.Add(new PolyMap(corners[0].Point, corners[2].Point));

			// Evaluate whether the "Occluder" occludes something.
			/*for (int j = i + 1; j < _occluders.Count; j++)
			{
				// Angles between "Illumer" origin to either side of "Occluder".
				angle[0] = Vector.Angle(_illumer.Origin, corners[1].Point);
				angle[1] = Vector.Angle(_illumer.Origin, corners[2].Point);
				angle[2] = Vector.Angle(_illumer.Origin, corners[0].Point);//_occluders[j].Position + _occluders[j].Size / 2);

				// "Cone" polygon.
				triangle[0] = _illumer.Origin;

				triangle[1] = new Vector(
					_illumer.Origin.X - _illumer.Radius * MathF.Cos(angle[0]),
					_illumer.Origin.Y - _illumer.Radius * MathF.Sin(angle[0])
				);

				triangle[2] = new Vector(
					_illumer.Origin.X - _illumer.Radius * MathF.Cos(angle[1]),
					_illumer.Origin.Y - _illumer.Radius * MathF.Sin(angle[1])
				);

				// Crude method for checking whether occluded is within triangle.
				Vector origin = _occluders[j].Position + _occluders[j].Size / 2;

				//bool intersects = false;
				//foreach (Vector point in triangle)
				//	intersects = Vector.Distance(point, origin) < _occluders[j].Radius ? true : false;

				if (
					Vector.Distance(triangle[0], origin) > _occluders[j].Radius &&
					Vector.Distance(triangle[1], origin) > _occluders[j].Radius &&
					Vector.Distance(triangle[2], origin) > _occluders[j].Radius
				) continue;


				_occluders.Remove(j);
				//_occluders.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);
				
			}*/
		}
	}

	private bool Visible(Vector origin, float radius)
	{
		Circle occluder = new Circle(origin, radius);
		Circle illumer = new Circle(_illumer.Origin, _illumer.Radius);

		// Seems to work. Cool effect :)
		return illumer.Intersects(occluder) ? true : false;
	}

	#endregion
}