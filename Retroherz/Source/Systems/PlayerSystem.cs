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
    public class PlayerSystem : EntityProcessingSystem
    {
        private readonly OrthographicCamera _camera;
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<PlayerComponent> _playerComponentMapper;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public PlayerSystem(OrthographicCamera camera)
            : base(Aspect
                .All(
                    typeof(SpriteComponent),
                    typeof(PlayerComponent),
                    typeof(PhysicsComponent))
                .One(typeof(ColliderComponent)))
        {
            _camera = camera;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            var deltaTime = gameTime.GetElapsedSeconds();
            
            var player = _playerComponentMapper.Get(entityId);
            var sprite = _spriteComponentMapper.Get(entityId);
            var physics = _physicsComponentMapper.Get(entityId);
            var collider = _colliderComponentMapper.Get(entityId);

            if (keyboardState.GetPressedKeyCount() > 0)
            {
                physics.Direction = Vector2.Zero;
                if (keyboardState.IsKeyDown(Keys.Up))
                    physics.Direction += -Vector2.UnitY;
                if (keyboardState.IsKeyDown(Keys.Down))
                    physics.Direction += Vector2.UnitY;
                if (keyboardState.IsKeyDown(Keys.Left))
                    physics.Direction += -Vector2.UnitX;
                if (keyboardState.IsKeyDown(Keys.Right))
                    physics.Direction += Vector2.UnitX;

                physics.Direction.Normalize();
                physics.Velocity += physics.Direction * player.MaxSpeed * deltaTime * 2;

                sprite.Play("Walk");
            }
            else if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Accelerate
                physics.Direction = Vector2.Normalize(_camera.ScreenToWorld(
                    new Vector2(mouseState.X, mouseState.Y)) - physics.Position - physics.Origin);

                physics.Velocity += physics.Direction * player.MaxSpeed * deltaTime;
                
                physics.Velocity = new Vector2((
                    MathHelper.Clamp(physics.Velocity.X, -player.MaxSpeed, player.MaxSpeed)),
                    MathHelper.Clamp(physics.Velocity.Y, -player.MaxSpeed, player.MaxSpeed));

                sprite.Play("Walk");
            }
            else sprite.Play("Idle");

            // Slow down
            var factor = deltaTime * -1;
            physics.Velocity = new Vector2(
                MathHelper.LerpPrecise(0, physics.Velocity.X, MathF.Pow(2, factor)),
                MathHelper.LerpPrecise(0, physics.Velocity.Y, MathF.Pow(2, factor)));

            //sprite.Play("Idle");
                        //System.Console.WriteLine(physics.Velocity);


            // Can haz floatie camera??
           /*physics.Position = new Vector2(
                MathHelper.LerpPrecise(physics.Position.X, _camera.Center.X, 0.1f * deltaTime),
                MathHelper.LerpPrecise(physics.Position.Y, _camera.Center.Y, 0.1F * deltaTime));*/

            // Update camera
            _camera.LookAt(physics.Position + physics.Origin);
        }
    }
}