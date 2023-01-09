using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

using Retroherz.Components;

// Predictive???
namespace Retroherz
{
	internal partial class Utils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static RectangleF ProspectiveRectangle(
			ColliderComponent collider,
			TransformComponent transform,
			float deltaTime)
		{
			// All dynamic colliders should have bounding rectangle rounded to nearest integer to help with
			// moving through tight passages etc.
			var bounds = new RectangleF(
				collider.Type == ColliderComponentType.Dynamic ? Vector2.Round(transform.Position) : transform.Position,
				collider.Size);

			// Pilot rectangle
			var pilot = new RectangleF(transform.Position + collider.Velocity * deltaTime, collider.Size);

			// Inflate and return
			var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
			var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);

			return new(minimum, maximum - minimum);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static BoundingRectangle ProspectiveBoundingRectangle(
			ColliderComponent collider,
			TransformComponent transform,
			float deltaTime)
		{
			var center = transform.Position + collider.Origin;
			//center = collider.Type == ColliderComponentType.Dynamic ? Vector2.Round(center) : center;
			var halfExtents = collider.Origin;

			// All dynamic colliders should have bounding rectangle rounded to nearest integer to help with
			// moving through tight passages etc.
			
			/*var bounds = new BoundingRectangle(
				center,
				halfExtents);*/			

			// Pilot rectangle
			var pilot = new BoundingRectangle(center + collider.Velocity * deltaTime, halfExtents);				

			return new BoundingRectangle(center, halfExtents).Union(pilot);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Vector2 Prospective(ColliderComponent collider, TransformComponent transform, float deltaTime)
		{
			return transform.Position + collider.Origin + collider.Velocity * deltaTime;
		}
	}
}