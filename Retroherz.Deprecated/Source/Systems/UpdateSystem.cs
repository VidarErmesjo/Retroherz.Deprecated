using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
	// Bad idea?
    public class UpdateSystem : EntityUpdateSystem
    {
		private readonly InputManager _inputManager;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

        public UpdateSystem(InputManager inputManager)
            : base(Aspect.All(typeof(ColliderComponent), typeof(SpriteComponent), typeof(TransformComponent)))
        {
			_inputManager = inputManager;
		}

        ~UpdateSystem() {}

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
        }

        public override void Update(GameTime gameTime)
        {
			var deltaTime = gameTime.GetElapsedSeconds();

			foreach (var entityId in ActiveEntities)
			{
				var collider = _colliderComponentMapper.Get(entityId);
				var sprite = _spriteComponentMapper.Get(entityId);
				var transform = _transformComponentMapper.Get(entityId);

				// Update position (can be done in ColliderSystem)
				//transform.Position += collider.Velocity * deltaTime;

				//transform.Position += -collider.DeltaOrigin;

				// Update sprite
				sprite.Scale = collider.Size;
				sprite.Position = transform.Position;
				sprite.Update(gameTime);
			}
        }
    }
}