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
using Retroherz.Math;

namespace Retroherz.Systems;
public partial class ColliderSystem : EntityUpdateSystem
{
	// EXP
	public static Bag<int> Colliders = new();
	// Bag of value tuples to hold collision candidates (after broad phase)
	private readonly Bag<int> _candidates = new();

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<MetaComponent> _metaComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

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
			// Get entity components
			(ColliderComponent collider, MetaComponent meta, TransformComponent transform) entity = new(
				_colliderComponentMapper.Get(entityId),
				_metaComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			// Do not process
			if (entity.collider.Type == ColliderComponentType.Static) continue;

			// Calculate ray vectors
			/*var position = entity.transform.Position + entity.collider.Origin;
			var direction = (entity.collider.Size - entity.collider.Origin + entity.collider.DeltaOrigin);
			direction = entity.collider.DeltaOrigin;
			var acceleration = entity.collider.Velocity * deltaTime;
			entity.collider.Rays[0] = new(position, direction * -Vector.UnitY + acceleration);		// Up
			entity.collider.Rays[1] = new(position, direction * Vector.UnitX + acceleration);		// Right
			entity.collider.Rays[2] = new(position, direction * Vector.UnitY + acceleration);		// Down
			entity.collider.Rays[3] = new(position, direction * -Vector.UnitX  + acceleration);	// Left*/


			// Update position if size has changed
			//if (entity.collider.DeltaSize != Vector.Zero)
				//entity.transform.Position += -entity.collider.DeltaOrigin;
		
			// Fun
			// MOVE TO => ActorSystem / AISystem?
			if (entity.meta.Type == MetaComponentType.NPC)
			{
				if (entity.collider.Velocity.X == 0.0f || entity.collider.Velocity.Y == 0.0f)
				{
					Vector randomVector = Vector.Random();

					// Buggy!
					entity.collider.Velocity = Vector.Clamp(randomVector, -Vector.One, Vector.One) * entity.collider.Size;
				}
			}

			// Clear and add all candidates for collision check (self excluded)
			_candidates.Clear();
			var inflated = Predictive.BoundingRectangle(
				entity.collider,
				entity.transform,
				deltaTime,
				false);

			foreach(int candidateId in ActiveEntities.AsReadOnlySpan())	// OPT??
			//foreach (var candidateId in ActiveEntities.Where(id => id != entityId))
			{
				if (candidateId == entityId)
					continue;

				(ColliderComponent collider, TransformComponent transform) candidate = new(
					_colliderComponentMapper.Get(candidateId),
					_transformComponentMapper.Get(candidateId)
				);

				// Add if will collide
				if (inflated.Intersects(Predictive.BoundingRectangle(
					candidate.collider,
					candidate.transform,
					deltaTime,
					false)))
					_candidates.Add(candidateId);
			}

			// Sort collision in order of distance
			Vector contactPoint = Vector.Zero;
			Vector contactNormal = Vector.Zero;
			float contactTime = 0;

			// Work out collisions ...
			entity.collider.Constraints.Clear();
			entity.collider.Contacts.Clear();
			foreach (int candidateId in _candidates.ToArray().AsSpan())	// OPT?
			//foreach (var candidateId in _candidates)
			{
				(ColliderComponent collider, TransformComponent transform) candidate = new(
					_colliderComponentMapper.Get(candidateId),
					_transformComponentMapper.Get(candidateId)
				);

				/*if (Constrained(
					entityId,
					candidateId,
					out contactPoint,
					out contactNormal,
					out contactTime,
					deltaTime))
				{
				}*/

				if (Collides(
					(entity.collider, entity.transform),
					(candidate.collider, candidate.transform),
					out contactPoint,
					out contactNormal,
					out contactTime,
					deltaTime))
				{					
					// On collision... add it to contact information for resolves, effects, visuals etc.
					(int, Vector, Vector, float) contact = new(
						candidateId,
						contactPoint,
						contactNormal,
						contactTime
					);

					entity.collider.Contacts.Add(contact);
				}
			}

			if (entity.collider.Constraints.Count > 0)
			{
				// Do the sort
				var constraints = entity.collider.Constraints.ToArray().AsSpan();
				constraints.Sort((a, b) => a.ContactTime < b.ContactTime ? -1 : 1);

				// Resolve all constraints (hopefully)
				foreach (var constraint in constraints)
				{
					(ColliderComponent collider, TransformComponent transform) candidate = new(
						_colliderComponentMapper.Get(constraint.Id),
						_transformComponentMapper.Get(constraint.Id)
					);

					Constrain(
						(entity.collider, entity.transform),
						(candidate.collider, candidate.transform),
						deltaTime);
						{

						}
				}
			}

			if (entity.collider.Contacts.Count > 0)
			{
				// Do the sort
				var contacts = entity.collider.Contacts.ToArray().AsSpan();
				contacts.Sort((a, b) => a.ContactTime < b.ContactTime ? -1 : 1 );

				// Now resolve the collision in correct order (shortest time)
				foreach (var contact in contacts)
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
					}
				}
			}

			// Update position DANGEROUS
			/*if (entity.collider.DeltaOrigin != Vector.Zero)
				entity.transform.Position -= entity.collider.DeltaOrigin;*/
		}
	}

	~ColliderSystem() {}
}