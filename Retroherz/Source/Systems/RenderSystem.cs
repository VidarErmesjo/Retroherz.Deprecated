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

                // Bounding rectangle
                var bounds = new RectangleF(physics.Position, physics.Size);
                _spriteBatch.DrawRectangle(bounds, Color.Green);

                // Pilot rectangle
                var deltaPosition = physics.Position + physics.Velocity * physics.Size * deltaTime;
                var pilot = new RectangleF(deltaPosition, physics.Size);
                _spriteBatch.DrawRectangle(pilot, Color.Red);

                var inflated = new RectangleF(physics.Position, physics.Size);
                var factor = physics.Velocity * deltaTime; //* physics.Size.Length() * deltaTime;
                factor = physics.Position - deltaPosition;
                inflated.Inflate(MathF.Abs(factor.X), MathF.Abs(factor.Y));

                var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
                var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);

                // Search area
                inflated = new RectangleF(minimum, maximum - minimum);
                _spriteBatch.DrawRectangle(inflated, Color.Yellow);

                // Rays
                foreach (var ray in physics.Rays)
                {
                    _spriteBatch.DrawPoint(ray.Item1, Color.BlueViolet, 4);
                    _spriteBatch.DrawPoint(ray.Item2, Color.Red, 4);
                    _spriteBatch.DrawLine(ray.Item2, ray.Item2 + ray.Item3 * 4, Color.White);

                    // Inflated rays
                    foreach (var area in physics.Inflated)
                        _spriteBatch.DrawLine(ray.Item1, area.Position + area.Size / 2, Color.BlueViolet);
                }

                // Inflated
                foreach (var area in physics.Inflated)
                    _spriteBatch.DrawRectangle(area.Position, area.Size, Color.BlueViolet);

                // Embellish the "in contact" rectangles in yellow
                /*for (int i = 0; i < 4; i++)
                {
                    if (physics.Contact[i] != null)
                        _spriteBatch.FillRectangle(
                            physics.Contact[i].Position, physics.Contact[i].Size, Color.Yellow);
                    physics.Contact[i] = null;
                }*/

                foreach (var contact in physics.Contacts)
                    for (var i = 0; i < 4; i++)
                    {
                        if (contact[i] != null)
                            _spriteBatch.FillRectangle(contact[i].Position, contact[i].Size, Color.Yellow);

                        contact[i] = null;
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