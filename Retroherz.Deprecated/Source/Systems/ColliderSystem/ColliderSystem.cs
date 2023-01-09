using System;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public partial class ColliderSystem : EntityUpdateSystem
    {
		private readonly Bag<int> _candidates;

        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
		private ComponentMapper<MetaComponent> _metaComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

        public ColliderSystem()
            : base(Aspect.All(typeof(ColliderComponent), typeof(MetaComponent), typeof(TransformComponent)))
        {
            // Bag of value tuples to hold collision candidates (after broad phase)
            _candidates = new();
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
			_metaComponentMapper = mapperService.GetMapper<MetaComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            var deltaTime = gameTime.GetElapsedSeconds();

            foreach (var entityId in ActiveEntities)
            {
				// Get entity components
				(ColliderComponent collider, MetaComponent meta, TransformComponent transform) entity = new(
					_colliderComponentMapper.Get(entityId),
					_metaComponentMapper.Get(entityId),
					_transformComponentMapper.Get(entityId)
				);

				// Update position if size has changed
				//if (entity.collider.DeltaSize != Vector2.Zero)
					//entity.transform.Position += -entity.collider.DeltaOrigin;
			
				// Fun
				// MOVE TO => ActorSystem / AISystem?
				if (entity.meta.Type == MetaComponentType.NPC)
				{
					if (entity.collider.Velocity.X == 0.0f || entity.collider.Velocity.Y == 0.0f)
					{
						var randomVector = Vector2.Zero;
						Random.Shared.NextUnitVector(out randomVector);

						// Buggy!
						entity.collider.Velocity = Vector2.Clamp(randomVector, -Vector2.One, Vector2.One) * entity.collider.Size;
					}
				}

	            // Clear and add all candidates for collision check (self excluded)
                _candidates.Clear();
				var inflated = Utils.ProspectiveRectangle(entity.collider, entity.transform, deltaTime);
				foreach (var candidateId in ActiveEntities.Where(id => id != entityId))
				{
					(ColliderComponent collider, TransformComponent transform) candidate = new(
						_colliderComponentMapper.Get(candidateId),
						_transformComponentMapper.Get(candidateId)
					);

					// Add if will collide
					if (inflated.Intersects(new(candidate.transform.Position, candidate.collider.Size)))
						_candidates.Add(candidateId);
				}

                // Sort collision in order of distance
                var contactPoint = Vector2.Zero;
                var contactNormal = Vector2.Zero;
                var contactTime = 0f;

                // Work out collisions ...
				entity.collider.Clear();
                foreach (var candidateId in _candidates)
                {
					(ColliderComponent collider, TransformComponent transform) candidate = new(
						_colliderComponentMapper.Get(candidateId),
						_transformComponentMapper.Get(candidateId)
					);

					// This should idealy be updated when size changes (how ????)
					// Update position if size has changed
					//if (candidate.collider.DeltaSize != Vector2.Zero)
						//candidate.transform.Position += -candidate.collider.DeltaOrigin;

                    if (Collides(
                        (entity.collider, entity.transform),
                        (candidate.collider, candidate.transform),
                        out contactPoint,
                        out contactNormal,
                        out contactTime,
                        deltaTime))
                    {
                        // On collision... add it to contact information for resolves, effects, visuals etc.
						var contact = new Contact(candidateId, contactPoint, contactNormal, contactTime);
						entity.collider.Add(contact);
                    }
                } 

                if (entity.collider.Contacts.Length > 0)
                {
                    // Now resolve the collision in correct order (shortest time)
					foreach (var contact in entity.collider.Contacts)
                    {
						(ColliderComponent collider, TransformComponent transform) candidate = new(
							_colliderComponentMapper.Get(contact.Id),
							_transformComponentMapper.Get(contact.Id)
						);

						// Attach / Put "Hit"Component?
						if (Resolve(
							(entity.collider, entity.transform),
							(candidate.collider, candidate.transform),
							deltaTime))
						{
							/*if (candidate.collider.Type == ColliderComponentType.Dynamic)
							{

							}*/
							/*if (contact.Id != entityId && candidate.collider.Velocity != Vector2.Zero)
							{
								var entity = GetEntity(contact.Id);
								entity.Attach(new ExpiryComponent(Random.Shared.NextSingle()));
							}*/

						}
                        /*if(Resolve((collider, transform), (target.collider, target.transform), deltaTime));
							if (meta.Type == MetaComponentType.Player && target.meta.Type == MetaComponentType.Wall)
								if (target.meta.Selected)
									DestroyEntity(target.meta.Id);*/
                    }
                }

				// Update position
				entity.transform.Position += entity.collider.Velocity * deltaTime;
            }
        }

        ~ColliderSystem() {}
	}
}