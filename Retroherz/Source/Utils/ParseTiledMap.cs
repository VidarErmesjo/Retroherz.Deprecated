using System;
using System.Collections.Generic;
using MonoGame.Extended.Tiled;

namespace Retroherz;

public enum TiledMapType
{
	Empty,
	Solid
}

public struct Tile
{
	public ushort X, Y, Width, Height;
	public TiledMapType Type;

	public Tile(ushort x = 0, ushort y = 0, ushort width = 0, ushort height = 0, TiledMapType type = default(TiledMapType))
	{
		X = y;
		Y = y;
		Width = width;
		Height = height;
		Type = type;
	}
}

internal partial class Utils
{
	internal static ReadOnlySpan<Tile> ParseTiledMap(TiledMap tiledMap, int layer = 0)
	{
		var tiles = new Tile[tiledMap.Width * tiledMap.Height].AsSpan();
		for (ushort y = 0; y < tiledMap.Height; y++)
			for (ushort x = 0; x < tiledMap.Width; x++)
			{
				var index = y * tiledMap.Width + x;
				var tile = new Tile(
						x,
						y,
						((ushort) tiledMap.TileWidth),
						((ushort) tiledMap.TileHeight),
						tiledMap.TileLayers[layer].GetTile(x, y).IsBlank ? TiledMapType.Empty : TiledMapType.Solid);

				tiles[index] = tile;
			}

		return tiles;;
	}
}