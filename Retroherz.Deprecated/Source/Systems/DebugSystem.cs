using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class DebugSystem : EntityDrawSystem, IDisposable
    {
        private bool _isDisposed = false;

        private readonly SpriteBatch _spriteBatch;
        private readonly OrthographicCamera _camera;

        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
		private ComponentMapper<ExpiryComponent> _expiryComponentMapper;
		private ComponentMapper<MetaComponent> _metaComponentMapper;
		private ComponentMapper<RayComponent> _rayComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

        public DebugSystem(GraphicsDevice graphicsDevice, OrthographicCamera camera)
            : base(Aspect
                .All(typeof(ColliderComponent), typeof(MetaComponent), typeof(TransformComponent)))
        {
            _camera = camera;
            _spriteBatch = new(graphicsDevice);
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
			//_inputManager.Update(gameTime);

            _spriteBatch.Begin(
                sortMode: SpriteSortMode.FrontToBack,
                blendState: BlendState.Additive,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix());

            foreach(var entityId in ActiveEntities)
            {
				(ColliderComponent collider, ExpiryComponent expiry, RayComponent ray, TransformComponent transform) entity = new(
					_colliderComponentMapper.Get(entityId),
					_expiryComponentMapper.Get(entityId),
					_rayComponentMapper.Get(entityId),
					_transformComponentMapper.Get(entityId)
				);

				//if (entity.expiry is not null)
					//entity.collider.Size += Vector2.One * entity.expiry.TimeRemaining;
					//entity.collider.Size -= Vector2.One * 0.01f;
				//_colliderComponentMapper.Put(entityId, new(velocity: entity.collider.Velocity, size: entity.collider.Size + Vector2.One * entity.expiry.TimeRemaining, type: entity.collider.Type));

				if (entity.ray is not null)
				{
					_spriteBatch.DrawCircle(entity.ray, 64, entity.ray.Hits.Count > 0 ? Color.Red : Color.White);
					foreach (var hit in entity.ray.Hits)
					{
						var origin = entity.transform.Position + entity.collider.Origin + entity.collider.Velocity * deltaTime;
						// Normals
						_spriteBatch.DrawPoint(origin, Color.BlueViolet, 4);
						_spriteBatch.DrawPoint(origin, Color.Red, 4);
						//_spriteBatch.DrawLine(origin, origin + hit.contactNormal * 8, Color.Red);

						//System.Console.WriteLine(hit.Distance);

						(ColliderComponent collider, TransformComponent transform) target = new(
							_colliderComponentMapper.Get(hit.Id),
							_transformComponentMapper.Get(hit.Id)
						);

						// Rays
						_spriteBatch.DrawLine(
							origin,
							target.transform.Position + target.collider.Origin,
							Color.BlueViolet);	
					}
				}

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
					var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
					var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);
					var inflated = new RectangleF(minimum, maximum - minimum);

					inflated = Utils.ProspectiveBoundingRectangle(entity.collider, entity.transform, deltaTime);

					_spriteBatch.DrawRectangle(inflated, Color.Yellow);
				}

				// Focused
				//if (meta.Focused)
					//_spriteBatch.DrawRectangle(bounds, Color.White);

				// Selected
				//if (meta.Selected)
					//_spriteBatch.FillRectangle(bounds, Color.Green);

                // Contac info
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
                        transform.Position + collider.Origin,
                        Color.BlueViolet);

                    // Inflated
                    _spriteBatch.DrawRectangle(
                        transform.Position - entity.collider.Origin,
                        collider.Size + entity.collider.Size,
                        Color.BlueViolet);
                    
                    // "Embelish" contacts
                    _spriteBatch.FillRectangle(
                        transform.Position,
                        collider.Size,
                        Color.Yellow);
                }
            }

            _spriteBatch.End();
        }

        public virtual void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
                _spriteBatch.Dispose();
            }

            _isDisposed = true;
        }

		~DebugSystem()
		{
			this.Dispose(false);
		}
    }
}