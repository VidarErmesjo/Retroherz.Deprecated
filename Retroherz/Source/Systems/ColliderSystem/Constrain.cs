using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;

using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class ColliderSystem
{
	// Courtesy of One Lone Coder
	// https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp        

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Constrain(
		(ColliderComponent collider, TransformComponent transform) subject,
		(ColliderComponent collider, TransformComponent transform) obstacle,
		float deltaTime)
	{
		Vector2 contactPoint = Vector2.Zero;
		Vector2 contactNormal = Vector2.Zero;
		float contactTime = 0;
		float timeHitFar = 0;

		// Expand obstacle collider box by subject dimensions
		(ColliderComponent collider, TransformComponent transform) rectangle = (
			new(
				size: obstacle.collider.Size + subject.collider.Size,
				velocity: obstacle.collider.Velocity,
				type: obstacle.collider.Type),
			new(position: obstacle.transform.Position - subject.collider.Origin));

		foreach (var ray in subject.collider.Rays)
			if (Intersects(
				ray,
				rectangle,
				out contactPoint,
				out contactNormal,
				out contactTime,
				out timeHitFar))
					if (contactTime >= 0f && contactTime < 1f)
			{
				// Resolve diagonal freak case (single contact).
				/*if (contactNormal == Vector2.Zero && subject.collider.Constraints.Count == 1)
				{
					contactNormal = Vector2.Clamp(-subject.collider.Velocity, -Vector2.One, Vector2.One);
					System.Console.WriteLine("Diagonal!");
				}*/

				var force = contactNormal * subject.collider.DeltaOrigin;// * (1 - MathF.Abs(contactTime));
				//force = contactNormal * Vector2.One;

				subject.collider.Velocity += force;

				System.Console.WriteLine($"F:{force} CN:{contactNormal} T:{contactTime}");

				/*subject.collider.Velocity += contactNormal * new Vector2(
					MathF.Abs(subject.collider.Velocity.X),
					MathF.Abs(subject.collider.Velocity.Y)
					) * (1 - contactTime);*/

				//return true;
			}

		//return false;
	}
}
