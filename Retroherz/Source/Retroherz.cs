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

        private GraphicsDeviceManager _graphics;

        private SpriteBatch _spriteBatch;

        private TiledMap _tiledMap;

        private World _world;

        private FastRandom _random;

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
            var position = Vector2.One * 16 * 7;//new Vector2(GameManager.VirtualResolution.Width / 2, GameManager.VirtualResolution.Height / 2);
            var size = new Size2(32, 32);
            var rectangle = new RectangleF(position, size);
            var circle = new CircleF(position, 8);
            var asepriteDocument = AssetsManager.Sprite("Shitsprite");//Content.Load<AsepriteDocument>("Aseprite/Shitsprite");

            var player = _world.CreateEntity();
            player.Attach(new PlayerComponent());
            player.Attach(new SpriteComponent(ref asepriteDocument));
            player.Attach(new ColliderComponent(rectangle));
            player.Attach(new RayComponent());
            player.Attach(new PhysicsComponent(position: position, size: size));

            return player.Id;
        }

        public int CreateActor()
        {
            var position = Vector2.One * 16 * 2;
            var size = new Size2(16f, 16f);
            var velocity = Vector2.One * _random.NextSingle(-1, 1) * 16;
            var rectangle = new RectangleF(position, size);
            var asepriteDocument = AssetsManager.Sprite("Shitsprite");

            var actor = _world.CreateEntity();
            actor.Attach(new SpriteComponent(ref asepriteDocument));
            actor.Attach(new ColliderComponent(rectangle));
            actor.Attach(new PhysicsComponent(position: position, velocity: velocity, size: size));

            return actor.Id;
        }

        public int CreateMap()
        {
            var map = _world.CreateEntity();
            map.Attach(new TiledMapComponent(_tiledMap));

            return map.Id;
        }

        protected override void Initialize()
        {
            base.Initialize();
            GameManager.Initialize();
            HUD.Initialize();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _random = new FastRandom((Math.Abs((int)DateTime.Now.Ticks)));
  
            _world = new WorldBuilder()
                .AddSystem(new PlayerSystem(GameManager.Camera))
                .AddSystem(new TiledMapSystem(_tiledMap, GraphicsDevice, GameManager.Camera))
                //.AddSystem(new ColliderSystem(_tiledMap))
                .AddSystem(new PhysicsSystem())
                .AddSystem(new RaySystem())
                .AddSystem(new RenderSystem(GraphicsDevice, GameManager.Camera))
                .Build();
            //_world.Initialize();
            //Components.Add(_world);

            var playerId = CreatePlayer();
            var actorId = CreateActor();
            //var mapId = CreateMap();

            GameManager.Camera.ZoomIn(6f);
            //GameManager.Camera.Rotate(MathHelper.Pi / 4);

            // Access TileMapLayers
            //var whatIs = _tiledMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");
            //var whatIs = _tiledMap.GetLayer("Tile Layer 1") as TiledMapTileLayer;
            //var whatIs = _tiledMap.TileLayers[0]; // can return null

            //whatIs.SetTile(1, 1, 1);
            

        }

        protected override void Update(GameTime gameTime)
        {
            if(GameManager.GamePadState.Buttons.Back == ButtonState.Pressed || GameManager.KeyboardState.IsKeyDown(Keys.Escape))
                Exit();
                
            GameManager.Update();
            //HUD.Update(gameTime);
            _world.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var scale = 1f / (GameManager.VirtualResolution.Height / GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height);
            //GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget((GameManager.LowResolution ? GameManager.VirtualRenderTarget : GameManager.DeviceRenderTarget));
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(GameManager.VirtualRenderTarget);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

            _spriteBatch.Begin();
            _world.Draw(gameTime);
            _spriteBatch.End();

            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

            _spriteBatch.Begin(                
                SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                effect: null);
            _spriteBatch.Draw(GameManager.VirtualRenderTarget, GameManager.DeviceRectangle, Color.White);
            _spriteBatch.End();

            //this.Draw(_spriteBatch);
            //HUD.Draw(_spriteBatch);

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
