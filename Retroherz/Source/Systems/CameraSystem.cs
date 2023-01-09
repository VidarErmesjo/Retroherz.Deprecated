using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;

using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems
{
    public class CameraSystem : EntityProcessingSystem
    {
        private readonly OrthographicCamera _camera;
		private readonly TiledMap _tiledMap;
		private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<PlayerComponent> _playerComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

		public OrthographicCamera Camera => _camera;

        public CameraSystem(OrthographicCamera camera, TiledMap tiledMap)
            : base(Aspect.All(typeof(ColliderComponent), typeof(PlayerComponent), typeof(TransformComponent)))
        {
            _camera = camera;
			_tiledMap = tiledMap;
        }

        ~CameraSystem() {}

        public override void Initialize(IComponentMapperService mapperService)
        {
			_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
			_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
			var player = _playerComponentMapper.Get(entityId);

			var collider = _colliderComponentMapper.Get(entityId);
			var transform = _transformComponentMapper.Get(entityId);

			// Follow player position and clamp to map dimentions
			var min = new Vector2(_camera.BoundingRectangle.Width, _camera.BoundingRectangle.Height) / 2;
			var max = new Vector2(_tiledMap.WidthInPixels, _tiledMap.HeightInPixels) - min;

			var lookAt = Vector2.Clamp(transform.Position + collider.Origin, Vector2.Round(min), Vector2.Round(max));
			_camera.LookAt(lookAt);
        }
    }
}