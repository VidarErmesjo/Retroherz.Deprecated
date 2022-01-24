using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

namespace MonoGame.ShadowCasting
{
    enum Direction
    {
        North,
        South,
        East,
        West
    }

    struct Edge
    {
        Vector2 start;
        Vector2 end;
    }

    struct Cell
    {
        int[] edgeId = new int[4];
        bool[] edgeExists = new bool[4];
        bool exists = false;
    }

    public static class ShadowCasting
    {
        public static void ConvertTileMapToPolyMap(TiledMap tileMap, ushort tileLayer = 0)
        {
            // Should be preallocated
            var tileArray = new Cell[tileMap.Width * tileMap.Height];
            var edges = new List<Edge>();


            var tiles = tileMap.TileLayers[tileLayer].Tiles.Where(x => !x.IsBlank);
            foreach(var tile in tiles)
            {
                var hasWest = !tileMap.TileLayers[tileLayer].GetTile(Convert.ToUInt16(tile.X - 1), tile.Y).IsBlank;
                var hasEast = !tileMap.TileLayers[tileLayer].GetTile(Convert.ToUInt16(tile.X + 1), tile.Y).IsBlank;
                var hasNorth = !tileMap.TileLayers[tileLayer].GetTile(tile.X, Convert.ToUInt16(tile.Y - 1)).IsBlank;
                var hasSouth = !tileMap.TileLayers[tileLayer].GetTile(tile.X, Convert.ToUInt16(tile.Y + 1)).IsBlank;

                if(hasWest && hasEast && hasNorth && hasSouth)
                System.Console.WriteLine("{0},{1} - {2}", tile.X, tile.Y, tile.GlobalIdentifier);
            }
        }        
    }
}