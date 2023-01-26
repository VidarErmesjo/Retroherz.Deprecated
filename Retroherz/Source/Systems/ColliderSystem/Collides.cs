using Microsoft.Xna.Framework;
using Retroherz.Components;
using Retroherz.Math;
using Ray = Retroherz.Math.Ray;

namespace Retroherz.Systems;

public partial class ColliderSystem
{
	// Courtesy of One Lone Coder
	// https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
	
	//[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static bool Collides(
		(ColliderComponent collider, TransformComponent transform) subject,
		(ColliderComponent collider, TransformComponent transform) obstacle,
		out Vector contactPoint,
		out Vector contactNormal,
		out float contactTime,
		float deltaTime)
	{
		contactPoint = Vector.Zero;
		contactNormal = Vector.Zero;
		contactTime = 0;
		float timeHitFar = 0;

		// Ignore
		//if (subject.collider.Type == ColliderComponentType.Static) return false;

		// Check if subject is actually moving and not changing size
		if (subject.collider.Velocity == Vector.Zero && subject.collider.DeltaHalfExtents == Vector.Zero) return false;

		// Calculate ray Vector
		Ray ray = new(
			origin: subject.transform.Position + subject.collider.HalfExtents,
			direction: subject.collider.Velocity * deltaTime);
			//direction: (subject.collider.Velocity - obstacle.collider.Velocity) * deltaTime);	// EXP

		// Expand obstacle collider rectangle by subject dimensions
		(Vector Position, Vector Size) rectangle = (
			obstacle.transform.Position - subject.collider.HalfExtents,
			obstacle.collider.Size + subject.collider.Size
		);

		if (obstacle.collider.Velocity != Vector.Zero)
		System.Console.WriteLine($"Sum:{subject.collider.Velocity - obstacle.collider.Velocity}");

		// EXP
		//rectangle.collider.Size += obstacle.collider.DeltaSize;// + subject.collider.DeltaSize;
		//rectangle.transform.Position += rectangle.collider.Velocity * deltaTime; //rectangle.collider.DeltaOrigin;

		// Expand obstacle collider rectangle by subject dimensions
		/*(ColliderComponent collider, TransformComponent transform) rectangle = (
			new(
				size: obstacle.collider.Size + subject.collider.Size,
				velocity: obstacle.collider.Velocity,
				type: obstacle.collider.Type),
			new(position: obstacle.transform.Position - subject.collider.Origin));*/

	
		// EXP
		/*if (obstacle.collider.Type == ColliderComponentType.Dynamic)
		{
			System.Console.WriteLine("{0}", obstacle.collider.Type.ToString());

			var sourceRectangle = new RectangleF(subject.transform.Position + subject.collider.Velocity * deltaTime, subject.collider.Size);
			var targetRectangle = new RectangleF(obstacle.transform.Position + obstacle.collider.Velocity * deltaTime, obstacle.collider.Size);

			var intersectingRectangle = sourceRectangle.Intersection(targetRectangle);

			// Early rejection
			if (intersectingRectangle.IsEmpty) return false;

			Vector penetration;
			if (intersectingRectangle.Width < intersectingRectangle.Height)
			{
				var displacement = sourceRectangle.Center.X < targetRectangle.Center.X
					? intersectingRectangle.Width
					: -intersectingRectangle.Width;
				penetration = Vector.UnitX * displacement;
			}
			else
			{
				var displacement = sourceRectangle.Center.Y < targetRectangle.Center.Y
					? intersectingRectangle.Height
					: -intersectingRectangle.Height;
				penetration = Vector.UnitY * displacement;
			}

			//subject.transform.Position += -penetration;

			contactNormal = -penetration.NormalizedCopy();
			if (float.IsNaN(contactNormal.X) || float.IsNaN(contactNormal.Y)) contactNormal = Vector.Zero;

			contactPoint = subject.transform.Position + subject.collider.Origin;
			contactTime = 0.0f;// = (penetration / obstacle.collider.Size).Length();

			// FUN
			//obstacle.collider.Velocity += -contactNormal * new Vector(MathF.Abs(subject.collider.Velocity.X), MathF.Abs(subject.collider.Velocity.Y) * contactTime);
			//subject.collider.Velocity += contactNormal * new Vector(MathF.Abs(obstacle.collider.Velocity.X), MathF.Abs(obstacle.collider.Velocity.Y) * contactTime);

			return (contactNormal != Vector.Zero) ? true : false;
		}*/

		//System.Console.WriteLine($"s.Size:{subject.collider.Size} t.Size:{rectangle.collider.Size} diff:{rectangle.collider.Size - subject.collider.Size}");
		//System.Console.WriteLine($"s.Pos:{subject.transform.Position} t.Pos:{rectangle.transform.Position} diff:{rectangle.transform.Position - subject.transform.Position}");
		
		// Cast ray
		if(ray.Intersects(
			rectangle,
			out contactPoint,
			out contactNormal,
			out contactTime,
			out timeHitFar
		))
			return (contactTime >= 0 && contactTime < 1);
		else 
			return false;
	}
}