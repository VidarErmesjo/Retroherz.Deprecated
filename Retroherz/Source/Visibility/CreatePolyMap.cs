using System;
using System.Runtime.CompilerServices;
using System.Linq;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Collections;
using Retroherz.Math;

namespace Retroherz.Visibility;

public static partial class VisibilityExtensions
{
	private static Sekk<PolyMap> _edges = new Sekk<PolyMap>();

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
	public static Sekk<PolyMap> CreatePolyMap(this TiledMap tiledMap, string layerName) => CreatePolyMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	public static Sekk<PolyMap> CreatePolyMap(this TiledMap tiledMap, ushort layer = 0) => CreatePolyMap(
		tiledMap.TileLayers[layer].Tiles,
		tiledMap.TileLayers[layer].Width,
		tiledMap.TileLayers[layer].Height,
		tiledMap.TileLayers[layer].TileWidth,
		tiledMap.TileLayers[layer].TileHeight
	);

	// Courtesy of One Lone Coder - based on and modified by Vidar "Voidar" Ermesjø
	// https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp
	private static Sekk<PolyMap> CreatePolyMap(
		TiledMapTile[] tiles,
		int width,
		int height,
		int tileWidth,
		int tileHeight
	)
	{
		Span<PolyMapCell> map = stackalloc PolyMapCell[tiles.Length];

		// Parse the tiles.
		int count = 0;
		for (int i = 0; i < tiles.Length; i++)
			if (!tiles[i].IsBlank)
			{
				map[i].Exists = true;
				count++;
			}

		// Reset "PolyMap" or resize if needed.
		if (_edges.Count < count)
			_edges = new Sekk<PolyMap>(count);
		else
			_edges.Clear();

		// Iterate through region from top left to bottom right.
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

				// If this cell exists, check if it needs edges.
				if (map[current].Exists)
				{
					// If this cell has NO WESTERN NEIGHBOUR, it NEEDS a WESTERN EDGE.
					if (!map.Neighbour(western).Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (map.Neighbour(northern).Western.Exists)
						{
							// NORTHERN neighbour HAS a western edge, so grow it DOWNWARDS.
							int id = map[northern].Western.Id;

							Vector start = new(_edges[id].Start.X, _edges[id].Start.Y);
							Vector end = new(_edges[id].End.X, _edges[id].End.Y + tileHeight);

							map[current].Western = ExtendEdge(id, start, end);
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x) * tileWidth, (sy + y) * tileHeight);
							Vector end = new(start.X, start.Y + tileHeight);

							map[current].Western = CreateEdge(start, end);
						}
					}

					// If this cell has NO EASTERN NEIGHBOUR, it NEEDS a EASTERN EDGE.
					if (!map.Neighbour(eastern).Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (map.Neighbour(northern).Eastern.Exists)
						{
							// NORTHERN neighbour HAS one, so grow it DOWNWARDS.
							int id = map[northern].Eastern.Id;

							Vector start = new(_edges[id].Start.X, _edges[id].Start.Y);
							Vector end = new(_edges[id].End.X, _edges[id].End.Y + tileWidth);

							map[current].Eastern = ExtendEdge(id, start, end);
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x + 1) * tileWidth, (sy + y) * tileHeight);
							Vector end = new(start.X, start.Y + tileHeight);

							map[current].Eastern = CreateEdge(start, end);
						}
					}

					// If this cell has NO NORTHERN NEIGHBOUR, it needs a NORTHERN EDGE.
					if (!map.Neighbour(northern).Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (map.Neighbour(western).Northern.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = map[western].Northern.Id;

							Vector start = new(_edges[id].Start.X, _edges[id].Start.Y);
							Vector end = new(_edges[id].End.X + tileWidth, _edges[id].End.Y);

							map[current].Northern = ExtendEdge(id, start, end);
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x) * tileWidth, (sy + y) * tileHeight);
							Vector end = new(start.X + tileWidth, start.Y);

							map[current].Northern = CreateEdge(start, end);
						}
					}

					// If this cell has NO SOUTHERN NEIGHBOUR, it needs a SOUTHERN EDGE.
					if (!map.Neighbour(southern).Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (map.Neighbour(western).Southern.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = map[western].Southern.Id;

							// ExtendEdge() ??
							Vector start = new(_edges[id].Start.X, _edges[id].Start.Y);
							Vector end = new(_edges[id].End.X + tileWidth, _edges[id].End.Y);

							map[current].Southern = ExtendEdge(id, start, end);
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							Vector start = new((sx + x) * tileWidth, (sy + y + 1) * tileHeight);
							Vector end = new(start.X + tileWidth, start.Y);

							map[current].Southern = CreateEdge(start, end);
						}
					}
				}
			}

		return _edges;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (int Id, bool Exists) CreateEdge(Vector start, Vector end)
	{
		// Add edge to polygon pool.
		int id = _edges.Count;
		_edges.Add(new PolyMap(start, end));

		// Update tile information with edge information.
		return (id, true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (int Id, bool Exists) ExtendEdge(int id, Vector start, Vector end)
	{
		// Extend edge.
		_edges[id] = new PolyMap(start, end);

		// Update tile information with edge information.
		return (id, true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static PolyMapCell Neighbour(this Span<PolyMapCell> map, int index) => (
		(index < 0 || !(index < map.Length)) ? new PolyMapCell() { Exists = false } : map[index]
	);
}