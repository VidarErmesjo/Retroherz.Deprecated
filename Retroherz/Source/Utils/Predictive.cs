using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz;

internal class Predictive
{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static BoundingRectangle BoundingRectangle(
			ColliderComponent collider,
			TransformComponent transform,
			float deltaTime,
			bool margin = false)
		{
			Point2 center = (
				(transform.Position) +
				(collider.Type == ColliderComponentType.Dynamic ? collider.Velocity * deltaTime : Math.Vector.Zero) + 
				(margin ? collider.Size : collider.Origin));
			Size2 halfExtents = collider.Origin;

			return new(center, halfExtents);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Vector Vector(
			ColliderComponent collider,
			TransformComponent transform,
			float deltaTime) => transform.Position + collider.Origin + collider.Velocity * deltaTime;
}