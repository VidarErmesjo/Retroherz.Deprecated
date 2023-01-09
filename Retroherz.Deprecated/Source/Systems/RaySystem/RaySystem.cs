using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
	// VisionSystem??????
	public partial class RaySystem : EntityUpdateSystem
	{
		private ComponentMapper<ColliderComponent> _colliderComponentMapper;
		private ComponentMapper<RayComponent> _rayComponentMapper;
		private ComponentMapper<TransformComponent> _transformComponentMapper;

		public RaySystem()
			: base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
		{
		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
			_rayComponentMapper = mapperService.GetMapper<RayComponent>();
			_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
		}

		public override void Update(GameTime gameTime)
		{
			var deltaTime = gameTime.GetElapsedSeconds();

			foreach (var arbiterId in ActiveEntities)
			{
				(ColliderComponent collider, RayComponent ray, TransformComponent transform) arbiter = new(
					_colliderComponentMapper.Get(arbiterId),
					_rayComponentMapper.Get(arbiterId),
					_transformComponentMapper.Get(arbiterId)
				);

				// Sort rays and targets
				if (arbiter.ray is not null)
				{
					arbiter.ray.Center = arbiter.transform.Position + arbiter.collider.Origin;

					// Find targets
					var circle = new CircleF(arbiter.ray.Center, arbiter.ray.Radius);		
					arbiter.ray.Hits.Clear();			
					foreach (var candidateId in ActiveEntities.Where(id => id != arbiterId))
					{
						(ColliderComponent collider, TransformComponent transform) candidate = new(
							_colliderComponentMapper.Get(candidateId),
							_transformComponentMapper.Get(candidateId)
						);

						var inflated = Utils.ProspectiveBoundingRectangle(candidate.collider, candidate.transform, deltaTime);

						if (circle.Intersects(inflated))
						{
							var origin = Utils.Prospective(arbiter.collider, arbiter.transform, deltaTime);
							var target = Utils.Prospective(candidate.collider, candidate.transform, deltaTime);
							var distance = Vector2.Distance(origin, target);
							var hit = new Hit(candidateId, distance);
							arbiter.ray.Hits.Add(hit);
						}
					}
				}
			}
		}
	}
}
