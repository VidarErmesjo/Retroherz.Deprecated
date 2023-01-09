/*using System;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Utils
{
	public class Components : EntitySystem
	{
		private ComponentMapper<ColliderComponent> _colliderComponentMapper;
		private ComponentMapper<TransformComponent> _transformComponentMapper;

		public Components()
			: base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
		{
		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
			_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
		}

		public Nullable<(ColliderComponent collider, TransformComponent transform)> Get(int entityId)
		{ 
			if (GetEntity(entityId) is not null) return null;			

			var collider = _colliderComponentMapper.Get(entityId);
			var transform = _transformComponentMapper.Get(entityId);
			return (collider, transform);
		}


	}
}*/