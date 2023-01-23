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
	internal static bool Resolve(
		(ColliderComponent collider, TransformComponent transform) subject,
		(ColliderComponent collider, TransformComponent transform) obstacle,
		float deltaTime)
	{
		Vector contactPoint = Vector.Zero;
		Vector contactNormal = Vector.Zero;
		float contactTime = 0;

		if (Collides(
			subject,
			obstacle,
			out contactPoint,
			out contactNormal,
			out contactTime,
			deltaTime))
		{
			// Resolve diagonal freak case (single contact).
			// - VE
			if (contactNormal == Vector.Zero && subject.collider.Contacts.Count == 1)
			{
				contactNormal = Vector.Clamp(-subject.collider.Velocity, -Vector.One, Vector.One);
				System.Console.WriteLine("Diagonal!");
			}

			// Dynamics ??
			/*if (obstacle.collider.Type == ColliderComponentType.Dynamic) // obstacle.collider.Velocity != Vector.Zero
			{
					
				var a = contactNormal * subject.collider.Velocity.Abs() * (1 - contactTime);
				var b = contactNormal * obstacle.collider.Velocity.Abs() * (1 - contactTime);
				var c = contactNormal * (subject.collider.Velocity.Abs() + obstacle.collider.Velocity.Abs()) * (1 - contactTime);

				Console.WriteLine($"Vel A:{subject.collider.Velocity} Vel B:{obstacle.collider.Velocity} Force A:{a} Force B: {b} Force C:{c}");
			}*/

			// Displace
			subject.collider.Velocity += contactNormal * subject.collider.Velocity.Abs() * (1 - contactTime);


			// Displace
			/*subject.collider.Velocity += contactNormal * new Vector(
				MathD.Abs(subject.collider.Velocity.X),
				MathD.Abs(subject.collider.Velocity.Y)
				) * (1 - contactTime);*/

			return true;
		}

		return false;
	}
}
