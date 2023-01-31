using System;
using System.Runtime.CompilerServices;
using System.Linq;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Collections;
using Retroherz.Math;

namespace Retroherz.Visibility;

public struct Edge
{
	public Vector Start = default(Vector);
	public Vector End = default(Vector);

	public Edge(Vector start, Vector end)
	{
		Start = start;
		End = end;
	}
}

public static class PolyMap
{
	private static Sekk<Edge> _edges = new Sekk<Edge>();

	private struct Cell
	{
		public (int Id, bool Exists) Northern;
		public (int Id, bool Exists) Southern;
		public (int Id, bool Exists) Eastern;
		public (int Id, bool Exists) Western;
		public required bool Exists;
	}

	///	<summary>
	///	Represents a tile map. Based on <see cref="SpanArray" />.
	///	</summary>
	private ref struct TileMap
	{
		private readonly Span<TiledMapTile> _tiles;

		///	<summary>
		///	Returns the length of the array.
		///	<summary>
		public int Length => _tiles.Length;

		public readonly int Width;
		public readonly int Height;
		public readonly int TileWidth;
		public readonly int TileHeight;

		public TileMap()
		{
			Width = 0;
			Height = 0;
			TileWidth = 0;
			TileHeight = 0;
			_tiles = new Span<TiledMapTile>();
		}

		public TileMap(TiledMapTile[] tiles, int width, int height, int tileWidth, int tileHeight)
		{
			Width = width;
			Height = height;
			TileWidth = tileWidth;
			TileHeight = tileHeight;

			_tiles = tiles.AsSpan();
		}

		public TiledMapTile this[int x, int y]
		{
			get => _tiles[y * this.Width + x];
			set => _tiles[y * this.Width + x] = value;
		}

		public bool IsBlank(int x, int y)
		{
			// Return false if index out of bounds ("nothing is something").
			if (x < 0 || y < 0 || x > this.Width - 1 || y > this.Height - 1) return false;

			return this[x, y].IsBlank ? true : false;
		}
	}

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	public static Sekk<Edge> CreatePolyMap(this TiledMap tiledMap, string layerName) => CreatePolyMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	public static Sekk<Edge> CreatePolyMap(this TiledMap tiledMap, ushort layer = 0) => CreatePolyMap(
		tiledMap.TileLayers[layer].Tiles,
		tiledMap.TileLayers[layer].Width,
		tiledMap.TileLayers[layer].Height,
		tiledMap.TileLayers[layer].TileWidth,
		tiledMap.TileLayers[layer].TileHeight
	);

	// Courtesy of One Lone Coder - based on and modified by Vidar "Voidar" Ermesjø
	// https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp
	private static Sekk<Edge> CreatePolyMap(
		TiledMapTile[] tiles,
		int width,
		int height,
		int tileWidth,
		int tileHeight
	)
	{
		Span<Cell> map = stackalloc Cell[tiles.Length];

		//TileMap tileMap = new TileMap(tiles, width, height, tileWidth, tileHeight);

		// Parse the tiles.
		for (int i = 0; i < tiles.Length; i++)
			if (!tiles[i].IsBlank)
				map[i].Exists = true;

		// Clear the "PolyMap".
		_edges.Clear();

		// Edge vectors.
		Span<Vector> edge = stackalloc Vector[2];

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

							edge[0] = new Vector(_edges[id].Start.X, _edges[id].Start.Y);
							edge[1] = new Vector(_edges[id].End.X, _edges[id].End.Y + tileHeight);

							map[current].Western = ExtendEdge(id, start: edge[0], end: edge[1]);
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							edge[0] = new Vector((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge[1] = new Vector(edge[0].X, edge[0].Y + tileHeight);

							map[current].Western = CreateEdge(start: edge[0], end: edge[1]);
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

							edge[0] = new Vector(_edges[id].Start.X, _edges[id].Start.Y);
							edge[1] = new Vector(_edges[id].End.X, _edges[id].End.Y + tileWidth);

							map[current].Eastern = ExtendEdge(id, start: edge[0], end: edge[1]);
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							edge[0] = new Vector((sx + x + 1) * tileWidth, (sy + y) * tileHeight);
							edge[1] = new Vector(edge[0].X, edge[0].Y + tileHeight);

							map[current].Eastern = CreateEdge(start: edge[0], end: edge[1]);
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

							edge[0] = new Vector(_edges[id].Start.X, _edges[id].Start.Y);
							edge[1] = new Vector(_edges[id].End.X + tileWidth, _edges[id].End.Y);

							map[current].Northern = ExtendEdge(id, start: edge[0], end: edge[1]);
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							edge[0] = new Vector((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge[1] = new Vector(edge[0].X + tileWidth, edge[0].Y);

							map[current].Northern = CreateEdge(start: edge[0], end: edge[1]);
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

							edge[0] = new Vector(_edges[id].Start.X, _edges[id].Start.Y);
							edge[1] = new Vector(_edges[id].End.X + tileWidth, _edges[id].End.Y);

							map[current].Southern = ExtendEdge(id, start: edge[0], end: edge[1]);
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							edge[0] = new Vector((sx + x) * tileWidth, (sy + y + 1) * tileHeight);
							edge[1] = new Vector(edge[0].X + tileWidth, edge[0].Y);

							map[current].Southern = CreateEdge(start: edge[0], end: edge[1]);
						}
					}
				}
			}

		return _edges;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (int Id, bool Exists) CreateEdge(in Vector start, in Vector end)
	{
		// Add edge to polygon pool.
		int id = _edges.Count;
		_edges.Add(new Edge(start, end));

		// Update tile information with edge information.
		return (id, true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (int Id, bool Exists) ExtendEdge(in int id, in Vector start, in Vector end)
	{
		// Extend edge.
		_edges[id] = new Edge(start, end);

		// Update tile information with edge information.
		return (id, true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Cell Neighbour(this Span<Cell> map, in int index) => (
		(index < 0 || !(index < map.Length)) ? new Cell() { Exists = false } : map[index]
	);
}