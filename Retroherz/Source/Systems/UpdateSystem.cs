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
    public class UpdateSystem : EntityProcessingSystem
    {
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

        public UpdateSystem()
            : base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)).One(typeof(SpriteComponent)))
        {}

        ~UpdateSystem() {}

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var collider = _colliderComponentMapper.Get(entityId);
            var sprite = _spriteComponentMapper.Get(entityId);
            var transform = _transformComponentMapper.Get(entityId);

            /*if (collider != null)
            {
                var sum = Vector2.Zero;
                foreach (var e in collider.PenetrationVectors)
                {
                    sum += e / collider.PenetrationVectors.Count;
                }
                collider.Bounds.Position -= sum;
                //System.Console.WriteLine("Sum: " + sum);
                transform.Position = collider.Bounds.Position;
            }*/

            // Update position
            transform.Position += collider.Velocity * gameTime.GetElapsedSeconds();

            // Update sprite
            if (sprite != null)
            {
                //var scale = (RectangleF) collider.Bounds;
                sprite.Scale = new Vector2(collider.Size.X, collider.Size.Y);
                sprite.Position = transform.Position;
                sprite.Update(gameTime);
            }

            /*if (collider != null)
            {
                collider.Bounds.Position = transform.Position;
                collider.PenetrationVectors.Clear();        
            }*/
        }
    }
}