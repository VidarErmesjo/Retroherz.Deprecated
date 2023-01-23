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
public class DebugSystem : EntityDrawSystem, IDisposable
{
	//private bool _isDisposed = false;

	private readonly SpriteBatch _spriteBatch;
	private readonly OrthographicCamera _camera;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<ExpiryComponent> _expiryComponentMapper;
	private ComponentMapper<MetaComponent> _metaComponentMapper;
	private ComponentMapper<RayComponent> _rayComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	public DebugSystem(OrthographicCamera camera, SpriteBatch spriteBatch)
		: base(Aspect
			.All(typeof(ColliderComponent), typeof(MetaComponent), typeof(TransformComponent)))
	{
		_camera = camera;
		_spriteBatch = spriteBatch;
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_expiryComponentMapper = mapperService.GetMapper<ExpiryComponent>();
		_metaComponentMapper = mapperService.GetMapper<MetaComponent>();
		_rayComponentMapper = mapperService.GetMapper<RayComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
	}

	public override void Draw(GameTime gameTime)
	{
		var deltaTime = gameTime.GetElapsedSeconds();

		_spriteBatch.Begin(
			sortMode: SpriteSortMode.FrontToBack,
			blendState: BlendState.Additive,
			samplerState: SamplerState.PointClamp,
			transformMatrix: _camera.GetViewMatrix());

		foreach(int entityId in ActiveEntities.AsReadOnlySpan())
		{
			(ColliderComponent collider, ExpiryComponent expiry, RayComponent ray, TransformComponent transform) entity = new(
				_colliderComponentMapper.Get(entityId),
				_expiryComponentMapper.Get(entityId),
				_rayComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			// Do not process what can not be seen
			if (!_camera.BoundingRectangle.Intersects(Predictive.BoundingRectangle(
				entity.collider,
				entity.transform,
				deltaTime
			)))
				continue;

			// Ignore tiles
			if (entity.collider.Type != ColliderComponentType.Static)
			{			
				// Bounding rectangle
				var bounds = new RectangleF(entity.transform.Position, entity.collider.Size);
				_spriteBatch.DrawRectangle(bounds, Color.Green);

				// Pilot rectangle
				var pilot = new RectangleF(entity.transform.Position + entity.collider.Velocity * deltaTime, entity.collider.Size);
				_spriteBatch.DrawRectangle(pilot, Color.Red);

				// Search area
				var inflated = Predictive.BoundingRectangle(entity.collider, entity.transform, deltaTime);
				//inflated = Utils.ProspectiveCircle(entity.collider, entity.transform, deltaTime, true);

				_spriteBatch.DrawRectangle(inflated, Color.Yellow);
				//_spriteBatch.DrawCircle(inflated, 32, Color.Yellow);
			}

			// Constraints info
			foreach (var contact in entity.collider.Constraints)
			{
				var collider = _colliderComponentMapper.Get(contact.Id);
				var transform = _transformComponentMapper.Get(contact.Id);

				var origin = contact.ContactPoint + entity.collider.Velocity * deltaTime;
				// Normals
				_spriteBatch.DrawPoint(origin, Color.BlueViolet, 4);
				_spriteBatch.DrawPoint(origin, Color.Red, 4);
				_spriteBatch.DrawLine(origin, origin + contact.ContactNormal * 8, Color.Red);

				// Rays
				_spriteBatch.DrawLine(
					origin,
					transform.Position + collider.HalfExtents,
					Color.BlueViolet);

				// Inflated
				_spriteBatch.DrawRectangle(
					contact.Obstacle.transform.Position - entity.collider.HalfExtents,
					contact.Obstacle.collider.Size + entity.collider.Size,
					Color.BlueViolet);
				
				// "Embelish" contacts
				_spriteBatch.FillRectangle(
					transform.Position,
					collider.Size,
					Color.Yellow);
			}

			// Contacts info
			foreach (var contact in entity.collider.Contacts)
			{
				var collider = _colliderComponentMapper.Get(contact.Id);
				var transform = _transformComponentMapper.Get(contact.Id);

				var origin = contact.ContactPoint + entity.collider.Velocity * deltaTime;
				// Normals
				_spriteBatch.DrawPoint(origin, Color.BlueViolet, 4);
				_spriteBatch.DrawPoint(origin, Color.Red, 4);
				_spriteBatch.DrawLine(origin, origin + contact.ContactNormal * 8, Color.Red);

				// Rays
				_spriteBatch.DrawLine(
					origin,
					transform.Position + collider.HalfExtents,
					Color.BlueViolet);

				// Inflated
				_spriteBatch.DrawRectangle(
					transform.Position - entity.collider.HalfExtents,
					collider.Size + entity.collider.Size,
					Color.BlueViolet);
				
				// "Embelish" contacts
				_spriteBatch.FillRectangle(
					transform.Position,
					collider.Size,
					Color.Yellow);
			}

			if (entity.collider.Type == ColliderComponentType.Dynamic)
				foreach (var ray in entity.collider.Rays)
				{
					var position = ray.Position + entity.collider.Velocity * deltaTime;
					var direction = position + ray.Direction;
					_spriteBatch.DrawLine(position, direction, Color.Red);
				}
		}

		_spriteBatch.End();
	}
}