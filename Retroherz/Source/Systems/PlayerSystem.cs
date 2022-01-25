using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class PlayerSystem : EntityUpdateSystem
    {
        private readonly OrthographicCamera _camera;
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<CircularColliderComponent> _circularComponentMapper;
        private ComponentMapper<PlayerComponent> _playerComponentMapper;
        private ComponentMapper<RectangularColliderComponent> _rectangularColliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        // PlayerSystem??
        public PlayerSystem(OrthographicCamera camera)
            : base(Aspect
                .All(
                    typeof(SpriteComponent),
                    typeof(PlayerComponent),
                    typeof(PhysicsComponent))
                .One(
                    typeof(CircularColliderComponent),
                    typeof(RectangularColliderComponent)))
        {
            _camera = camera;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _circularComponentMapper = mapperService.GetMapper<CircularColliderComponent>();
            _playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
            _rectangularColliderComponentMapper = mapperService.GetMapper<RectangularColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var deltaTime = gameTime.GetElapsedSeconds();
            foreach(int entityId in ActiveEntities)
            {
                var player = _playerComponentMapper.Get(entityId);
                var sprite = _spriteComponentMapper.Get(entityId);
                var physics = _physicsComponentMapper.Get(entityId);
                var circularCollider = _circularComponentMapper.Get(entityId);
                var collider = _rectangularColliderComponentMapper.Get(entityId);

                // Calculate velocity from direction
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    physics.Direction = Vector2.Normalize(_camera.ScreenToWorld(
                        //new Vector2(mouseState.X, mouseState.Y)) - physics.Position - collider.Size / 2);
                        new Vector2(mouseState.X, mouseState.Y)) - physics.Position - Vector2.One * collider.Size / 2);

                    physics.Velocity += physics.Direction * player.MaxSpeed * ((float)deltaTime);
                    //MathHelper.Clamp(physics.Velocity.X, 0, 1);
                    sprite.Play("Walk");
                }
                else
                {
                    physics.Velocity = new Vector2(
                        MathHelper.LerpPrecise(physics.Velocity.X, 0, 1f * ((float)deltaTime)),
                        MathHelper.LerpPrecise(physics.Velocity.Y, 0, 1f * ((float)deltaTime)));

                    sprite.Play("Idle");
                }
                // Animate
                //sprite.Position = physics.Position;
                sprite.Update(gameTime);

                // Update position
                if(collider.hasCollided)
                {
                    physics.Position = collider.Position;
                    physics.Velocity = collider.Velocity; // -physics.Velocity for knock-back effect
                }
                else
                {
                    physics.Position += physics.Velocity * deltaTime;
                    collider.Velocity = physics.Velocity;
                }

                collider.hasCollided = false;

                /*if(circularCollider.hasCollided)
                {
                    physics.Position = circularCollider.Position;
                    //physics.Velocity = Vector2.Zero; // -circularCollider.Velocity; knockback-ish
                }
                else
                {
                    physics.Position += physics.Velocity * deltaTime;
                }

                circularCollider.hasCollided = false;*/

                // Update camera position
                _camera.LookAt(physics.Position);
            }
        }
    }
}