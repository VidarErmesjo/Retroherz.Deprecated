using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems
{
	// VisionSystem??????
	public partial class RaySystem : EntityUpdateSystem
	{
		private readonly Bag<(Vector Position, double Angle, double Distance)> _edges = new(4);

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
				(ColliderComponent collider, RayComponent ray, TransformComponent transform) subject = new(
					_colliderComponentMapper.Get(arbiterId),
					_rayComponentMapper.Get(arbiterId),
					_transformComponentMapper.Get(arbiterId)
				);

				// Sort rays and targets
				if (subject.ray is not null)
				{
					subject.ray.Center = subject.transform.Position + subject.collider.Origin;

					// Find and sort targets
					var circle = new CircleF(subject.ray.Center, subject.ray.Radius);
					var rectangle = RectangleF.Empty;
;					subject.ray.Hits.Clear();			
					foreach (var candidateId in ActiveEntities.Where(id => id != arbiterId ))
					{
						(ColliderComponent collider, TransformComponent transform) candidate = new(
							_colliderComponentMapper.Get(candidateId),
							_transformComponentMapper.Get(candidateId)
						);

						var inflated = Predictive.BoundingRectangle(
							candidate.collider,
							candidate.transform,
							deltaTime);

						//if (circle.Intersects(inflated))
						{
							//var origin = Predictive.Vector(subject.collider, subject.transform, deltaTime);
							Vector origin = subject.transform.Position + subject.collider.Origin;

							RectangleF corners = new(
								candidate.transform.Position,
								candidate.collider.Size
							);

							// Clear and repopulate
							_edges.Clear();
							foreach (var corner in corners.GetCorners())
							{
								_edges.Add((
									corner,
									Vector.Angle(origin, corner),
									Vector.Distance(origin, corner)
								));
							}

							// Sort by distance
							var sorted = _edges.ToArray().AsSpan();
							sorted.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);

							// Reject nearest and farthest - keep "sides"
							subject.ray.Hits.Add(new(candidateId, new (Vector, double, double)[2] {sorted[1], sorted[2]}));
						}
					}

					// EXP

					/*if (subject.ray.Hits.Count > 0)
					{
						var hits = subject.ray.Hits.ToArray().AsSpan();
						hits.Sort((a, b) => a.Distance < b.Distance ? -1 : 1);

						var points = new Vector[4];
						var rectangle = RectangleF.Empty;
						foreach (var hit in hits)
						{
							(ColliderComponent collider, TransformComponent transform) candidate = new(
								_colliderComponentMapper.Get(hit.Id),
								_transformComponentMapper.Get(hit.Id)
							);

							rectangle = new(candidate.transform.Position + candidate.collider.Origin, candidate.collider.Origin);

							// Clockwise
							points[0] = rectangle.TopLeft;
							points[1] = rectangle.TopRight;
							points[2] = rectangle.BottomRight;
							points[3] = rectangle.BottomLeft;

							foreach (var point in points)
							{
								
							}


						}
					}*/
				}
			}
		}
	}
}
