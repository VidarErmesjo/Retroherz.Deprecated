using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;

using Retroherz.Components;

namespace Retroherz.Systems;

public partial class ColliderSystem
{
	// Courtesy of One Lone Coder
	// https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp        

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool Resolve(
		(ColliderComponent collider, TransformComponent transform) source,
		(ColliderComponent collider, TransformComponent transform) target,
		float deltaTime)
	{
		var contactPoint = Vector2.Zero;
		var contactNormal = Vector2.Zero;
		var contactTime = 0f;

		if (Collides(
			source,
			target,
			out contactPoint,
			out contactNormal,
			out contactTime,
			deltaTime))
		{
			// Resolve diagonal freak case (single contact).
			if (contactNormal == Vector2.Zero && source.collider.Contacts.Length == 1)
			{
				contactNormal = Vector2.Clamp(-source.collider.Velocity, -Vector2.One, Vector2.One);
				System.Console.WriteLine("Diagonal!");
			}

			var someforce = Vector2.Zero;

		// Check if source is changing size and nudge it
		/*if (source.collider.DeltaSize != Vector2.Zero)
		{
			// Inert and changing size
			var sourceRectangle = Utils.ProspectiveRectangle(source.collider, source.transform, deltaTime);
			var targetRectangle = new RectangleF(target.transform.Position, target.collider.Size);
			var intersectingRectangle = sourceRectangle.Intersection(targetRectangle);

			// Early rejection
			if (intersectingRectangle.IsEmpty) return false;

			Vector2 penetration;
			if (intersectingRectangle.Width < intersectingRectangle.Height)
			{
				var displacement = sourceRectangle.Center.X < targetRectangle.Center.X
					? intersectingRectangle.Width
					: -intersectingRectangle.Width;
				penetration = Vector2.UnitX * displacement;
			}
			else
			{
				var displacement = sourceRectangle.Center.Y < targetRectangle.Center.Y
					? intersectingRectangle.Height
					: -intersectingRectangle.Height;
				penetration = Vector2.UnitY * displacement;
			}

			someforce = -penetration;

			//Vector2.Clamp(penetration, -Vector2.One, Vector2.One);
			// Nudge position
			//source.transform.Position += -penetration;
			//source.collider.Velocity -= source.collider.Velocity + penetration;


			if (penetration.X >= 1f || penetration.Y >= 1f)
			{
				//source.collider.Size -= source.collider.DeltaSize;
				//source.collider.Velocity = Vector2.Zero;
								// Nudge
				//source.collider.Velocity += -penetration;
			}
			else
			{

				// Shift position
				//source.transform.Position += -penetration;
			}
			//System.Console.WriteLine($"Penetration:{penetration} Velocity:{source.collider.Velocity}");

			//contactNormal = -penetration.NormalizedCopy();
			//if (float.IsNaN(contactNormal.X) || float.IsNaN(contactNormal.Y)) contactNormal = Vector2.Zero;

			//return (contactNormal != Vector2.Zero) ? true : false;


			// Nudge
			//source.collider.Velocity += -penetration;
			//rayDirection = -penetration * deltaTime;

			//System.Console.WriteLine($"Penetration:{penetration}");
			//System.Console.WriteLine($"Trangt! {penetration.X > 1f || penetration.Y > 1f}");
		}*/
			// Displace
			/*source.collider.Velocity += contactNormal * new Vector2(
				MathF.Abs(source.collider.Velocity.X),
				MathF.Abs(source.collider.Velocity.Y)
				) * (1 - contactTime);*/

			// Exp

			//var l = source.collider.DeltaOrigin.Length();
			//var z = MathF.Sqrt(MathF.Pow(source.collider.DeltaSize.X, 2) + MathF.Pow(source.collider.DeltaSize.Y, 2));
			//System.Console.WriteLine($"l:{l} z:{z}");
			//System.Console.WriteLine(contactNormal * source.collider.Velocity * (1 - contactTime));

			var penetration = contactNormal * new Vector2(
				MathF.Abs(source.collider.Velocity.X),
				MathF.Abs(source.collider.Velocity.Y)
				) * (1 - contactTime);

			//var deltaSize = contactNormal * source.collider.DeltaSize.Length() * (1 - contactTime);
			var deltaSize = contactNormal * new Vector2(
				MathF.Abs(source.collider.DeltaSize.X),
				MathF.Abs(source.collider.DeltaSize.Y)
				) * (1 - contactTime);

			//var deltaOrigin = contactNormal * source.collider.DeltaOrigin.Length() * (1 - contactTime);
			var deltaOrigin = contactNormal * new Vector2(
				MathF.Abs(source.collider.DeltaOrigin.X),
				MathF.Abs(source.collider.DeltaOrigin.Y)
				) * (1 - contactTime);

			source.collider.Velocity += penetration + deltaSize + deltaOrigin;

			System.Console.WriteLine($"V:[{source.collider.Velocity}], dV:[{penetration}], dS:[{deltaSize}], dO:[{deltaOrigin}], T:[{contactTime}]");

			return true;
		}

		return false;
	}
}
