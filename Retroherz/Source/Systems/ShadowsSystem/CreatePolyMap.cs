using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using Retroherz.Math;
using Retroherz.Systems;

namespace Retroherz.Systems;

/// <summary>
///	Extensions for MonoGame.Extended.Tiled
/// </summary>
public partial class ShadowsSystem
{
	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	internal ReadOnlySpan<PolyMapEdge> CreatePolyMap(TiledMap tiledMap, string layerName) => CreatePolyMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to polygon map.
	/// </summary>
	internal ReadOnlySpan<PolyMapEdge> CreatePolyMap(TiledMap tiledMap, ushort layer = 0) => CreatePolyMap(
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

		// Reset cells
		Cells.Span.Clear(); // Not needed?

		// Parse the tiles (TODO: should be only done when needed)
		for (int i = 0; i < tiles.Length; i++)
			Cells.Span[i].Exists = !tiles[i].IsBlank ? true : false;
			/*Cells.Span[i] = new(
				exists: !tiles[i].IsBlank ? true : false
			);*/

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
				int current = (y + sy) * pitch + (x + sx);		// This
				int northern = (y + sy - 1) * pitch + (x + sx);	// Northern Neighbour
				int southern = (y + sy + 1) * pitch + (x + sx);	// Southern Neighbour
				int western = (y + sy) * pitch + (x + sx - 1);		// Western Neighbour
				int eastern = (y + sy) * pitch + (x + sx + 1);		// Eastern Neighbour

				// TODO: These steps can certainly be generalized?

				// If this cell exists, check if it needs edges.
				if (Cells.Span[current].Exists)
				{
					// If this cell has NO WESTERN NEIGHBOUR, it NEEDS a WESTERN EDGE.
					if (!Cells.Span[western].Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (Cells.Span[northern].Western.Exists)
						{
							// NORTHERN neighbour HAS a western edge, so grow it DOWNWARDS.
							int id = Cells.Span[northern].Western.Id;

							PolyMap[id].End.Y += tileHeight;
							//Edges.Span[id].End.Y += tileHeight;
							Cells.Span[current].Western.Id = id;
							Cells.Span[current].Western.Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector2((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector2(edge.Start.X, edge.Start.Y + tileHeight);

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);
							//Edges.Span[id] = edge;

							// Update tile information with edge information.
							Cells.Span[current].Western.Id = id;
							Cells.Span[current].Western.Exists = true;
						}
					}

					// If this cell has NO EASTERN NEIGHBOUR, it NEEDS a EASTERN EDGE.
					if (!Cells.Span[eastern].Exists)
					{
						// It can either extend it from its NORTHERN neighbour if they have
						// one, or It can start a new one.
						if (Cells.Span[northern].Eastern.Exists)
						{
							// NORTHERN neighbour HAS one, so grow it DOWNWARDS.
							int id = Cells.Span[northern].Eastern.Id;

							PolyMap[id].End.Y += tileWidth;
							//Edges.Span[id].End.Y += tileWidth;
							Cells.Span[current].Eastern.Id = id;
							Cells.Span[current].Eastern.Exists = true;
						}
						else
						{
							// NORTHERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector2((sx + x + 1) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector2(edge.Start.X, edge.Start.Y + tileHeight);

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);
							//Edges.Span[id] = edge;

							// Update tile information with edge information.
							Cells.Span[current].Eastern.Id = id;
							Cells.Span[current].Eastern.Exists = true;
						}
					}

					// If this cell has NO NORTHERN NEIGHBOUR, it needs a NORTHERN EDGE.
					if (!Cells.Span[northern].Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (Cells.Span[western].Northern.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = Cells.Span[western].Northern.Id;

							PolyMap[id].End.X += tileWidth;
							//Edges.Span[id].End.X += tileWidth;

							Cells.Span[current].Northern.Id = id;
							Cells.Span[current].Northern.Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector2((sx + x) * tileWidth, (sy + y) * tileHeight);
							edge.End = new Vector2(edge.Start.X + tileWidth, edge.Start.Y);

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);
							//Edges.Span[id] = edge;

							// Update tile information with edge information.
							Cells.Span[current].Northern.Id = id;
							Cells.Span[current].Northern.Exists = true;
						}
					}

					// If this cell has NO SOUTHERN NEIGHBOUR, it needs a SOUTHERN EDGE.
					if (!Cells.Span[southern].Exists)
					{
						// It can either extend it from its WESTERN neighbour if they have
						// one, or It can start a new one.
						if (Cells.Span[western].Southern.Exists)
						{
							// WESTERN neighbour HAS one, so grow it EASTWARDS.
							int id = Cells.Span[western].Southern.Id;

							PolyMap[id].End.X += tileWidth;
							//Edges.Span[id].End.X += tileWidth;
							Cells.Span[current].Southern.Id = id;
							Cells.Span[current].Southern.Exists = true;
						}
						else
						{
							// WESTERN neighbour DOES NOT have one, so create one.
							PolyMapEdge edge = new();
							edge.Start = new Vector2((sx + x) * tileWidth, (sy + y + 1) * tileHeight);
							edge.End = new Vector2(edge.Start.X + tileWidth, edge.Start.Y);

							if (PolyMap.Contains(edge))
								Console.WriteLine("Duplicate!");

							// Add edge to polygon pool.
							int id = PolyMap.Count;
							PolyMap.Add(edge);
							//Edges.Span[id] = edge;

							// Update tile information with edge information.
							Cells.Span[current].Southern.Id = id;
							Cells.Span[current].Southern.Exists = true;
						}
					}
				}
			}

		return PolyMap.AsReadOnlySpan();
	}
}