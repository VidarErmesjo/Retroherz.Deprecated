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
    public class ExpirySystem : EntityProcessingSystem
    {
        private ComponentMapper<ExpiryComponent> _expiryMapper;

        public ExpirySystem() 
            : base(Aspect.All(typeof(ExpiryComponent)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _expiryMapper = mapperService.GetMapper<ExpiryComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var expiry = _expiryMapper.Get(entityId);
            expiry.TimeRemaining -= gameTime.GetElapsedSeconds();
            
            if (expiry.TimeRemaining <= 0)
                DestroyEntity(entityId);
        }
    }
}