using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.Tiled;
using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class ShadowsSystem : EntityUpdateSystem, IDrawSystem
{
	private readonly OrthographicCamera _camera;
	private readonly SpriteBatch _spriteBatch;
	private readonly TiledMap _tiledMap;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<PlayerComponent> _playerComponentMapper;
	private ComponentMapper<SpriteComponent> _spriteComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	//internal Memory<PolyMapEdge> polyMap;
	internal static Memory<PolyMapCell> Cells;
	internal static readonly Bag<PolyMapEdge> PolyMap = new();

	internal static readonly Vector2[] triangle = new Vector2[3];
	internal static readonly Bag<VisibilityPolygonPoint> visibilityPolygonPoints = new();

	internal static Memory<VisibilityPolygonPoint> VisibilityPolygon = new();

	internal static (ColliderComponent collider, PlayerComponent player, SpriteComponent sprite, TransformComponent transform) entity;
	
	public ShadowsSystem(OrthographicCamera camera, SpriteBatch spriteBatch, TiledMap tiledMap)
		: base(Aspect.All(typeof(ColliderComponent), typeof(SpriteComponent), typeof(TransformComponent)))
	{
		_camera = camera;
		_spriteBatch = spriteBatch;
		_tiledMap = tiledMap;

		Cells = new(new PolyMapCell[tiledMap.Width * tiledMap.Height]);
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
		_spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
	}

	public override void Update(GameTime gameTime)
	{
		foreach (int entityId in ActiveEntities.AsReadOnlySpan())
		{
			entity = new(
				_colliderComponentMapper.Get(entityId),
				_playerComponentMapper.Get(entityId),
				_spriteComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			if (entity.player is null)
				continue;

			CalculateVisibilityPolygon(entity.transform.Position + entity.collider.Origin, 16);
		}
	}

	public void Draw(GameTime gameTime)
	{
		_spriteBatch.Begin(
			sortMode: SpriteSortMode.FrontToBack,
			blendState: BlendState.Additive,
			samplerState: SamplerState.PointClamp,
			transformMatrix: _camera.GetViewMatrix()
		);

		foreach (int entityId in ActiveEntities.AsReadOnlySpan())
		{
			entity = new(
				_colliderComponentMapper.Get(entityId),
				_playerComponentMapper.Get(entityId),
				_spriteComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			if (entity.player is null)
				continue;

			for (int i = 0; i < VisibilityPolygon.Length - 1; i++)
			{
				triangle[0] = entity.transform.Position + entity.collider.Origin;
				triangle[1] = VisibilityPolygon.ToArray()[i].Position;
				triangle[2] = VisibilityPolygon.ToArray()[i + 1].Position;
				_spriteBatch.DrawPolygon(Vector2.Zero, new Polygon(triangle), Color.CornflowerBlue);
			}

			// Last
			triangle[0] = entity.transform.Position + entity.collider.Origin;
			triangle[1] = VisibilityPolygon.ToArray()[VisibilityPolygon.Length - 1].Position;
			triangle[2] = VisibilityPolygon.ToArray()[0].Position;
			_spriteBatch.DrawPolygon(Vector2.Zero, new Polygon(triangle), Color.CornflowerBlue);

			ReadOnlySpan<PolyMapEdge> polyMap = CreatePolyMap(_tiledMap);
			// "PolyMap"
			foreach (var edge in polyMap)
				_spriteBatch.DrawLine(edge.Start, edge.End, Color.Red);
		}

		_spriteBatch.End();
	}
}