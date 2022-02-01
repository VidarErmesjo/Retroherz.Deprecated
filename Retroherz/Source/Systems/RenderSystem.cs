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
                var pilot = new RectangleF(physics.Position + physics.Velocity * deltaTime, physics.Size);
                _spriteBatch.DrawRectangle(pilot, Color.Red);

                // Search area
                var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
                var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);
                var inflated = new RectangleF(minimum, maximum - minimum);
                //inflated.Inflate(-1f, -1f);
                _spriteBatch.DrawRectangle(inflated, Color.Yellow);

                // Contac info
                var union = new RectangleF(physics.Position, physics.Size);
                foreach (var contact in physics.ContactInfo)
                {
                    // Normals
                    _spriteBatch.DrawPoint(contact.Item2, Color.BlueViolet, 4);
                    _spriteBatch.DrawPoint(contact.Item2, Color.Red, 4);
                    _spriteBatch.DrawLine(contact.Item2, contact.Item2 + contact.Item3 * 8, Color.Red);

                    // Rays
                    _spriteBatch.DrawLine(contact.Item2, contact.Item1.Position + contact.Item1.Origin, Color.BlueViolet);

                    // Inflated
                    _spriteBatch.DrawRectangle(contact.Item1.Position - physics.Origin, contact.Item1.Size + physics.Size, Color.BlueViolet);
                    
                    // Contacts
                    _spriteBatch.FillRectangle(contact.Item1.Position, contact.Item1.Size, Color.Yellow);

                    union = union.Union(new RectangleF(contact.Item1.Position, contact.Item1.Size));
                }
                _spriteBatch.DrawRectangle(union, Color.GreenYellow);


                // Embellish the "in contact" rectangles in yellow
                for (int i = 0; i < 4; i++)
                {
                    if (physics.Contact[i] != null)
                        _spriteBatch.DrawPoint(
                        physics.Contact[i].Position + physics.Contact[i].Origin, Color.Red, physics.Contact[i].Size.Length());
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