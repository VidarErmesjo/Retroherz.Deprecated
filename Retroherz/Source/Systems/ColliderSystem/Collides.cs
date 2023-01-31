using System.Runtime.CompilerServices;
using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class ColliderSystem
{
	// Courtesy of One Lone Coder
	// https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
	
	/// <summary>
	///	Evaluates whether a collision occured between "Ego" and "Obstacle".
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static bool Collides(
		in (ColliderComponent collider, TransformComponent transform) ego,
		in (ColliderComponent collider, TransformComponent transform) obstacle,
		out Vector contactPoint,
		out Vector contactNormal,
		out float contactTime,
		in float deltaTime)
	{
		contactPoint = Vector.Zero;
		contactNormal = Vector.Zero;
		contactTime = 0;
		float timeHitFar = 0;

		// Check if ego is actually moving and not changing size
		if (ego.collider.Velocity == Vector.Zero && ego.collider.DeltaHalfExtents == Vector.Zero) return false;

		// Calculate ray Vector
		Ray ray = new(
			origin: ego.transform.Position + ego.collider.HalfExtents,
			//direction: ego.collider.Velocity * deltaTime);
			direction: (ego.collider.Velocity - obstacle.collider.Velocity) * deltaTime);	// YEAH! :P

		// Expand obstacle collider rectangle by ego dimensions
		(Vector Position, Vector Size) rectangle = (
			obstacle.transform.Position - ego.collider.HalfExtents,
			obstacle.collider.Size + ego.collider.Size
		);
	
		// Cast ray
		if(ray.Intersects(
			rectangle,
			out contactPoint,
			out contactNormal,
			out contactTime,
			out timeHitFar
		))
			return (contactTime >= -rectangle.Size.Magnitude() && contactTime < 1); // Works??? (>= -1) .. -2 even better? :p
		else 
			return false;
	}
}