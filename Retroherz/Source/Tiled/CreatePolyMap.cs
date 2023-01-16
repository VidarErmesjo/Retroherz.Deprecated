using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using Retroherz.Math;
using Retroherz.Systems;

namespace MonoGame.Extended.Tiled;

/// <summary>
///	Extensions for MonoGame.Extended.Tiled
/// </summary>
public static partial class TiledExtensions
{
	// Convenient semantics
	private const int NORTH = 0;
	private const int SOUTH = 1;
	private const int EAST = 2;
	private const int WEST = 3;

	private static Bag<Segment> PolyMap = new(10000);
	private static Memory<Cell> Cells;

	internal struct Cell
	{
		public (int Id, bool Exists)[] Edge;
		public bool Exists;

		public Cell(bool exists)
		{
			Edge = new (int, bool)[4];
			Exists = exists;
		}
	}

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	public static ReadOnlySpan<Segment> CreatePolyMap(this TiledMap tiledMap, string layerName) => CreatePolyMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	public static ReadOnlySpan<Segment> CreatePolyMap(this TiledMap tiledMap, ushort layer = 0) => CreatePolyMap(
		tiledMap.TileLayers[layer].Tiles,
		tiledMap.TileLayers[layer].Width,
		tiledMap.TileLayers[layer].Height,
		tiledMap.TileLayers[layer].TileWidth,
		tiledMap.TileLayers[layer].TileHeight
	);

	// Courtesy of One Lone Coder - based on and modified by Vidar "Voidar" Ermesjø
	// https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp
	private static ReadOnlySpan<Segment> CreatePolyMap(
		TiledMapTile[] tiles,
		int width,
		int height,
		int tileWidth,
		int tileHeight
	)
	{
		// Clear "PolyMap"
		PolyMap.Clear();

		// Move to ShadowsSystem for static memory??
		// Reset cells
		Span<Cell> cells = new(new Cell[tiles.Length]);
		//Cells = new(ParseTiledMapTiles(tiles).ToArray()); Cells.Span[].

		// Parse the tiles
		ParseTiledMapTiles(tiles).CopyTo(cells);


		// Iterate through region from top left to bottom right.
		// TODO: Iterate through whole map without ArgumentOutOfRangeException
		for (int x = 1; x < width - 1; x++)
			for (int y = 1; y < height - 1; y++)
			{
				// sx, sy => start / offset of map
				int sx = 0;
				int sy = 0;

				// pitch = width of tile map
				int pitch = width;

				// Create some convenient indices (index = y * width + x).
				int i = (y + sy) * pitch + (x + sx);		// This
				int n = (y + sy - 1) * pitch + (x + sx);	// Northern Neighbour
				int s = (y + sy + 1) * pitch + (x + sx);	// Southern Neighbour
				int w = (y + sy) * pitch + (x + sx - 1);	// Western Neighbour
				int e = (y + sy) * pitch + (x + sx + 1);	// Eastern Neighbour

				// TODO: These steps can certainly be generalized?

				// If this cell exists, check if it needs edges.
				if (cells[i].Exists)
				{
					// If this cell has NO WESTERN NEIGHBOUR, it NEEDS a WESTERN EDGE.
					if (!cells[w].Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (cells[n].Edge[WEST].Exists)
						{
							// NORTHERN neighbour HAS a western edge, so grow it DOWNWARDS.
							int id = cells[n].Edge[WEST].Id;

							PolyMap[id].End.Y += tileHeight;
							cells[i].Edge[WEST].Id = id;
							cells[i].Edge[WEST].Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							Segment edge = new();
							edge.Start = new Vector2((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector2(edge.Start.X, edge.Start.Y + tileHeight);

							if (PolyMap.Contains(edge))
								Console.WriteLine("Duplicate!");

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].Edge[WEST].Id = id;
							cells[i].Edge[WEST].Exists = true;
						}
					}

					// If this cell has NO EASTERN NEIGHBOUR, it NEEDS a EASTERN EDGE.
					if (!cells[e].Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (cells[n].Edge[EAST].Exists)
						{
							// NORTHERN neighbour HAS one, so grow it DOWNWARDS.
							int id = cells[n].Edge[EAST].Id;

							PolyMap[id].End.Y += tileWidth;
							cells[i].Edge[EAST].Id = id;
							cells[i].Edge[EAST].Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							Segment edge = new();
							edge.Start = new Vector2((sx + x + 1) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector2(edge.Start.X, edge.Start.Y + tileHeight);

							if (PolyMap.Contains(edge))
								Console.WriteLine("Duplicate!");

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].Edge[EAST].Id = id;
							cells[i].Edge[EAST].Exists = true;
						}
					}

					// If this cell has NO NORTHERN NEIGHBOUR, it needs a NORTHERN EDGE.
					if (!cells[n].Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (cells[w].Edge[NORTH].Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = cells[w].Edge[NORTH].Id;

							PolyMap[id].End.X += tileWidth;

							cells[i].Edge[NORTH].Id = id;
							cells[i].Edge[NORTH].Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							Segment edge = new();
							edge.Start = new Vector2((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector2(edge.Start.X + tileWidth, edge.Start.Y);

							if (PolyMap.Contains(edge))
								Console.WriteLine("Duplicate!");

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].Edge[NORTH].Id = id;
							cells[i].Edge[NORTH].Exists = true;
						}
					}

					// If this cell has NO SOUTHERN NEIGHBOUR, it needs a SOUTHERN EDGE.
					if (!cells[s].Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (cells[w].Edge[SOUTH].Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = cells[w].Edge[SOUTH].Id;

							PolyMap[id].End.X += tileWidth;
							cells[i].Edge[SOUTH].Id = id;
							cells[i].Edge[SOUTH].Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							Segment edge = new();
							edge.Start = new Vector2((sx + x) * tileWidth, (sy + y + 1) * tileHeight);
							edge.End = new Vector2(edge.Start.X + tileWidth, edge.Start.Y);

							if (PolyMap.Contains(edge))
								Console.WriteLine("Duplicate!");

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].Edge[SOUTH].Id = id;
							cells[i].Edge[SOUTH].Exists = true;
						}
					}
				}
			}

		return PolyMap.AsReadOnlySpan();
	}
}