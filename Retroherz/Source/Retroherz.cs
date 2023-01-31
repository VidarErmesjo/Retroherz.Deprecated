using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.ViewportAdapters;

using Arch;
using Arch.Core;
using Arch.Core.CommandBuffer;
using Arch.Core.Extensions;
using Arch.Core.Utils;

using MonoGame.Extended.Tiled.Renderers;

using Retroherz.Collections;
using Retroherz.Components;
using Retroherz.Systems;
using Retroherz.Managers;
using Retroherz.Math;
using Retroherz.Visibility;

// RetroHerzen ???
// Erm ???
// vErmNGN
namespace Retroherz
{
    public class Retroherz : Game
    {
        private bool _isDisposed = false;

		private Arch.Core.World ArchWorld;

		// NEW
	
		readonly AssetsManager assetsManager;
		readonly GraphicsManager graphicsManager;
		readonly VisibilityComputer visibilityComputer;

		InputManager inputManager;
		OrthographicCamera camera;
		GraphicsDevice graphics;
        SpriteBatch spriteBatch;
		TiledMapRenderer tiledMapRenderer;
		(Effect Blur, Effect Combine, Effect Light) effect;
		(RenderTarget2D BlurMap, RenderTarget2D ColorMap, RenderTarget2D LightMap) renderTarget;
		TiledMap tiledMap;
		MonoGame.Extended.Entities.World world;

		// NEW

        public Retroherz(GraphicsMode mode = GraphicsMode.Default, bool fullscreen = true, bool scale = false)
        {
			assetsManager = new(Content);
			graphicsManager = new(this, mode, fullscreen, scale);
			visibilityComputer = VisibilityComputer.GetInstance();
        }

        protected override void LoadContent()
        {
            assetsManager.LoadContent();
            tiledMap =  Content.Load<TiledMap>("Tiled/Shitmap"); //AssetsManager.Map("Shitmap");
/*
Unhandled exception. Microsoft.Xna.Framework.Content.ContentLoadException: The content file was not found.
 ---> System.IO.FileNotFoundException: Could not find file 'C:\Users\Voidar\Documents\Programmering\dotnet\MonoGame\Retroherz\Retroherz\bin\Debug\net7.0-windows\Content\Tiled\Shitbrick.xnb'.
File name: 'C:\Users\Voidar\Documents\Programmering\dotnet\MonoGame\Retroherz\Retroherz\bin\Debug\net7.0-windows\Content\Tiled\Shitbrick.xnb'
*/
			// Load and register effects
			effect.Blur = graphicsManager.RegisterEffect(assetsManager.Effect("Blur"));
			effect.Combine = graphicsManager.RegisterEffect(assetsManager.Effect("Combine"));
			effect.Light = graphicsManager.RegisterEffect(assetsManager.Effect("Light"));

            base.LoadContent();
        }

        public void CreatePlayer()
        {
            Vector position = tiledMap.ObjectLayers[0].Objects[0].Position;
            Vector size = new(32, 32);

            var player = world.CreateEntity();
			player.Attach(new MetaComponent(id: player.Id, type: MetaComponentType.Player));
            player.Attach(new PlayerComponent());
			player.Attach(new PointLightComponent(radius: 96, color: Color.CornflowerBlue));
            player.Attach(new SpriteComponent(asepriteDocument: assetsManager.Sprite("Shitsprite")));
            player.Attach(new TransformComponent(position: position));
            player.Attach(new ColliderComponent(size: size, type: ColliderComponentType.Dynamic));
			player.Attach(new Collider { Velocity = Vector.Zero }); // :)
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
			var tiles = Utils.ParseTiledMap(tiledMap).ToArray().Where(tile => tile.Type == TiledMapType.Empty).ToList();

			for (int i = 0; i < amount; i++ )
			{
				var minSize = 8;
				var maxSize = 16;
				var randomSize = System.Math.Clamp(Random.Shared.NextSingle() * maxSize, minSize, maxSize);
				var spawnTile = ((int)System.Math.Floor(Random.Shared.NextSingle() * tiles.Count));
				var tile = tiles.ElementAt(spawnTile);

				Vector position = new Vector(tile.X * tiledMap.TileWidth, tile.Y * tiledMap.TileHeight);
				Vector size = new(randomSize, randomSize);
				Vector velocity = Vector.Random();
				velocity = Vector.Clamp(velocity, -Vector.One, Vector.One) * size;
				var rectangle = new RectangleF(position, size);

				var actor = world.CreateEntity();
				actor.Attach(new ActorComponent());
				actor.Attach(new MetaComponent(id: actor.Id, type: MetaComponentType.NPC));
				actor.Attach(new SpriteComponent(asepriteDocument: assetsManager.Sprite("Shitsprite")));
				actor.Attach(new TransformComponent(position: position));
				actor.Attach(new ColliderComponent(velocity: velocity, size: size, type: ColliderComponentType.Dynamic));
				actor.Attach(new RayComponent(radius: randomSize * 2));
				actor.Attach(new PointLightComponent(radius: size.Magnitude() * 2, color: Color.DarkGoldenrod));


				tiles.Remove(tile);
			}
        }

