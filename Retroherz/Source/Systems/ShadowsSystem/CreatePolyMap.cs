using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Math;
using Retroherz.Systems;

namespace Retroherz.Systems;

public partial class ShadowsSystem
{
	private static Bag<PolyMapEdge> PolyMap = new(10000);
	private static Memory<PolyMapCell> Cells;

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	private ReadOnlySpan<PolyMapEdge> CreatePolyMap(TiledMap tiledMap, string layerName) => CreatePolyMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	private ReadOnlySpan<PolyMapEdge> CreatePolyMap(TiledMap tiledMap, ushort layer = 0) => CreatePolyMap(
		tiledMap.TileLayers[layer].Tiles,
		tiledMap.TileLayers[layer].Width,
		tiledMap.TileLayers[layer].Height,
		tiledMap.TileLayers[layer].TileWidth,
		tiledMap.TileLayers[layer].TileHeight
	);

	// Courtesy of One Lone Coder - based on and modified by Vidar "Voidar" Ermesjø
	// https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp
	private ReadOnlySpan<PolyMapEdge> CreatePolyMap(
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
		Span<PolyMapCell> cells = new(new PolyMapCell[tiles.Length]);
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
						if (cells[n].West.Exists)
						{
							// NORTHERN neighbour HAS a western edge, so grow it DOWNWARDS.
							int id = cells[n].West.Id;

							PolyMap[id].End.Y += tileHeight;
							cells[i].West.Id = id;
							cells[i].West.Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector(edge.Start.X, edge.Start.Y + tileHeight);

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].West.Id = id;
							cells[i].West.Exists = true;
						}
					}

					// If this cell has NO EASTERN NEIGHBOUR, it NEEDS a EASTERN EDGE.
					if (!cells[e].Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (cells[n].East.Exists)
						{
							// NORTHERN neighbour HAS one, so grow it DOWNWARDS.
							int id = cells[n].East.Id;

							PolyMap[id].End.Y += tileWidth;
							cells[i].East.Id = id;
							cells[i].East.Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector((sx + x + 1) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector(edge.Start.X, edge.Start.Y + tileHeight);

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].East.Id = id;
							cells[i].East.Exists = true;
						}
					}

					// If this cell has NO NORTHERN NEIGHBOUR, it needs a NORTHERN EDGE.
					if (!cells[n].Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (cells[w].North.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = cells[w].North.Id;

							PolyMap[id].End.X += tileWidth;

							cells[i].North.Id = id;
							cells[i].North.Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector(edge.Start.X + tileWidth, edge.Start.Y);

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].North.Id = id;
							cells[i].North.Exists = true;
						}
					}

					// If this cell has NO SOUTHERN NEIGHBOUR, it needs a SOUTHERN EDGE.
					if (!cells[s].Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (cells[w].South.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = cells[w].South.Id;

							PolyMap[id].End.X += tileWidth;
							cells[i].South.Id = id;
							cells[i].South.Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector((sx + x) * tileWidth, (sy + y + 1) * tileHeight);
							edge.End = new Vector(edge.Start.X + tileWidth, edge.Start.Y);

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);

							// Update tile information with edge information.
							cells[i].South.Id = id;
							cells[i].South.Exists = true;
						}
					}
				}
			}

		return PolyMap.AsReadOnlySpan();
	}
}