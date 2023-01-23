using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using Retroherz.Math;
using Retroherz.Visibility;

namespace Retroherz.Systems;

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

public class TiledMapSystem : EntityUpdateSystem
{
	private readonly OrthographicCamera _camera;
	private readonly TiledMap _tiledMap;
	private readonly TiledMapRenderer _tiledMapRenderer;
	private readonly Bag<Tile> _tiles;
	private readonly VisibilityComputer _visibilityComputer;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	public static bool TiledMapDidUpdate = false;

	public TiledMapSystem(OrthographicCamera camera, TiledMap tiledMap, TiledMapRenderer tiledMapRenderer)
		: base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
	{
		_camera = camera;
		_tiledMap = tiledMap;
		_tiledMapRenderer = tiledMapRenderer;
		_tiles = new(tiledMap.Width * tiledMap.Height);

		//_tiledMap.CreatePolyMap();
		_visibilityComputer = VisibilityComputer.GetInstance();
		Console.WriteLine($"VisibilityComputer.IsAlive:{VisibilityComputer.IsAlive}");
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

		_visibilityComputer.SetTiledMap(_tiledMap, 0);

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
		foreach (int entityId in ActiveEntities.AsReadOnlySpan())
			RestrictMovementToTiledMap(entityId);

		// Remove tile if entity was destroyed (sync entities and map)
		foreach (Tile tile in _tiles.ToArray().AsSpan())
			if (!ActiveEntities.Contains(tile.Id))
			{
				this.RemoveTile(tile);
				this.ReloadMap();
				_tiles.Remove(tile);

				// Update the "PolyMap"
				_visibilityComputer.SetTiledMap(_tiledMap, 0);
				//_tiledMap.CreatePolyMap();
			}

		_tiledMapRenderer.Update(gameTime);
	}

	private void RestrictMovementToTiledMap(int entityId)
	{
		var collider = _colliderComponentMapper.Get(entityId);
		var transform = _transformComponentMapper.Get(entityId);

		if (collider.Type == Components.ColliderComponentType.Static) return;

		if (transform.Position.X < 0)
		{
			transform.Position = transform.Position.SetX(0);
			collider.Velocity = collider.Velocity.SetX(0);
		}
		else if (transform.Position.X > _tiledMap.WidthInPixels - collider.Size.X)
		{
			transform.Position = transform.Position.SetX(_tiledMap.WidthInPixels - collider.Size.X);
			collider.Velocity = collider.Velocity.SetX(0);
		}
		
		if (transform.Position.Y < 0)
		{
			transform.Position = transform.Position.SetY(0);
			collider.Velocity = collider.Velocity.SetY(0);
		}
		else if (transform.Position.Y > _tiledMap.HeightInPixels - collider.Size.Y)
		{
			transform.Position = transform.Position.SetY(_tiledMap.HeightInPixels - collider.Size.Y);
			collider.Velocity = collider.Velocity.SetY(0);
		}			
	}
}