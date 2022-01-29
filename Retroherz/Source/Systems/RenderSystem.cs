using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using Retroherz.Components;

namespace Retroherz.Systems
{
    // DrawSystem => RenderSystem??
    public class RenderSystem : EntityDrawSystem, IDisposable
    {
        private bool _isDisposed = false;

        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrthographicCamera _camera;
        private readonly RenderTarget2D _renderTarget;

        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        //private ComponentMapper<RectangularColliderComponent> _colliderComponentMapper;
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public RenderSystem(GraphicsDevice graphicsDevice, OrthographicCamera camera)
            : base(Aspect
                .All(typeof(SpriteComponent), typeof(PhysicsComponent))
                .One(typeof(ColliderComponent), typeof(RectangularColliderComponent)))
        {
            _camera = camera;
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            //_colliderComponentMapper = mapperService.GetMapper<RectangularColliderComponent>();
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.Additive,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix());

            var deltaTime = gameTime.GetElapsedSeconds();
            foreach(var entityId in ActiveEntities)
            {
                var collider = _colliderComponentMapper.Get(entityId);
                var sprite = _spriteComponentMapper.Get(entityId);
                var physics = _physicsComponentMapper.Get(entityId);

                // SpriteSystem
                sprite.Scale = new Vector2(physics.Size.X, physics.Size.Y);
                sprite.Position = physics.Position;
                sprite.Update(gameTime);
                sprite.Draw(_spriteBatch);

                _spriteBatch.DrawRectangle(physics.Position, physics.Size, Color.Green);

                var deltaPosition = physics.Position + physics.Velocity * 16 * deltaTime;
                _spriteBatch.DrawRectangle(deltaPosition, physics.Size, Color.Red);
                var inflated = new RectangleF(physics.Position, physics.Size);
                var factor = physics.Velocity.NormalizedCopy() * physics.Size * physics.Velocity.Length() * deltaTime; //* physics.Size.Length() * deltaTime;

                //IShapeF circle = new CircleF(physics.Position + physics.Size / 2, factor * 4);
                //_spriteBatch.DrawCircle((CircleF) circle, 32, Color.White);

                inflated.Inflate(MathF.Abs(factor.X), MathF.Abs(factor.Y));
                _spriteBatch.DrawRectangle(inflated, Color.Yellow);
 
                // Embellish the "in contact" rectangles in yellow
                for (int i = 0; i < 4; i++)
                {
                    if (physics.Contact[i] != null)
                        _spriteBatch.FillRectangle(
                            physics.Contact[i].Position, physics.Contact[i].Size, Color.Yellow);
                    physics.Contact[i] = null;
                }
                /*WeaponComponent weapon = _weaponMapper.Get(entity);
                SuperSprite sprite = _spriteMapper.Get(entity);

                sprite.Draw(_spriteBatch);

                if(weapon.isCharging)
                {
                    _spriteBatch.DrawLine(
                        weapon.origin.X,
                        weapon.origin.Y,
                        weapon.destination.X,
                        weapon.destination.Y,
                        new Color
                        {
                            R = 0,
                            G = 255,
                            B = 0,
                            A = (byte) weapon.charge
                        },
                        weapon.charge);
                }
                else if(weapon.charge > 0.0f)
                {
                    _spriteBatch.DrawLine(
                        weapon.origin.X,
                        weapon.origin.Y,
                        weapon.destination.X,
                        weapon.destination.Y,
                        new Color
                        {
                            R = 255,
                            G = 0,
                            B = 0,
                            A = (byte) weapon.charge
                        },
                        weapon.charge);
                }*/
            }

            _spriteBatch.End();
        }

        /*public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }*/

        public virtual void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
                _graphicsDevice.Dispose();
                _spriteBatch.Dispose();
                _renderTarget.Dispose();
            }

            _isDisposed = true;
        }

    }
}