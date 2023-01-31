using System.Runtime.CompilerServices;
using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;

public partial class ColliderSystem
{
	// Courtesy of One Lone Coder
	// https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp        

	///	<summary>
	///	Resolves a collision conflict between "Ego" and "Obstacle".
	///	</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool Resolve(
		in (ColliderComponent collider, TransformComponent transform) ego,
		in (ColliderComponent collider, TransformComponent transform) obstacle,
		in float deltaTime)
	{
		Vector contactPoint = Vector.Zero;
		Vector contactNormal = Vector.Zero;
		float contactTime = 0;

		if (Collides(
			ego,
			obstacle,
			out contactPoint,
			out contactNormal,
			out contactTime,
			deltaTime))
		{
			// Resolve diagonal freak case (single contact).
			// - VE
			if (contactNormal == Vector.Zero && ego.collider.Contacts.Count == 1)
			{
				contactNormal = Vector.Clamp(-ego.collider.Velocity, -Vector.One, Vector.One);
				System.Console.WriteLine("Diagonal!");
			}

			// Displace
			ego.collider.Velocity += contactNormal * ego.collider.Velocity.Abs() * (1 - contactTime);

			return true;
		}

		return false;
	}
}
