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

namespace Retroherz.Systems
{
    public class TiledMapSystem : IUpdateSystem, IDrawSystem
    {
        private bool _isDisposed = false;

        private readonly OrthographicCamera _camera;
        private readonly SpriteBatch _spriteBatch;
        private readonly TiledMapRenderer _tiledMapRenderer;
        private readonly TiledMap _tiledMap;

        public TiledMapSystem(TiledMap tiledMap, GraphicsDevice graphics, OrthographicCamera camera)
        {
            _camera = camera;
            _spriteBatch = new SpriteBatch(graphics);
            _tiledMapRenderer = new TiledMapRenderer(graphics, tiledMap);
            _tiledMap = tiledMap;
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
            this.Dispose(true);
        }        
    }
}