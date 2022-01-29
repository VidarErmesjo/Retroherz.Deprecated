using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Entities;

using PubSub;

using Retroherz;
using Retroherz.Components;

namespace Retroherz.Systems
{
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

        private void RemoveTile(ushort x, ushort y) { _tiledMap.TileLayers[0].RemoveTile(x, y); }

        public TiledMapSystem(TiledMap tiledMap, GraphicsDevice graphics, OrthographicCamera camera)
        {
            _camera = camera;
            _spriteBatch = new SpriteBatch(graphics);
            _tiledMap = tiledMap;
            _tiledMapRenderer = new TiledMapRenderer(graphics, tiledMap);

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
                        payload.Tiles = _tiledMap.TileLayers[0].Tiles.Where(tile => !tile.IsBlank).ToArray();
                        break;

                    default:
                        break;                   
                }
            });
        }

        public virtual void Initialize(World world)
        {
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

    public class TiledMapSystemEvent : EventArgs
    {
        public TiledMapSystemAction Action { get; set; }
        public IEnumerable<TiledMapTile> Tiles { get; set; }
        public Vector2 Location { get; set; }

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