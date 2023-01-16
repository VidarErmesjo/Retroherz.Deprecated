using System;
using MonoGame.Extended.Tiled;
namespace Retroherz.Systems;

public partial class ShadowsSystem
{
	/// <summary>
	///	Returns a read only span of cells initialized from a set of Tiled map tiles.
	/// </summary>
	internal ReadOnlySpan<PolyMapCell> ParseTiledMapTiles(TiledMapTile[] tiles)
	{
		Span<PolyMapCell> cells = new PolyMapCell[tiles.Length];

		for (int i = 0; i < cells.Length; i++)
			cells[i] = new()
			{
				Exists = !tiles[i].IsBlank ? true : false
			};

		return cells;
	}
}