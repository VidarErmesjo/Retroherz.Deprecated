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
using PubSub;

using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class ColliderSystem
{
	// Courtesy of One Lone Coder
	// https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
	
	//[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal bool Constrained(
		int subjectId,
		int obstacleId,
		out Vector2 contactPoint,
		out Vector2 contactNormal,
		out float contactTime,
		float deltaTime)
	{
		contactPoint = Vector2.Zero;
		contactNormal = Vector2.Zero;
		contactTime = 0;
		float timeHitFar = 0;

		(ColliderComponent collider, TransformComponent transform) subject = new(
			_colliderComponentMapper.Get(subjectId),
			_transformComponentMapper.Get(subjectId)
		);

		// Early rejection
		if (subject.collider.Type == ColliderComponentType.Static) return false;

		// Check if subject is actually changing size
		//if (subject.collider.DeltaSize == Vector2.Zero) return false;

		(ColliderComponent collider, TransformComponent transform) obstacle = new(
			_colliderComponentMapper.Get(obstacleId),
			_transformComponentMapper.Get(obstacleId)
		);		

		// Expand obstacle collider box by subject dimensions
		(ColliderComponent collider, TransformComponent transform) rectangle = (
			new(
				size: obstacle.collider.Size + subject.collider.Size,
				velocity: obstacle.collider.Velocity,
				type: obstacle.collider.Type),
			new(position: obstacle.transform.Position - subject.collider.HalfExtents));

		// Cast rays
		foreach (var ray in subject.collider.Rays)
			if (Intersects(
				ray,
				rectangle,
				out contactPoint,
				out contactNormal,
				out contactTime,
				out timeHitFar))
					if (contactTime >= 0 && contactTime < 1)
					{
						Constraint contact = new(obstacleId, rectangle, contactPoint, contactNormal, contactTime);

						// Possible duplicates
						if (!subject.collider.Constraints.Contains(contact))
							subject.collider.Constraints.Add(contact);
					}

		return subject.collider.Contacts.Count > 0 ? true : false;
	}
}