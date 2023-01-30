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
///	Represents a tile map. Based on <see cref="SpanArray" />.
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

		_tiles =  new Span<TiledMapTile>(tiles);
	}

	public TiledMapTile this[int x, int y]
	{
		get => _tiles[y * this.Width + x];
		set => _tiles[y * this.Width + x] = value;
	}
}

public struct Block
{
	public Vector Position = new();// default(Vector);
	public Vector Size = default(Vector);

	public Block(Vector position, Vector size)
	{
		Position = position;
		Size = size;
	}
}

public struct BlockMap
{
	//public Sekk<(Vector Position, Vector Size)> Blocks;

	//public BlockMap(int size) => Blocks = new Sekk<(Vector Position, Vector Size)>(size);
}

public enum TileType
{
	Empty,
	Solid
}

public struct Region
{
	public Vector First = default(Vector);
	public Vector Last = default(Vector);
	//public TileType Type;
	public bool IsBlank = false;

	public Region(Vector first, Vector last, bool isBlank)
	{
		First = first;
		Last = last;
		IsBlank = isBlank;
	}
}

public static partial class VisibilityExtensions
{
	private static Sekk<Block> _blocks = new Sekk<Block>();
	private static Sekk<Region> _regions = new Sekk<Region>();

	/// <summary>
	///	Convert Tiled map to "BlockMap".
	/// </summary>
	public static Sekk<BlockMap> CreateBlockMap(this TiledMap tiledMap, string layerName) => CreateBlockMap(
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Tiles,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Width,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).Height,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileWidth,
		tiledMap.GetLayer<TiledMapTileLayer>(layerName).TileHeight
	);

	/// <summary>
	///	Convert Tiled map to "BlockMap".
	/// </summary>
	public static Sekk<BlockMap> CreateBlockMap(this TiledMap tiledMap, ushort layer = 0) => CreateBlockMap(
		tiledMap.TileLayers[layer].Tiles,
		tiledMap.TileLayers[layer].Width,
		tiledMap.TileLayers[layer].Height,
		tiledMap.TileLayers[layer].TileWidth,
		tiledMap.TileLayers[layer].TileHeight
	);

	private static Sekk<BlockMap> CreateBlockMap(
		TiledMapTile[] tiles,
		int width,
		int height,
		int tileWidth,
		int tileHeight
	)
	{
		TileMap tileMap = new TileMap(tiles, width, height, tileWidth, tileHeight);
		SpanArray<bool> taken = new SpanArray<bool>(width, height);

		// Clear the "BlockMap".
		_blocks.Clear();

		for (int y = 0; y < tileMap.Width; ++y)
			for (int x = 0; x < tileMap.Height; ++x)
			{
				if (taken[x, y] || tileMap[x, y].IsBlank)
					continue;

				// Here we go.
				(int X, int Y) first, last;
				bool isBlank = false;

				// Start region from here.
				last = first = (x, y);
				isBlank = tileMap[x, y].IsBlank;
				taken[x, y] = true;

				// Search right as far as we can go.
				for (int i = x; i < tileMap.Width; ++i)
				{
					// Update last if the tile type is free and of the same brand.
					if (tileMap[i, y].IsBlank == tileMap[x, y].IsBlank && !taken[i, y])
					{
						last = (i, y);
						taken[i, y] = true;
					}
					else
					{
						// Rightwards expansion has ended. Now search row below.
						for (int j = y; j < tileMap.Height; ++j)
						{
						//check then row below the previously added row
                        //from rc->first_tile.x to rc->last_tile.x on the
                        // y + y2 line. If all are same then make each
                        // of them taken and make the y + y2 as rc->last_tile.y
                        // if there's a difference then break

							// ... as far as we got.
							if (taken[i, j])
								break;

							for (int k = first.X; k < last.X; ++k)
							{
								if (tileMap[k, j].IsBlank == isBlank)
								{
									last.Y = j;
									taken[k, j] = true;
								}
								else break;
							}
						}

						break;
					}
				}

				// Add what we got!
				if (!isBlank)
					_regions.Add(new Region(first, last, true));
			}

		// Assemble the "BlockMap".

		foreach (Region region in _regions)
			_blocks.Add(new Block(region.First, region.Last - region.First));

		foreach (Block block in _blocks)
			Console.WriteLine($"Position:{block.Position} Size:{block.Size}");

		return new();
	}
}