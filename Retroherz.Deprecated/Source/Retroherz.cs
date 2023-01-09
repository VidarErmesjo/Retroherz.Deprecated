using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

using Arch;
using Arch.Core;
using Arch.Core.CommandBuffer;
using Arch.Core.Extensions;
using Arch.Core.Utils;

using MonoGame.Extended.Entities;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using MonoGame.Assets;
using Retroherz.Components;
using Retroherz.Systems;

// RetroHerzen ???
// Erm ???
// vErmNGN
namespace Retroherz
{
	public static class ECS
	{

		public static CameraSystem CameraSystem;
		public static TiledMapSystem TiledMapSystem;
	}

    public class Retroherz : Game
    {
        private bool _isDisposed = false;

		private Arch.Core.World World;
        private MonoGame.Extended.Entities.World _world;
		
        public readonly AssetsManager AssetsManager;
        public readonly GameManager GameManager;

		public InputManager InputManager;

		private OrthographicCamera Camera;
        private SpriteBatch SpriteBatch;
		private TiledMap TiledMap;

        public Retroherz(Size resolution = default(Size), bool fullscreen = true)
        {
            AssetsManager = new AssetsManager(Content);
            GameManager = new GameManager(this, resolution, fullscreen);

			// Refactor to make instances here!
			//Camera = new(GameManager.GraphicsDeviceManager.GraphicsDevice);
			//InputManager = new(Camera);
        }

        protected override void LoadContent()
        {
            AssetsManager.LoadContent();

            /* EXPERIMENTAL! */

            TiledMap = Content.Load<TiledMap>("Tiled/Shitmap");

           //ShadowCasting.ShadowCasting.ConvertTileMapToPolyMap(_tiledMap);


            /* !EXPERIMENTAL */

            base.LoadContent();
        }

        public void CreatePlayer()
        {
            var position = Vector2.One * 16 * 7;
			position.Y += 8;
            var size = new Size2(16, 16);
            var rectangle = new RectangleF(position, size);
            var circle = new CircleF(position, 8);
            var asepriteDocument = AssetsManager.Sprite("Shitsprite");//Content.Load<AsepriteDocument>("Aseprite/Shitsprite");

            var player = _world.CreateEntity();
			player.Attach(new MetaComponent(id: player.Id, type: MetaComponentType.Player));
            player.Attach(new PlayerComponent());
            player.Attach(new SpriteComponent(asepriteDocument));
            player.Attach(new TransformComponent(position: position));
            player.Attach(new ColliderComponent(size: size, type: ColliderComponentType.Dynamic));
			player.Attach(new RayComponent(radius: 64));
        }

        public void CreateActors(int amount = 1)
        {
			var spawned = new HashSet<int>();
			var tiles = Utils.ParseTiledMap(TiledMap).Where(tile => tile.Type == TiledMapType.Empty).ToList();

			for (int i = 0; i < amount; i++ )
			{
				var minSize = 8;
				var maxSize = 16;
				var randomSize = Math.Clamp(Random.Shared.NextSingle() * maxSize, minSize, maxSize);

				var spawnTile = ((int)Math.Floor(Random.Shared.NextSingle() * tiles.Count));
				if (spawned.Contains(spawnTile))
					i--;
				else
					spawned.Add(spawnTile);

				var position = new Vector2(tiles[spawnTile].X * TiledMap.TileWidth, tiles[spawnTile].Y * TiledMap.TileHeight);//Vector2.One * 16 * 2;
				var size = new Size2(randomSize, randomSize);
				var velocity = Vector2.Zero;
				Random.Shared.NextUnitVector(out velocity);
				velocity = Vector2.Clamp(velocity, -Vector2.One, Vector2.One) * size;
				var rectangle = new RectangleF(position, size);
				var asepriteDocument = AssetsManager.Sprite("Shitsprite");

				var actor = _world.CreateEntity();
				System.Console.WriteLine("Spawned: {0}", position);
				actor.Attach(new ActorComponent());
				actor.Attach(new MetaComponent(id: actor.Id, type: MetaComponentType.NPC));
				actor.Attach(new SpriteComponent(asepriteDocument));
				actor.Attach(new TransformComponent(position: position));
				actor.Attach(new ColliderComponent(velocity: velocity, size: size, type: ColliderComponentType.Dynamic));
				actor.Attach(new RayComponent(radius: randomSize * 2));
			}
        }

        protected override void Initialize()
        {
            base.Initialize();
            GameManager.Initialize();

			Camera = new(GraphicsDevice);
            SpriteBatch = new SpriteBatch(GraphicsDevice);

			InputManager = new(Camera);
			InputManager.Initialize();

			// Arch START
			// Bytte fra Extended ESC til Arch? :)
			World = Arch.Core.World.Create();

			// COMPONENT (struct over class)
			var entity = World.Create(new Vector2(0, 0));
			System.Console.WriteLine(entity.Get<Vector2>());

			if (entity.Has<Vector2>())
				entity.Set(new Vector2(4, 4));
			System.Console.WriteLine(entity.Get<Vector2>());

			// SYSTEM
			var query = new QueryDescription().WithAll<Vector2>();
			World.Query(in query, (in Arch.Core.Entity e) => {
				e.Set(new Vector2(3, 3));
			});
			World.Query(in query, (ref Vector2 vector) => {
				vector = Vector2.One * 2;
			});
			System.Console.WriteLine(entity.Get<Vector2>());

			Arch.Core.World.Destroy(World);

			// Arch END

            _world = new WorldBuilder()
                .AddSystem(new TiledMapSystem(TiledMap, GraphicsDevice, Camera))
                .AddSystem(new ExpirySystem())
                .AddSystem(new PlayerSystem(Camera, InputManager))
				.AddSystem(new SelectSystem(InputManager))
				.AddSystem(new RaySystem())
                .AddSystem(new ColliderSystem())
                .AddSystem(new UpdateSystem(InputManager))
                .AddSystem(new RenderSystem(GraphicsDevice, Camera))
				.AddSystem(new DebugSystem(GraphicsDevice, Camera))
				.AddSystem(new CameraSystem(Camera, TiledMap))
				.AddSystem(new MetaSystem())
				.AddSystem(new HUDSystem(GraphicsDevice, Camera, InputManager))
                .Build();
            _world.Initialize();

			//EntityManager.Initialize(_world);
            //Components.Add(_world);

			var coolNames = new List<string> {"Halsbrann Sivertsen", "Salman Rushtid"};

			CreatePlayer();
			//CreateActors(50);

            Camera.ZoomIn(6f);
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
			InputManager.Update(gameTime);
            _world.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(GameManager.VirtualRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            _world.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);

            SpriteBatch.Begin(                
                SpriteSortMode.FrontToBack,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                effect: null);
            SpriteBatch.Draw(GameManager.VirtualRenderTarget, GameManager.DeviceRectangle, Color.White);
            SpriteBatch.End();

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
            base.UnloadContent();
            this.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
                SpriteBatch.Dispose();
                _world.Dispose();
				//GraphicsDevice.Dispose();
            }

            _isDisposed = true;
        }

        ~Retroherz()
        {
            this.Dispose(false);
        }
    }
}
