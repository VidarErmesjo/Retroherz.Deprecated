/*using System;
namespace MonoGame.Extended.Tiled;

/// <summary>
///	Extensions for MonoGame.Extended.Tiled
/// </summary>
public static partial class TiledExtensions
{
	/// <summary>
	///	Returns a read only span of cells initialized from a set of Tiled map tiles.
	/// </summary>
	internal static ReadOnlySpan<Cell> ParseTiledMapTiles(TiledMapTile[] tiles)
	{
		Span<Cell> cells = new Cell[tiles.Length];

		for (int i = 0; i < cells.Length; i++)
			cells[i] = new(
				exists: !tiles[i].IsBlank ? true : false
			);

		return cells;
	}
}*/