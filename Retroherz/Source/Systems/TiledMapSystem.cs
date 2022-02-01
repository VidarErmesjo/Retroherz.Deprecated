using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Entities;

using PubSub;

using Retroherz.Components;

namespace Retroherz.Systems
{
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

    public enum TiledMapType
    {
        Empty,
        Solid
    }

    public enum TiledMapSystemAction
    {
        Default,
        RemoveTile,
        GetTiles,
    }

    public class TiledMapSystem : IUpdateSystem, IDrawSystem
    {
        private bool _isDisposed = false;

        Hub hub = Hub.Default;

        private readonly OrthographicCamera _camera;
        private readonly SpriteBatch _spriteBatch;
        private readonly TiledMapRenderer _tiledMapRenderer;
        private readonly TiledMap _tiledMap;
        private readonly Dictionary<int, Tile> _tiles;
        //private readonly TiledMapType[] _tiles;
        private ComponentMapper<TiledMapComponent> _tiledMapComponentMapper;

        private void RemoveTile(ushort x, ushort y) { _tiledMap.TileLayers[0].RemoveTile(x, y); }

        public TiledMap TiledMap { get => _tiledMap; }

        public TiledMapSystem(TiledMap tiledMap, GraphicsDevice graphics, OrthographicCamera camera)
        {
            _camera = camera;
            _spriteBatch = new SpriteBatch(graphics);
            _tiledMap = tiledMap;
            _tiledMapRenderer = new TiledMapRenderer(graphics, tiledMap);

            //_tiles = new TiledMapType[_tiledMap.Width * _tiledMap.Height];
            _tiles = new Dictionary<int, Tile>(_tiledMap.Width * _tiledMap.Height);

            // Listen to events
            hub.Subscribe<TiledMapSystemEvent>(this, payload => {
                // Convert to tile space
                var location = Vector2.Floor(payload.Location / new Vector2(
                    _tiledMap.TileWidth,
                    _tiledMap.TileHeight));

                // Check bounds
                var x = ((ushort)MathHelper.Clamp(location.X, ushort.MinValue, ushort.MaxValue));
                var y = ((ushort)MathHelper.Clamp(location.Y, ushort.MinValue, ushort.MaxValue));

                switch (payload.Action)
                {
                    case TiledMapSystemAction.RemoveTile:
                        RemoveTile(x, y);
                        _tiledMapRenderer.LoadMap(_tiledMap);
                        break;

                    case TiledMapSystemAction.GetTiles:
                        payload.TileWidth = _tiledMap.TileWidth;
                        payload.TileHeight = _tiledMap.TileHeight;
                        break;

                    default:
                        break;                   
                }
            });
        }

        public virtual void Initialize(World world)
        {
            // Parse TiledMap
            for (ushort y = 0; y < _tiledMap.Height; y++)
                for (ushort x = 0; x < _tiledMap.Width; x++)
                {
                    var index = y * _tiledMap.Width + x;
                    var tile = _tiledMap.TileLayers[0].GetTile(x, y);
                    _tiles.Add(
                        index,
                        new Tile(
                            x,
                            y,
                            ((ushort)_tiledMap.TileWidth),
                            ((ushort)_tiledMap.TileHeight),
                            tile.IsBlank ? TiledMapType.Empty : TiledMapType.Solid));
                }
            //var tiledMap = world.GetEntity(tiledMap.Id).Get<TiledMapComponent>();
            //_tiledMapRenderer.LoadMap(t.TiledMap);
            //_tiledMapComponentMapper = mapperService.GetMapper<TiledMapComponent>();

            foreach (var tile in _tiles) if (tile.Value.Type == TiledMapType.Solid)
                System.Console.WriteLine("{0}, {1}, {2}, {3}", tile.Key, tile.Value.X, tile.Value.Y, tile.Value.Type);

            //_tiledMapRenderer.LoadMap(_tiledMapComponentMapper.t().TiledMap);
            //var payload = new TiledMapSystemEvent(TiledMapSystemAction.GetTiles);
            //hub.Publish<TiledMapSystemEvent>(payload);
            hub.Publish(new TileMap(_tiles));
        }

        public virtual void Update(GameTime gameTime)
        {
            _tiledMapRenderer.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime)
        {
            _tiledMapRenderer.Draw(_camera.GetViewMatrix());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
                _spriteBatch.Dispose();
                _tiledMapRenderer.Dispose();
            }

            _isDisposed = true;
        }

        ~TiledMapSystem()
        {
            hub.Unsubscribe<TiledMapSystemEvent>();
            this.Dispose(true);
        }
    }

    public class TileMap
    {
        public Dictionary<int, Tile> Tiles { get; }

        public TileMap(Dictionary<int, Tile> tiles) { Tiles = tiles; }
    }

    public class TiledMapSystemEvent
    {
        public TiledMapSystemAction Action { get; set; }
        public TiledMap TiledMap { get; set; }
        public int TileWidth { get ; set; }
        public int TileHeight { get; set; }
        public Vector2 Location { get; }

        public TiledMapSystemEvent(
            TiledMapSystemAction action = default(TiledMapSystemAction),
            Vector2 location = default(Vector2))
        {
            Action = action;
            Location = location;
        }

        ~TiledMapSystemEvent() {}
    }
}