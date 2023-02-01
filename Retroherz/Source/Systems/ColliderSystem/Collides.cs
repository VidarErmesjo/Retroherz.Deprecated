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
		in (ColliderComponent Collider, TransformComponent Transform) ego,
		in (ColliderComponent Collider, TransformComponent Transform) obstacle,
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
		if (ego.Collider.Velocity == Vector.Zero && ego.Collider.DeltaHalfExtents == Vector.Zero) return false;

		// Calculate ray Vector
		Ray ray = new(
			origin: ego.Transform.Position + ego.Collider.HalfExtents,
			//direction: ego.Collider.Velocity * deltaTime);
			direction: (ego.Collider.Velocity - obstacle.Collider.Velocity) * deltaTime);	// YEAH! :P

		// Expand obstacle collider rectangle by ego dimensions
		(Vector Position, Vector Size) rectangle = (
			obstacle.Transform.Position - ego.Collider.HalfExtents,
			obstacle.Collider.Size + ego.Collider.Size
		);
	
		// Cast ray
		if(ray.Intersects(
			rectangle,
			out contactPoint,
			out contactNormal,
			out contactTime,
			out timeHitFar
		))
			return (contactTime >= -obstacle.Collider.Size.Magnitude() && contactTime < 1); // Works??? (>= -1) .. -2 even better? :p
		else 
			return false;
	}
}