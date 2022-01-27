using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class CameraSystem : EntityProcessingSystem
    {
        private readonly OrthographicCamera _camera;
        private ComponentMapper<PlayerComponent> _playerComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;
        public CameraSystem(OrthographicCamera camera)
            : base(Aspect.All(typeof(PlayerComponent), typeof(PhysicsComponent)))
        {
            _camera = camera;
        }

        ~CameraSystem() {}

        public override void Initialize(IComponentMapperService mapperService)
        {
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var physics = _physicsComponentMapper.Get(entityId);
            _camera.LookAt(physics.Position);
        }
    }
}