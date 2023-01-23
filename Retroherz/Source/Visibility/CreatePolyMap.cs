using System;
using System.Runtime.CompilerServices;
using System.Linq;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Math;

namespace Retroherz.Visibility;

public static partial class VisibilityExtensions
{
	private static Size _dimensions;
	private static Memory<PolyMapCell> _map = null;
	private static Memory<PolyMap> _edges  = null;
	private static int index = 0;

	public static ReadOnlySpan<PolyMap> PolyMap => _edges.Span.Slice(0, index);

	private struct PolyMapCell
	{
		public (int Id, bool Exists) Northern;
		public (int Id, bool Exists) Southern;
		public (int Id, bool Exists) Eastern;
		public (int Id, bool Exists) Western;
		public required bool Exists;
	}

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	public static Memory<PolyMap> CreatePolyMap(this TiledMap tiledMap, string layerName) => CreatePolyMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	public static Memory<PolyMap> CreatePolyMap(this TiledMap tiledMap, ushort layer = 0) => CreatePolyMap(
		tiledMap.TileLayers[layer].Tiles,
		tiledMap.TileLayers[layer].Width,
		tiledMap.TileLayers[layer].Height,
		tiledMap.TileLayers[layer].TileWidth,
		tiledMap.TileLayers[layer].TileHeight
	);

	// Courtesy of One Lone Coder - based on and modified by Vidar "Voidar" Ermesjø
	// https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp
	private static Memory<PolyMap> CreatePolyMap(
		TiledMapTile[] tiles,
		int width,
		int height,
		int tileWidth,
		int tileHeight
	)
	{
		// Reset or resize if needed
		if (_map.Length < tiles.Length)
			{
				_map = new PolyMapCell[tiles.Length];
				_dimensions = new(width, height);

				Console.WriteLine($"CreatePolyMap() => _map[{_map.Length}]");
			}
		else
			_map.Span.Clear();

		// Parse the tiles.
		int count = 0;
		for (int i = 0; i < tiles.Length; i++)
			if (!tiles[i].IsBlank)
			{
				_map.Span[i].Exists = true;
				count++;
			}

		// Reset "PolyMap" or resize if needed.
		if (_edges.Length < count)
		{
			_edges = new PolyMap[count];
			Console.WriteLine($"CreatePolyMap() => _edges[{_edges.Length}]");
		}
		else
			_edges.Span.Clear();

		// Reset the index.
		index = 0;	

		// Iterate through region from top left to bottom right.
		// TODO: Iterate through whole map without ArgumentOutOfRangeException
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
			{
				// sx, sy => start / offset of map
				int sx = 0;
				int sy = 0;

				// pitch = width of tile map
				int pitch = width;

				// Create some convenient indices (index = y * width + x).
				int current = (y + sy) * pitch + (x + sx);		// This
				int northern = (y + sy - 1) * pitch + (x + sx);	// Northern Neighbour
				int southern = (y + sy + 1) * pitch + (x + sx);	// Southern Neighbour
				int western = (y + sy) * pitch + (x + sx - 1);	// Western Neighbour
				int eastern = (y + sy) * pitch + (x + sx + 1);	// Eastern Neighbour

				// TODO: These steps can certainly be generalized?

				// If this cell exists, check if it needs edges.
				if (_map.Span[current].Exists)
				{
					// If this cell has NO WESTERN NEIGHBOUR, it NEEDS a WESTERN EDGE.
					//if (!_map.Span[!westernOutOfBounds ? western : 0].Exists || !westernOutOfBounds)
					if (!Neighbour(western).Exists)
					//if (!_map.Span[western].Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (Neighbour(northern).Western.Exists)
						{
							// NORTHERN neighbour HAS a western edge, so grow it DOWNWARDS.
							int id = _map.Span[northern].Western.Id;
							_edges.Span[id].End.Y += tileHeight;

							_map.Span[current].Western.Id = id;
							_map.Span[current].Western.Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x) * tileWidth, (sy + y) * tileHeight);
							Vector end = new(start.X, start.Y + tileHeight);

							_map.Span[current].Western = CreateEdge(start, end);
						}
					}

					// If this cell has NO EASTERN NEIGHBOUR, it NEEDS a EASTERN EDGE.
					if (!Neighbour(eastern).Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (Neighbour(northern).Eastern.Exists)
						{
							// NORTHERN neighbour HAS one, so grow it DOWNWARDS.
							int id = _map.Span[northern].Eastern.Id;
							_edges.Span[id].End.Y += tileWidth;

							_map.Span[current].Eastern.Id = id;
							_map.Span[current].Eastern.Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x + 1) * tileWidth, (sy + y) * tileHeight);
							Vector end = new(start.X, start.Y + tileHeight);

							_map.Span[current].Eastern = CreateEdge(start, end);
						}
					}

					// If this cell has NO NORTHERN NEIGHBOUR, it needs a NORTHERN EDGE.
					if (!Neighbour(northern).Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (Neighbour(western).Northern.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = _map.Span[western].Northern.Id;
							_edges.Span[id].End.X += tileWidth;

							_map.Span[current].Northern.Id = id;
							_map.Span[current].Northern.Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x) * tileWidth, (sy + y) * tileHeight);
							Vector end = new(start.X + tileWidth, start.Y);

							_map.Span[current].Northern = CreateEdge(start, end);
						}
					}

					// If this cell has NO SOUTHERN NEIGHBOUR, it needs a SOUTHERN EDGE.
					if (!Neighbour(southern).Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (Neighbour(western).Southern.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = _map.Span[western].Southern.Id;
							_edges.Span[id].End.X += tileWidth;

							_map.Span[current].Southern.Id = id;
							_map.Span[current].Southern.Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x) * tileWidth, (sy + y + 1) * tileHeight);
							Vector end = new(start.X + tileWidth, start.Y);

							_map.Span[current].Southern = CreateEdge(start, end);
						}
					}
				}
			}

		/*// Add border edges

		// Top (left-right)
		_edge = new();
		_edge.Start = new Vector(0, 0);
		_edge.End = new Vector(tileWidth * width, 0);
		__edges.Add(_edge);

		// Right (top-down)
		_edge = new();
		_edge.Start = new Vector(tileWidth * width, 0);
		_edge.End = new Vector(tileWidth * width, tileHeight * height);
		__edges.Add(_edge);

		// Bottom (right-left)
		_edge = new();
		_edge.Start = new Vector(tileWidth * width, tileHeight * height);
		_edge.End = new Vector(0, tileHeight * height);
		__edges.Add(_edge);

		// Left (bottom-up)
		_edge = new();
		_edge.Start = new Vector(0, tileHeight * height);
		_edge.End = new Vector(0, 0);
		__edges.Add(_edge);*/

		//var what = _edges.Span.Slice(0, index).ToArray().Distinct(new PolyMapComparer()).ToArray().AsSpan();
		//Console.WriteLine($"Edges:{_edges.Span.Slice(0, index).Length} Unique:{what.Length}");

		// Seems to work :)
		return _edges.Slice(0, index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (int Id, bool Exists) CreateEdge(Vector start, Vector end)
	{
		// Add edge to polygon pool.
		int id = index;
		_edges.Span[index] = new PolyMap(start, end);
		index++;

		// Update tile information with edge information.
		return (id, true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static PolyMapCell Neighbour(int index)
	{
 		bool outOfBounds = index < 0 || !(index < _map.Length) ? true : false;

		return outOfBounds ? new() { Exists = false } : _map.Span[index];
	}
}