        protected override void Initialize()
        {
            base.Initialize();

			graphics = graphicsManager.Initialize();
			Console.WriteLine($"{graphicsManager}");

			camera = graphicsManager.GetCamera();
			spriteBatch = graphicsManager.GetSpriteBatch();
			tiledMapRenderer = new(graphics);
			inputManager = new(camera);

			// Set up all render targets.
			// The blur map does not need a depth buffer			
			renderTarget.BlurMap = graphicsManager.CreateRenderTarget(
				"Blur",
				mipMap: false,
				preferredFormat: SurfaceFormat.Color,
				preferredDepthFormat: DepthFormat.None,
				preferredMultiSampleCount: 16,
				usage: RenderTargetUsage.DiscardContents	
			);

			renderTarget.ColorMap = graphicsManager.CreateRenderTarget(
				"Color",
				mipMap: false,
				preferredFormat: SurfaceFormat.Color,
				preferredDepthFormat: DepthFormat.Depth16,
				preferredMultiSampleCount: 16,
				usage: RenderTargetUsage.DiscardContents	
			);

			renderTarget.LightMap = graphicsManager.CreateRenderTarget(
				"Light",
				mipMap: false,
				preferredFormat: SurfaceFormat.Color,
				preferredDepthFormat: DepthFormat.Depth16,
				preferredMultiSampleCount: 16,
				usage: RenderTargetUsage.DiscardContents	
			);

			tiledMapRenderer.LoadMap(tiledMap);

			// Arch START
			// Bytte fra Extended ESC til Arch? :)
			ArchWorld = Arch.Core.World.Create();

			// COMPONENT (struct over class)
			Arch.Core.Entity entity = ArchWorld.Create(new Transform(0, 0));
			Console.WriteLine(entity.Get<Transform>());

			if (entity.Has<Transform>())
				entity.Set(new Transform(4, 4));

			Console.WriteLine(entity.Get<Transform>());

			// SYSTEM
			var query = new QueryDescription().WithAll<Transform>();

			List<Arch.Core.Entity> ents = new();
			ArchWorld.GetEntities(in query, ents);
			Console.WriteLine($"GetEntities:{ents.Count} SizeOf:{Unsafe.SizeOf<Arch.Core.Entity>()}");

			ArchWorld.Query(in query, (in Arch.Core.Entity entity) => {
				entity.Set(new Transform(3, 3));
				entity.Get<Transform>().Rotation = 4;
				Console.WriteLine(entity.IsAlive());
				ArchWorld.Destroy(in entity);
			});

			ArchWorld.Query(in query, (ref Transform transform) => {
				transform.Position = Vector.One * 2;
			});

			if (entity.IsAlive())
				System.Console.WriteLine(entity.Get<Transform>());
			Console.WriteLine(entity.IsAlive());

			Arch.Core.World.Destroy(ArchWorld);

			// Arch END

			//ShadowsSystem shadowsSystem = new(graphicsManager, tiledMap);

            world = new WorldBuilder()
			    .AddSystem(new PlayerSystem(assetsManager, camera, inputManager))
				.AddSystem(new CameraSystem(camera, tiledMap))
				//.AddSystem(new VisibilitySystem(camera, effect, graphics, renderTarget, spriteBatch))
				.AddSystem(new TiledMapSystem(camera, tiledMap, tiledMapRenderer))
                //.AddSystem(new TiledMapSystem(camera, tiledMap, tiledMapRenderer))
                .AddSystem(new ExpirySystem())
                .AddSystem(new ColliderSystem())
                .AddSystem(new UpdateSystem(inputManager))
				.AddSystem(new ShadowsSystem(graphicsManager, tiledMap))
                .AddSystem(new RenderSystem(camera, effect, graphics, renderTarget, spriteBatch, tiledMapRenderer))
				.AddSystem(new DebugSystem(camera, spriteBatch))
				//.AddSystem(new MetaSystem())
				//.AddSystem(new CameraSystem(camera, tiledMap))
				.AddSystem(new SelectSystem(inputManager))
				.AddSystem(new HUDSystem(camera, inputManager, spriteBatch))
                .Build();
            world.Initialize();			

			//EntityManager.Initialize(World;);
            //Components.Add(World;);

			var coolNames = new List<string> {"Halsbrann Sivertsen", "Salman Rushtid"};

			CreatePlayer();
			CreateActors(25);

			var actor = world.CreateEntity();
			actor.Attach(new TransformComponent(position: Vector.One * 16 * 10));
			var size = Vector.Clamp(Vector.One * Random.Shared.NextSingle() * 8, Vector.One * 8, Vector.One * 16);
			actor.Attach(new ColliderComponent(
				size: size,
				velocity: Vector.One * 16,
				type: ColliderComponentType.Dynamic));
			actor.Attach(new MetaComponent(id: actor.Id, type: MetaComponentType.NPC));
			actor.Attach(new SpriteComponent(assetsManager.Sprite("Shitsprite")));
			actor.Attach(new PointLightComponent(radius: size.Magnitude() * 4, color: Color.DarkGoldenrod));
        }

