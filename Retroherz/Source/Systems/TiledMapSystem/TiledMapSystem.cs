using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using Retroherz.Components;

namespace Retroherz.Systems
{
	public struct Tile
	{
		public int Id;
		public ushort X;
		public ushort Y;
		public ushort Layer;

		public Tile(int id, ushort x, ushort y, ushort layer)
		{
			Id = id;
			X = x;
			Y = y;
			Layer = layer;
		}

		public override string ToString() => "{" + $"X:{X} Y:{Y} Layer:{Layer}" + "}";
	}

    public partial class TiledMapSystem : EntityUpdateSystem, IDrawSystem
    {
		private bool _isDisposed = false;

		private readonly OrthographicCamera _camera;
		private readonly TiledMap _tiledMap;
		private readonly TiledMapRenderer _tiledMapRenderer;
		private readonly Bag<Tile> _tiles;

		private ComponentMapper<ColliderComponent> _colliderComponentMapper;
		private ComponentMapper<TransformComponent> _transformComponentMapper;

        public TiledMapSystem(TiledMap tiledMap, GraphicsDevice graphics, OrthographicCamera camera)
			: base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
        {
			_camera = camera;
			_tiledMap = tiledMap;
			_tiledMapRenderer = new(graphics, tiledMap);
			_tiles = new(tiledMap.Width * tiledMap.Height);
        }

		public List<TiledMapTile> Tiles(ushort layer = 0) => _tiledMap.TileLayers[layer].Tiles.ToList();
		public List<TiledMapTile> Tiles(string layer = "Tile Layer 1") => _tiledMap.GetLayer<TiledMapTileLayer>(layer).Tiles.ToList();

		public void RemoveTile(Tile tile) => _tiledMap.TileLayers[tile.Layer].RemoveTile(tile.X, tile.Y);
		public void RemoveTile(ushort x, ushort y, ushort layer = 0) => _tiledMap.TileLayers[layer].RemoveTile(x, y);
		public void RemoveTile(ushort x, ushort y, string layer = "Tile Layer 1") =>  _tiledMap.GetLayer<TiledMapTileLayer>(layer).RemoveTile(x, y);

		private void ReloadMap() => _tiledMapRenderer.LoadMap(_tiledMap);

		public override void Initialize(IComponentMapperService mapperService)
        {
			_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
			_transformComponentMapper = mapperService.GetMapper<TransformComponent>();

			ushort layer = 0;

			// Should every tile be an entity???
			// Certainly objects should
			foreach (var tile in Tiles(layer).Where(tile => !tile.IsBlank))
			{
				var entity = CreateEntity();
                var position = new Vector2(tile.X * _tiledMap.TileWidth, tile.Y * _tiledMap.TileHeight);
                var size = new Vector2(_tiledMap.TileWidth, _tiledMap.TileHeight);
				var tiledMapTile = new Tile(entity.Id, tile.X, tile.Y, layer);

				entity.Attach(new ColliderComponent(size: size, type: ColliderComponentType.Static));
				entity.Attach(new MetaComponent(id: entity.Id, type: MetaComponentType.Wall));
				entity.Attach(new TransformComponent(position));
				//entity.Attach(new SpriteComponent(_tiledMap.GetTilesetByTileGlobalIdentifier(1).Texture));
				//entity.Attach(tiledMapTile);

				// Keep local track of tiles to sync EC system and renderer
				_tiles.Add(tiledMapTile);
			}
		}

        public override void Update(GameTime gameTime)
        {
			foreach (var entityId in ActiveEntities)
				RestrictMovementToTiledMap(entityId);

			// Remove tile if entity was destroyed (sync entities and map)
			foreach (var tile in _tiles)
				if (!ActiveEntities.Contains(tile.Id))
				{
					this.RemoveTile(tile);
					this.ReloadMap();
					_tiles.Remove(tile);
				}

			_tiledMapRenderer.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
			_tiledMapRenderer.Draw(_camera.GetViewMatrix());
		}

		public new void Dispose()
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
                _tiledMapRenderer.Dispose();
            }

            _isDisposed = true;
        }


        ~TiledMapSystem()
		{
			this.Dispose(false);
		}
    }
}