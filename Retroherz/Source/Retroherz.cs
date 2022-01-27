using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

using MonoGame.Extended.Entities;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using MonoGame.Assets;
using Retroherz.Components;
using MonoGame.ShadowCasting;
using Retroherz.Systems;

// RetroHerzen ???
// Erm ???
// vErmNGN
namespace Retroherz
{
    public class Retroherz : Game
    {
        private bool _isDisposed = false;

        private SpriteBatch _spriteBatch;

        private TiledMap _tiledMap;
        private TiledMapRenderer _tileMapRenderer;

        private Entity _player;
        private World _world;

        public readonly AssetsManager AssetsManager;
        public readonly GameManager GameManager;
        public readonly HUD HUD;

        public Retroherz(Size resolution = default(Size), bool fullscreen = true)
        {
            AssetsManager = new AssetsManager(Content);
            GameManager = new GameManager(this, resolution, fullscreen);
            HUD = new HUD(this);
        }

        protected override void LoadContent()
        {
            AssetsManager.LoadContent();
            HUD.LoadContent();

            /* EXPERIMENTAL! */

            _tiledMap = Content.Load<TiledMap>("Tiled/Shitmap");

            //ShadowCasting.ShadowCasting.ConvertTileMapToPolyMap(_tiledMap);

            /* !EXPERIMENTAL */

            base.LoadContent();
        }

        // CreatePlayer(State state) ???
        public int CreatePlayer()
        {
            var position = new Vector2(GameManager.VirtualResolution.Width / 2, GameManager.VirtualResolution.Height / 2);
            var asepriteDocument = AssetsManager.Sprite("Shitsprite");//Content.Load<AsepriteDocument>("Aseprite/Shitsprite");

            _player = _world.CreateEntity();
            _player.Attach(new PlayerComponent());
            _player.Attach(new SpriteComponent(ref asepriteDocument));
            _player.Attach(new ColliderComponent(new RectangleF(position, new Size2(16f, 16f))));
            _player.Attach(new RectangularColliderComponent(position: position, size: new Size2(32, 32f)));
            _player.Attach(new PhysicsComponent(position: position));

            return _player.Id;
        }

        protected override void Initialize()
        {
            base.Initialize();
            GameManager.Initialize();
            HUD.Initialize();

            _tileMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Add Dispose() to all systems
            _world = new WorldBuilder()
                .AddSystem(new PlayerSystem(GameManager.Camera))
                .AddSystem(new ColliderSystem(_tiledMap))
                .AddSystem(new RenderSystem(GraphicsDevice, GameManager.Camera))
                .Build();
            Components.Add(_world);

            var playerId = CreatePlayer();
        }

        protected override void Update(GameTime gameTime)
        {
            if(GameManager.GamePadState.Buttons.Back == ButtonState.Pressed || GameManager.KeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            _tileMapRenderer.Update(gameTime);
                
            GameManager.Update();
            HUD.Update(gameTime);

            //_world.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget((GameManager.LowResolution ? GameManager.VirtualRenderTarget : GameManager.DeviceRenderTarget));
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

            _tileMapRenderer.Draw(GameManager.Camera.GetViewMatrix());

            //this.Draw(_spriteBatch);
            HUD.Draw(_spriteBatch);

            //_world.Draw(gameTime);
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws to device
        /// </summary>
        /// <remarks>   
        ///     Includes effects
        /// </remarks>
        /*private void Draw(SpriteBatch spriteBatch, Effect effect = null)
        {
           GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                effect: effect);

            spriteBatch.Draw(
                (GameManager.LowResolution ? GameManager.VirtualRenderTarget : GameManager.DeviceRenderTarget),
                GameManager.DeviceRectangle,
                Color.White);

            spriteBatch.End();
        }*/

        protected override void UnloadContent()
        {
            GameManager.UnloadContent();
            AssetsManager.UnloadContent();
            HUD.UnloadContent();
            base.UnloadContent();
            this.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
                _tileMapRenderer.Dispose();
                _spriteBatch.Dispose();
                _world.Dispose();
            }

            _isDisposed = true;
        }

        ~Retroherz()
        {
            this.Dispose(false);
        }
    }
}
