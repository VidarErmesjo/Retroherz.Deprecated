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
using Retroherz.Math;

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
            Vector position = Vector.One * 16 * 7;
			position.Y += 9;
            Vector size = new(32, 32);

            var player = _world.CreateEntity();
			player.Attach(new MetaComponent(id: player.Id, type: MetaComponentType.Player));
            player.Attach(new PlayerComponent());
            player.Attach(new SpriteComponent(asepriteDocument: AssetsManager.Sprite("Shitsprite")));
            player.Attach(new TransformComponent(position: position));
            player.Attach(new ColliderComponent(size: size, type: ColliderComponentType.Dynamic));
			player.Attach(new RayComponent(radius: 64));
        }

		public class TileComparer : IComparable<Tile>
		{
			public int CompareTo(Tile tile)
			{
				return tile.Type == TiledMapType.Empty ? 1 : -1;
			}
		} 

        public void CreateActors(int amount = 1)
        {
			TileComparer tileComparer = new();
			var tiles = Utils.ParseTiledMap(TiledMap).ToArray().Where(tile => tile.Type == TiledMapType.Empty).ToList();

			for (int i = 0; i < amount; i++ )
			{
				var minSize = 8;
				var maxSize = 16;
				var randomSize = System.Math.Clamp(Random.Shared.NextSingle() * maxSize, minSize, maxSize);
				var spawnTile = ((int)System.Math.Floor(Random.Shared.NextSingle() * tiles.Count));
				var tile = tiles.ElementAt(spawnTile);

				Vector position = new Vector(tile.X * TiledMap.TileWidth, tile.Y * TiledMap.TileHeight);
				Vector size = new(randomSize, randomSize);
				Vector velocity = Vector.Random();
				velocity = Vector.Clamp(velocity, -Vector.One, Vector.One) * size;
				var rectangle = new RectangleF(position, size);

				var actor = _world.CreateEntity();
				actor.Attach(new ActorComponent());
				actor.Attach(new MetaComponent(id: actor.Id, type: MetaComponentType.NPC));
				actor.Attach(new SpriteComponent(asepriteDocument: AssetsManager.Sprite("Shitsprite")));
				actor.Attach(new TransformComponent(position: position));
				actor.Attach(new ColliderComponent(velocity: velocity, size: size, type: ColliderComponentType.Dynamic));
				actor.Attach(new RayComponent(radius: randomSize * 2));

				tiles.Remove(tile);
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
			var entity = World.Create(new Transform(0, 0));
			System.Console.WriteLine(entity.Get<Transform>());

			if (entity.Has<Transform>())
				entity.Set(new Transform(4, 4));

			System.Console.WriteLine(entity.Get<Transform>());

			// SYSTEM
			var query = new QueryDescription().WithAll<Transform>();
			World.Query(in query, (in Arch.Core.Entity e) => {
				e.Set(new Transform(3, 3));
				e.Get<Transform>().Rotation = 4;
				System.Console.WriteLine(e.IsAlive());
				World.Destroy(in e);
			});

			World.Query(in query, (ref Transform transform) => {
				transform.Position = Vector.One * 2;
			});

			if (entity.IsAlive())
				System.Console.WriteLine(entity.Get<Transform>());
			System.Console.WriteLine(entity.IsAlive());

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
				//.AddSystem(new MetaSystem())
				.AddSystem(new HUDSystem(GraphicsDevice, Camera, InputManager))
                .Build();
            _world.Initialize();

			//EntityManager.Initialize(_world);
            //Components.Add(_world);

			var coolNames = new List<string> {"Halsbrann Sivertsen", "Salman Rushtid"};

			CreatePlayer();
			//CreateActors(50);

			var actor = _world.CreateEntity();
			actor.Attach(new TransformComponent(position: Vector.One * 16));
			actor.Attach(new ColliderComponent(
				size: Vector.Clamp(Vector.One * Random.Shared.NextSingle() * 8, Vector.One * 8, Vector.One * 16),
				velocity: Vector.One * 16,
				type: ColliderComponentType.Dynamic));
			actor.Attach(new MetaComponent(id: actor.Id, type: MetaComponentType.NPC));
			actor.Attach(new SpriteComponent(AssetsManager.Sprite("Shitsprite")));

            Camera.ZoomIn(6f);
            //GameManager.Camera.Rotate(MathHelper.Pi / 4);

            // Access TileMapLayers
            //var whatIs = _tiledMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");
            //var whatIs = _tiledMap.GetLayer("Tile Layer 1") as TiledMapTileLayer;
            //var whatIs = _tiledMap.TileLayers[0]; // can return null

            //whatIs.SetTile(1, 1, 1);

			// Vector
			Vector vector = new(65656, 656);
			System.Console.WriteLine(vector);
			System.Console.WriteLine(vector.Magnitude());
			System.Console.WriteLine(vector.Normalized());
			System.Console.WriteLine(vector);
			vector.Normalize();
			System.Console.WriteLine(vector);
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
