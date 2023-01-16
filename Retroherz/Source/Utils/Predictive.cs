using System.Runtime.CompilerServices;
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
				(collider.Type == ColliderComponentType.Dynamic ? collider.Velocity * deltaTime : Vector.Zero) + 
				(margin ? collider.Size : collider.Origin)
			);

			Size2 halfExtents = collider.Origin;

			return new(center, halfExtents);
		}
}