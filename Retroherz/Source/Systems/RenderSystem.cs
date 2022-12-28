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
        private ComponentMapper<SpriteComponent> _spriteComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

        public RenderSystem(GraphicsDevice graphicsDevice, OrthographicCamera camera)
            : base(Aspect
                .All(typeof(ColliderComponent), typeof(SpriteComponent), typeof(TransformComponent)))
        {
            _camera = camera;
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
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
                var transform = _transformComponentMapper.Get(entityId);

                // Sprite
                if(sprite != null)
                sprite.Draw(_spriteBatch);

                // Bounding rectangle
                var bounds = new RectangleF(transform.Position, collider.Size);
                _spriteBatch.DrawRectangle(bounds, Color.Green);

                // Pilot rectangle
                var pilot = new RectangleF(transform.Position + collider.Velocity * deltaTime, collider.Size);
                _spriteBatch.DrawRectangle(pilot, Color.Red);

                // Search area
                var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
                var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);
                var inflated = new RectangleF(minimum, maximum - minimum);
                _spriteBatch.DrawRectangle(inflated, Color.Yellow);

                // Contac info
                foreach (var contact in collider.ContactInfo)
                {
					var origin = contact.contactPoint + collider.Velocity * deltaTime;
                    // Normals
                    _spriteBatch.DrawPoint(origin, Color.BlueViolet, 4);
                    _spriteBatch.DrawPoint(origin, Color.Red, 4);
                    _spriteBatch.DrawLine(origin, origin + contact.contactNormal * 8, Color.Red);

                    // Rays
                    _spriteBatch.DrawLine(
                        origin,
                        contact.target.tranform.Position + contact.target.collider.Origin,
                        Color.BlueViolet);

                    // Inflated
                    _spriteBatch.DrawRectangle(
                        contact.target.tranform.Position - collider.Origin,
                        contact.target.collider.Size + collider.Size,
                        Color.BlueViolet);
                    
                    // Contacts
                    _spriteBatch.FillRectangle(
                        contact.target.tranform.Position,
                        contact.target.collider.Size,
                        Color.Yellow);

                }
                collider.ContactInfo.Clear();

                if (collider.Bounds != null)
                    if(collider.Type == ColliderComponentType.Border)
                        _spriteBatch.FillRectangle((RectangleF) collider.Bounds, Color.Blue);
                    else
                        _spriteBatch.DrawRectangle((RectangleF) collider.Bounds, Color.Red);


                // Embellish the "in contact" rectangles in yellow
                /*for (int i = 0; i < 4; i++)
                {
                    if (collider.Contact[i] != null)
                        _spriteBatch.DrawPoint(
                        collider.Contact[i].Value.Item2.Position + collider.Contact[i].Value.Item1.Origin,
                        Color.Red,
                        collider.Contact[i].Value.Item1.Size.Length());
                    collider.Contact[i] = null;
                }*/
            }

            _spriteBatch.End();
        }

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