        protected override void Update(GameTime gameTime)
        {
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

			inputManager.Update(gameTime);
			graphicsManager.Update(gameTime);
            world.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
			//graphics.SetRenderTarget(graphicsManager.RenderTarget); 
			//graphics.Clear(Color.Black);

			world.Draw(gameTime);

			/*graphics.SetRenderTarget(null);
			graphics.Clear(Color.Black);

			spriteBatch.Begin(
				sortMode: SpriteSortMode.Immediate,
				blendState: BlendState.NonPremultiplied,
				samplerState: SamplerState.PointClamp,
				depthStencilState: DepthStencilState.None,
				rasterizerState: RasterizerState.CullNone
			);
			spriteBatch.Draw(
				graphicsManager.RenderTarget,
				graphicsManager.DestinationRectangle,
				Color.White
			);
			spriteBatch.End();*/
        }

        /// <summary>
        ///     Draws to device
        /// </summary>
        /// <remarks>   
        ///     Includes effects
        /// </remarks>
        /*private void Draw(SpriteBatch SpriteBatch, Effect Effect = null)
        {
           GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);

            SpriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                Effect: Effect);

            SpriteBatch.Draw(
                (GameManager.LowResolution ? GameManager.VirtualRenderTarget : GameManager.DeviceRenderTarget),
                GameManager.DeviceRectangle,
                Color.White);

            SpriteBatch.End();
        }*/

        protected override void UnloadContent()
        {
			Console.WriteLine($"EntityCount:{world.EntityCount}");
            assetsManager.UnloadContent();
            base.UnloadContent();
        }

		// IDisposable
		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        protected override void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
				effect.Blur.Dispose();
				effect.Combine.Dispose();
				effect.Light.Dispose();
				graphicsManager.Dispose();
				renderTarget.BlurMap.Dispose();
				renderTarget.ColorMap.Dispose();
				renderTarget.LightMap.Dispose();
                spriteBatch.Dispose();
				tiledMapRenderer.Dispose();
                world.Dispose();

				Console.WriteLine("Effect.Dispose() => OK");
				Console.WriteLine("GraphicsDevice.Dispose() => OK");
				Console.WriteLine("RenderTarget.Dispose() => OK");
				Console.WriteLine("SpriteBatch.Dispose() => OK");
				Console.WriteLine("TiledMapRenderer.Dispose() => OK");
				Console.WriteLine("virtualRenderTarget.Dispose() => OK");
				Console.WriteLine("World.Dispose() => OK");
            }

            _isDisposed = true;
        }

        ~Retroherz() => this.Dispose(false);
    }
}
