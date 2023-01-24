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

using Retroherz.Collections;
using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;
public partial class ColliderSystem : EntityUpdateSystem
{
	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<MetaComponent> _metaComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	public readonly SpanBag<int> Obstacles = new SpanBag<int>();

	public ColliderSystem()
		: base(Aspect.All(typeof(ColliderComponent), typeof(MetaComponent), typeof(TransformComponent)))
	{
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

		foreach (int entityId in ActiveEntities.AsReadOnlySpan())
		{
			(ColliderComponent collider, MetaComponent meta, TransformComponent transform) ego = (
				_colliderComponentMapper.Get(entityId),
				_metaComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			// Do not process
			if (ego.collider.Type == ColliderComponentType.Static) continue;

			// Calculate ray vectors
			/*var position = ego.transform.Position + ego.collider.Origin;
			var direction = (ego.collider.Size - ego.collider.Origin + ego.collider.DeltaOrigin);
			direction = ego.collider.DeltaOrigin;
			var acceleration = ego.collider.Velocity * deltaTime;
			ego.collider.Rays[0] = new(position, direction * -Vector.UnitY + acceleration);		// Up
			ego.collider.Rays[1] = new(position, direction * Vector.UnitX + acceleration);		// Right
			ego.collider.Rays[2] = new(position, direction * Vector.UnitY + acceleration);		// Down
			ego.collider.Rays[3] = new(position, direction * -Vector.UnitX  + acceleration);	// Left*/


			// Update position if size has changed
			//if (ego.collider.DeltaSize != Vector.Zero)
				//ego.transform.Position += -ego.collider.DeltaOrigin;
		
			// Fun
			// MOVE TO => ActorSystem / AISystem?
			if (ego.meta.Type == MetaComponentType.NPC)
			{
				if (ego.collider.Velocity.X == 0.0f || ego.collider.Velocity.Y == 0.0f)
				{
					Vector randomVector = Vector.Random();

					// Buggy!
					ego.collider.Velocity = Vector.Clamp(randomVector, -Vector.One, Vector.One) * ego.collider.Size;
				}
			}

			// Clear and add all candidates for collision check (self excluded)
			Obstacles.Clear();
			BoundingRectangle inflated = Predictive.BoundingRectangle(
				ego.collider,
				ego.transform,
				deltaTime,
				false
			);

			foreach(int obstacleId in ActiveEntities.AsReadOnlySpan())	// OPT??
			//foreach (var obstacleId in ActiveEntities.Where(id => id != entityId))
			{
				if (obstacleId == entityId)
					continue;

				(ColliderComponent collider, TransformComponent transform) obstacle = new(
					_colliderComponentMapper.Get(obstacleId),
					_transformComponentMapper.Get(obstacleId)
				);

				// Add if will collide
				if (inflated.Intersects(Predictive.BoundingRectangle(
					obstacle.collider,
					obstacle.transform,
					deltaTime,
					false
				)))
					Obstacles.Add(obstacleId);
			}

			// Sort collision in order of distance
			Vector contactPoint = Vector.Zero;
			Vector contactNormal = Vector.Zero;
			float contactTime = 0;

			// Work out collisions ...
			ego.collider.Constraints.Clear();
			ego.collider.Contacts.Clear();
			foreach (int obstacleId in Obstacles)
			{
				(ColliderComponent collider, TransformComponent transform) obstacle = (
					_colliderComponentMapper.Get(obstacleId),
					_transformComponentMapper.Get(obstacleId)
				);

				/*if (Constrained(
					entityId,
					obstacleId,
					out contactPoint,
					out contactNormal,
					out contactTime,
					deltaTime))
				{
				}*/

				if (Collides(
					(ego.collider, ego.transform),
					(obstacle.collider, obstacle.transform),
					out contactPoint,
					out contactNormal,
					out contactTime,
					deltaTime))
				{					
					// On collision... add it to contact information for resolves, effects, visuals etc.
					(int, Vector, Vector, float) contact = (
						obstacleId,
						contactPoint,
						contactNormal,
						contactTime
					);

					ego.collider.Contacts.Add(contact);
				}
			}

			/*if (ego.collider.Constraints.Count > 0)
			{
				// Do the sort
				var constraints = ego.collider.Constraints.AsSpan();
				constraints.Sort((a, b) => a.ContactTime < b.ContactTime ? -1 : 1);

				// Resolve all constraints (hopefully)
				foreach (var constraint in constraints)
				{
					(ColliderComponent collider, TransformComponent transform) obstacle = new(
						_colliderComponentMapper.Get(constraint.Id),
						_transformComponentMapper.Get(constraint.Id)
					);

					Constrain(
						(ego.collider, ego.transform),
						(obstacle.collider, obstacle.transform),
						deltaTime);
						{

						}
				}
			}*/

			if (ego.collider.Contacts.Count > 0)
			{
				// Do the sort
				var contacts = ego.collider.Contacts.AsSpan();
				contacts.Sort((a, b) => a.ContactTime < b.ContactTime ? -1 : 1 );

				// Now resolve the collision in correct order (shortest time)
				foreach (var contact in contacts)
				{
					(ColliderComponent collider, TransformComponent transform) obstacle = new(
						_colliderComponentMapper.Get(contact.Id),
						_transformComponentMapper.Get(contact.Id)
					);

					// Attach / Put "Hit"Component?
					if (Resolve(
						(ego.collider, ego.transform),
						(obstacle.collider, obstacle.transform),
						deltaTime))
					{
					}
				}
			}

			// Update position DANGEROUS
			/*if (ego.collider.DeltaOrigin != Vector.Zero)
				ego.transform.Position -= ego.collider.DeltaOrigin;*/
		}
	}

	~ColliderSystem() {}
}