using System.Collections.Generic;
using MonoGame.Extended.Tiled;
namespace Retroherz
{
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
		internal static List<Tile> ParseTiledMap(TiledMap tiledMap)
		{
			var tiles = new List<Tile>();
			for (ushort y = 0; y < tiledMap.Height; y++)
				for (ushort x = 0; x < tiledMap.Width; x++)
				{
					var index = y * tiledMap.Width + x;
					var tile = tiledMap.TileLayers[0].GetTile(x, y);
					tiles.Add(new Tile(
							x,
							y,
							((ushort) tiledMap.TileWidth),
							((ushort) tiledMap.TileHeight),
							tile.IsBlank ? TiledMapType.Empty : TiledMapType.Solid));
				}

			return tiles;
		}
	}
}