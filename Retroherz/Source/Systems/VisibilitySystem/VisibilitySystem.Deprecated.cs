using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class _VisibilitySystem : EntityUpdateSystem, IDrawSystem
{
	private readonly Bag<int> _polygons = new();
	private readonly Bag<(double Theta, Vector Origin, Vector Position, Vector ContactPoint, Vector ContactNormal)> _rays = new();

	private readonly OrthographicCamera _camera;
	private readonly GraphicsDeviceManager _graphics;
	private readonly (RenderTarget2D ColorMap, RenderTarget2D LightMap, RenderTarget2D BlurMap) _renderTarget;
	private readonly SpriteBatch _spriteBatch;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<MetaComponent> _metaComponentMapper;
	private ComponentMapper<PlayerComponent> _playerComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	internal int playerId;
	internal Nullable<(ColliderComponent collider, TransformComponent transform)> player = null;

	public _VisibilitySystem(
		OrthographicCamera camera,
		GraphicsDeviceManager graphics,
		SpriteBatch spriteBatch,
		(RenderTarget2D colorMap, RenderTarget2D lightMap, RenderTarget2D blurMap) renderTarget
		)
		: base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
	{
		_camera = camera;
		_graphics = graphics;
		_renderTarget = renderTarget;
		_spriteBatch = spriteBatch;

	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_metaComponentMapper = mapperService.GetMapper<MetaComponent>();
		_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
	}

	public bool PlayerReady()
	{
		foreach (int entityId in ActiveEntities)
		{
			// Find player
			if (_playerComponentMapper.Get(entityId) is not null)
			{
				player = new(
					_colliderComponentMapper.Get(entityId),
					_transformComponentMapper.Get(entityId)
				);

				playerId = entityId;

				return true;
			}
		}

		return false;
	}

	public override void Update(GameTime gameTime)
	{
		if (!PlayerReady())
			return;

		if (player == null) return;

		float deltaTime = gameTime.GetElapsedSeconds();

		var t = ActiveEntities.ToArray();//.AsSpan();

		_rays.Clear();
		foreach (int entityId in ActiveEntities.AsReadOnlySpan())
		//foreach (int entityId in ActiveEntities.Where(id => id != playerId))
		{
			if (entityId == playerId)
				continue;

			(ColliderComponent collider, TransformComponent transform) entity = new(
				_colliderComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			// Do not process what can not be seen
			if (!_camera.BoundingRectangle.Intersects(Predictive.BoundingRectangle(
				entity.collider,
				entity.transform,
				deltaTime
			)))
				continue;

			// Target rectangle
			RectangleF rectangle = new(
				entity.transform.Position,
				entity.collider.Size
			);

			// Player origin
			/*Vector origin = (
				player?.transform.Position  +
				player?.collider.Origin
			) ?? Vector.Zero;*/
			Vector origin = Vector.Zero;//  Predictive.Vector(player?.collider, player?.transform, deltaTime);

			Vector direction;
			Math.Ray ray;	// Make internal
			double theta;

			Vector contactPoint;
			Vector contactNormal;

			// Known rays
			foreach (var corner in rectangle.GetCorners().ToArray().AsSpan())
			{
				direction = (corner - origin); 
				ray = new(origin, direction);
				theta = Vector.Angle(origin, corner);

				/*if (ray.Intersects(
					rectangle,
					out contactPoint,
					out contactNormal
				))
				_rays.Add((
					theta,
					origin,
					corner,
					contactPoint,
					contactNormal
				));*/

				foreach (int b in ActiveEntities.AsReadOnlySpan())
				//foreach (int b in ActiveEntities.Where(id => id != entityId && id != playerId))
				{
					if (b == entityId && b == playerId)
						continue;

					contactPoint = Vector.Zero;
					contactNormal = Vector.Zero;

					(ColliderComponent collider, TransformComponent transform) B = new(
						_colliderComponentMapper.Get(b),
						_transformComponentMapper.Get(b)
					);

					var r = new RectangleF(B.transform.Position, B.collider.Size);

					if (ray.Intersects(r, out contactPoint, out contactNormal))
					_rays.Add((
						Vector.Angle(origin, r.Center),
						origin,
						corner,
						contactPoint,
						contactNormal
					));
				}
			}

			// Unknown rays
		}
	}

	public void Draw(GameTime gameTime)
	{
		/*var rays = _rays.ToArray().AsSpan();
		rays.Sort((a, b) => a.Theta < b.Theta ? -1 : 1);

		_graphics.GraphicsDevice.SetRenderTarget(null);
		_graphics.GraphicsDevice.Clear(Color.Transparent);

		

		_spriteBatch.Begin(
			sortMode: SpriteSortMode.FrontToBack,
			blendState: BlendState.Additive,
			samplerState: SamplerState.PointClamp,
			transformMatrix: _camera.GetViewMatrix()
		);

		foreach (var ray in rays)
		{
			_spriteBatch.DrawLine(
				ray.Origin,
				ray.ContactPoint,//ray.Position,
				Color.BlueViolet,
				layerDepth: 0	
			);

			_spriteBatch.DrawLine(
				point: ray.Origin,
				length: float.MaxValue,
				angle: ((float)ray.Theta),
				color: Color.BlueViolet
			);

			//_spriteBatch.DrawPoint(ray.ContactPoint, Color.White, 4);
			//_spriteBatch.DrawLine(ray.ContactPoint, ray.ContactPoint + ray.ContactNormal * 8, Color.Red);
		}

		_spriteBatch.End();
		_graphics.GraphicsDevice.SetRenderTarget(null);*/
	}
}