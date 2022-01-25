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
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public RenderSystem(GraphicsDevice graphicsDevice, OrthographicCamera camera)
            : base(Aspect.All(typeof(SpriteComponent), typeof(PhysicsComponent)))
        {
            _camera = camera;
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
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
                var sprite = _spriteComponentMapper.Get(entityId);
                var physics = _physicsComponentMapper.Get(entityId);

                sprite.Position = physics.Position;
                sprite.Render(_spriteBatch);

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