using System;
using MonoGame.Extended.Tiled;
using Retroherz.Collections;
using Retroherz.Math;

namespace Retroherz.Visibility;

public ref struct BooleanMap
{
	private readonly Span<bool> _taken;
	private readonly int _width;
	public int Lenght => _taken.Length;

	public BooleanMap(int width, int height)
	{
		_taken = new bool[width * height];
		_taken.Fill(false);
		_width = width;
	}

	public bool this[int x, int y]
	{
		get => _taken[y * _width + x];
		set => _taken[y * _width + x] = value;
	}
}

///	<summary>
///	Represents a tile map. Based on <see cref="Liste" />.
///	</summary>
public ref struct TileMap
{
	private readonly Span<TiledMapTile> _tiles;

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

public struct Block
{
	public Vector Position = default(Vector);
	public Vector Size = default(Vector);

	public Block(Vector position, Vector size)
	{
		Position = position;
		Size = size;
	}
}

public enum TileType
{
	Empty,
	Solid
}

public static partial class VisibilityExtensions
{
	private static Sekk<Block> _blocks = new Sekk<Block>();

	private const ushort NORTH = 0;
	private const ushort EAST = 1;
	private const ushort SOUTH = 2;
	private const ushort WEST = 3;

	/// <summary>
	///	Convert Tiled map to "BlockMap".
	/// </summary>
	public static Sekk<Block> CreateBlockMap(this TiledMap tiledMap, string layerName) => CreateBlockMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to "BlockMap".
	/// </summary>
	public static Sekk<Block> CreateBlockMap(this TiledMap tiledMap, ushort layer = 0) => CreateBlockMap(
		tiledMap.TileLayers[layer].Tiles,
		tiledMap.TileLayers[layer].Width,
		tiledMap.TileLayers[layer].Height,
		tiledMap.TileLayers[layer].TileWidth,
		tiledMap.TileLayers[layer].TileHeight
	);

	private static Sekk<Block> CreateBlockMap(
		TiledMapTile[] tiles,
		int width,
		int height,
		int tileWidth,
		int tileHeight
	)
	{
		TileMap tileMap = new TileMap(tiles, width, height, tileWidth, tileHeight);
		Liste<bool> taken = new Liste<bool>(width, height);

		// Clear the "BlockMap".
		_blocks.Clear();

		Span<bool> exists = stackalloc bool[4];
		Span<Vector> block = stackalloc Vector[2];

		// Rules:
		// 1. Ignore all tiles with four neighbours.
		for (int y = 0; y < tileMap.Width; y++)
			for (int x = 0; x < tileMap.Height; x++)
			{
				// Ignore tile if it does not exist.
				if (tileMap[x, y].IsBlank)
					continue;

				// Inspect neighbours.	
				exists[NORTH] = !tileMap.IsBlank(x, y - 1);
				exists[EAST] = !tileMap.IsBlank(x + 1, y);
				exists[SOUTH] = !tileMap.IsBlank(x, y + 1);
				exists[WEST] = !tileMap.IsBlank(x - 1, y);

				// Ignore tile if it has all four neighbours.
				if (exists[NORTH] && exists[EAST] && exists[SOUTH] && exists[WEST])
					continue;

				// Create block with position and size.
				block[0] = new Vector(x * tileMap.TileWidth, y * tileMap.TileHeight);
				block[1] = new Vector(tileMap.TileWidth, tileMap.TileHeight);
				_blocks.Add(new Block(position: block[0], size: block[1]));
			}


		return _blocks;
	}
}