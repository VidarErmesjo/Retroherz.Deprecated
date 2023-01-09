using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled.Renderers;

using Retroherz.Components;

namespace Retroherz.Systems
{
    // DrawSystem => RenderSystem??
    public class RenderSystem : EntityDrawSystem, IDisposable
    {
        private bool _isDisposed = false;

        private readonly SpriteBatch _spriteBatch;
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
            _spriteBatch = new(graphicsDevice);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
            _spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
			var deltaTime = gameTime.GetElapsedSeconds();

            _spriteBatch.Begin(
                sortMode: SpriteSortMode.FrontToBack,
                blendState: BlendState.Additive,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix());

            foreach(var entityId in ActiveEntities)
            {
                var collider = _colliderComponentMapper.Get(entityId);
                var sprite = _spriteComponentMapper.Get(entityId);
                var transform = _transformComponentMapper.Get(entityId);

                // Sprite
                //if(sprite != null)
                sprite?.Draw(_spriteBatch);
			}
            _spriteBatch.End();
        }

        public virtual void Dispose(bool disposing)
        {
            if(_isDisposed)
                return;

            if(disposing)
            {
                _spriteBatch.Dispose();
                _renderTarget.Dispose();
            }

            _isDisposed = true;
        }

		~RenderSystem()
		{
			this.Dispose(false);
		}
    }
}