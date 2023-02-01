using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class RenderSystem : EntityDrawSystem
{
	private readonly OrthographicCamera _camera;
	private readonly (Effect Blur, Effect Combine, Effect Light) _effect;
	private readonly GraphicsDevice _graphics;
	private readonly Bag<int> _obstacles = new();
	private readonly (RenderTarget2D ColorMap, RenderTarget2D LightMap, RenderTarget2D BlurMap) _renderTarget;
	private readonly SpriteBatch _spriteBatch;
	private readonly TiledMapRenderer _tiledMapRenderer;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<PointLightComponent> _pointLightComponentMapper;
	private ComponentMapper<SpriteComponent> _spriteComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	//internal float deltaTime;
	//internal readonly Quad quad = new();

	public RenderSystem(
		OrthographicCamera camera,
		(Effect Blur, Effect Combine, Effect Light) effect,
		GraphicsDevice graphics,
		(RenderTarget2D ColorMap, RenderTarget2D LightMap, RenderTarget2D BlurMap) renderTarget,
		SpriteBatch spriteBatch,
		TiledMapRenderer tiledMapRenderer
		)
		: base(Aspect
			.All(typeof(ColliderComponent), typeof(SpriteComponent), typeof(TransformComponent)))
	{
		_camera = camera;
		_effect = effect;
		_graphics = graphics;
		_renderTarget = renderTarget;
		_spriteBatch = spriteBatch;
		_tiledMapRenderer = tiledMapRenderer;
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_pointLightComponentMapper = mapperService.GetMapper<PointLightComponent>();
		_spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
	}

	public override void Draw(GameTime gameTime)
	{
		float deltaTime = gameTime.GetElapsedSeconds();

		// Sprites
		_spriteBatch.Begin(
			//sortMode: SpriteSortMode.BackToFront,
			blendState: BlendState.Additive,
			samplerState: SamplerState.PointClamp,
			transformMatrix: _camera.GetViewMatrix()
		);

		foreach(ref readonly int entityId in ActiveEntities.AsReadOnlySpan())
		{
			(ColliderComponent Collider, SpriteComponent Sprite, TransformComponent Transform) entity = (
				_colliderComponentMapper.Get(entityId),
				_spriteComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			// Do not process what can not be seen
			if (!_camera.BoundingRectangle.Intersects(new RectangleF(entity.Transform.Position, entity.Collider.Size)))
				continue;

			entity.Sprite?.Draw(_spriteBatch);
		}

		_spriteBatch.End();

		// Map
		_tiledMapRenderer.Draw(_camera.GetViewMatrix());
	}
}