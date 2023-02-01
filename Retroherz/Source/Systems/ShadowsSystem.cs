using System;
using System.Buffers;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.VectorDraw;
using Retroherz.Components;
using Retroherz.Collections;
using Retroherz.Managers;
using Retroherz.Math;
using Retroherz.Visibility;

namespace Retroherz.Systems;

// VisibilitySystem. Render in RenderSystem
public partial class ShadowsSystem : EntityUpdateSystem, IDrawSystem
{
	private readonly OrthographicCamera _camera;
	private readonly Rectangle _destinationRectangle;
	private readonly (Effect Blur, Effect Combine, Effect Light) _effect;
	private readonly GraphicsDevice _graphics;
	private readonly (RenderTarget2D Color, RenderTarget2D Light) _renderTarget;
	private readonly SpriteBatch _spriteBatch;
	private readonly TiledMap _tiledMap;
	private readonly VisibilityComputer _visibilityComputer;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<PlayerComponent> _playerComponentMapper;
	private ComponentMapper<PointLightComponent> _pointLightComponentMapper;
	private ComponentMapper<SpriteComponent> _spriteComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	public readonly Sekk<int> Occluders = new Sekk<int>(64);
	public readonly Sekk<int> Illumers = new Sekk<int>(64);

	public ShadowsSystem(GraphicsManager graphicsManager, TiledMap tiledMap)
		: base(Aspect.All(typeof(ColliderComponent), typeof(SpriteComponent), typeof(TransformComponent)))
	{
		_camera = graphicsManager.GetCamera();
		_destinationRectangle = graphicsManager.DestinationRectangle;
		_effect.Blur = graphicsManager.GetEffect("Blur");
		_effect.Combine = graphicsManager.GetEffect("Combine");
		_effect.Light = graphicsManager.GetEffect("Light");
		_graphics = graphicsManager.GetGraphicsDevice();
		_renderTarget.Color = graphicsManager.GetRenderTarget("Color");
		_renderTarget.Light = graphicsManager.GetRenderTarget("Light");	// ... or create it here
		_spriteBatch = graphicsManager.GetSpriteBatch();
		_tiledMap = tiledMap;
		_visibilityComputer = VisibilityComputer.GetInstance();
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
		_pointLightComponentMapper = mapperService.GetMapper<PointLightComponent>();
		_spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
	}

	public override void Update(GameTime gameTime)
	{
		float deltaTime = gameTime.GetElapsedSeconds();

		Occluders.Clear();
		Illumers.Clear();

		// "Query"
		foreach (ref readonly int entityId in ActiveEntities.AsReadOnlySpan())
		{
			Occluders.Add(entityId);

			if (_pointLightComponentMapper.Get(entityId) is null)
				continue;

			Illumers.Add(entityId);
		}

		// Process "Illumer".
		foreach (ref readonly int illumerId in Illumers)
		{
			(ColliderComponent Collider, PointLightComponent Light, TransformComponent Transform) illumer = (
				_colliderComponentMapper.Get(illumerId),
				_pointLightComponentMapper.Get(illumerId),
				_transformComponentMapper.Get(illumerId)
			);

			_visibilityComputer.SetCamera(_camera);

			_visibilityComputer.SetIllumer(
				origin: illumer.Transform.Position + illumer.Collider.HalfExtents,
				radius: illumer.Light.Radius
			);

			// Add "Occluders".
			foreach (ref readonly int occluderId in Occluders)
			{
				if (occluderId == illumerId)
					continue;

				(ColliderComponent Collider, PointLightComponent Light, SpriteComponent Sprite, TransformComponent Transform) occluder = (
					_colliderComponentMapper.Get(occluderId),
					_pointLightComponentMapper.Get(occluderId) ?? new PointLightComponent(),
					_spriteComponentMapper.Get(occluderId),
					_transformComponentMapper.Get(occluderId)
				);

				// EXP
				foreach (var slice in occluder.Sprite.Slices)
				{
					_visibilityComputer.AddOccluder(
						origin: slice.BoundingRectangle.Center,
						halfExtents: slice.BoundingRectangle.Size / 2,
						radius: occluder.Light.Radius
					);
				}

				/*_visibilityComputer.AddOccluder(
					origin: occluder.Transform.Position + occluder.Collider.HalfExtents,
					halfExtents: occluder.Collider.HalfExtents,
					radius: occluder.Light.Radius
				);*/
			}

			ReadOnlySpan<Vector> points = _visibilityComputer.CalculateVisibilityPolygon();

			illumer.Light.Primitive = new Primitive()
				.CreateTriangleFan(points, illumer.Transform.Position + illumer.Collider.HalfExtents)
				.ProjectToScreen(_camera);
		}
	}

	public void Draw(GameTime gameTime)
	{
		float deltaTime = gameTime.GetElapsedSeconds();
		// Draw Color
		//_graphics.SetRenderTarget(_renderTarget.Color);
		_graphics.Clear(Color.Transparent);

		/*_spriteBatch.Begin(
			sortMode: SpriteSortMode.BackToFront,
			blendState: BlendState.AlphaBlend,
			samplerState: SamplerState.PointClamp,
			depthStencilState: DepthStencilState.Default,
			rasterizerState: RasterizerState.CullCounterClockwise,
			effect: null,
			transformMatrix: _camera.GetViewMatrix()
		);
			//ReadOnlySpan<PolyMap> polyMap = _tiledMap.CreatePolyMap();
			foreach (var edge in _visibilityComputer.GetPolyMap())
				_spriteBatch.DrawLine(edge.Start, edge.End, Color.Red);

		_spriteBatch.End();*/

		// Draw Light
		float ambientLightStrength = 0.0f;

		//_graphics.Clear(Color.White * ambientLightStrength);

		_graphics.BlendState = BlendState.Additive;

		// Samplers states are set by the shader itself            
		_graphics.DepthStencilState = DepthStencilState.None;
		_graphics.RasterizerState = RasterizerState.CullNone;

		foreach (ref readonly int illumerId in Illumers)
		{
			(ColliderComponent Collider, PointLightComponent Light, TransformComponent Transform) illumer = (
				_colliderComponentMapper.Get(illumerId),
				_pointLightComponentMapper.Get(illumerId),
				_transformComponentMapper.Get(illumerId)
			);

			if (illumer.Light.Power <= 0)
				continue;

			// Apply the effect
			_effect.Light.Parameters["lightSource"].SetValue(illumer.Transform.Position + illumer.Collider.HalfExtents);
			_effect.Light.Parameters["lightColor"].SetValue(illumer.Light.Color.ToVector3() * illumer.Light.Power);
		 	_effect.Light.Parameters["lightRadius"].SetValue(illumer.Light.Radius);
			_effect.Light.Techniques[0].Passes[0].Apply();

			// Render the mesh
			illumer.Light.Primitive.Draw(_graphics);
		}
	}
}