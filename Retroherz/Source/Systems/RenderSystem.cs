using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class RenderSystem : EntityDrawSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrthographicCamera _camera;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public RenderSystem(GraphicsDevice graphicsDevice, OrthographicCamera camera)
            : base(Aspect
                .All(typeof(SpriteComponent), typeof(PhysicsComponent))
                .One(typeof(ColliderComponent)))
        {
            _camera = camera;
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
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

            foreach(var entityId in ActiveEntities)
            {
                var collider = _colliderComponentMapper.Get(entityId);
                var sprite = _spriteComponentMapper.Get(entityId);
                var physics = _physicsComponentMapper.Get(entityId);

                sprite.Position = physics.Position;
                sprite.Update(gameTime);
                sprite.Draw(_spriteBatch);

                _spriteBatch.DrawRectangle(collider, Color.Green);

                // Embellish the "in contact" rectangles in yellow
                for (int i = 0; i < 4; i++)
                {
                    if (collider.Contact[i] != null)
                        _spriteBatch.FillRectangle(collider.Contact[i].Bounds.Position, collider.Contact[i], Color.Yellow);
                    collider.Contact[i] = null;
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
    }
}