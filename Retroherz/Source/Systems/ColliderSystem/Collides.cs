using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;

using Retroherz.Math;

using Retroherz.Components;

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
		out double contactTime,
		float deltaTime)
	{
		contactPoint = Vector.Zero;
		contactNormal = Vector.Zero;
		contactTime = 0;
		double timeHitFar = 0;

		// Ignore
		if (subject.collider.Type == ColliderComponentType.Static) return false;

		// Check if subject is actually moving and not changing size
		if (subject.collider.Velocity == Vector.Zero && subject.collider.DeltaOrigin == Vector.Zero) return false;

		// Expand obstacle collider box by subject dimensions
		(ColliderComponent collider, TransformComponent transform) rectangle = (
			new(
				size: obstacle.collider.Size + subject.collider.Size,
				velocity: obstacle.collider.Velocity,
				type: obstacle.collider.Type),
			new(position: obstacle.transform.Position - subject.collider.Origin));

		// EXP
		//rectangle.collider.Size += obstacle.collider.DeltaSize;// + subject.collider.DeltaSize;
		//rectangle.transform.Position += rectangle.collider.Velocity * deltaTime; //rectangle.collider.DeltaOrigin;

		// Calculate ray vectors
		(Vector Position, Vector Direction) ray = new(
			subject.transform.Position + subject.collider.Origin,
			subject.collider.Velocity * deltaTime);
	
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
		if (Intersects(
			ray,
			rectangle,
			out contactPoint,
			out contactNormal,
			out contactTime,
			out timeHitFar))
				return (contactTime >= 0 && contactTime < 1);
		else 
			return false;
	}
}

/* ANOMALY

collider.Size = 31

CN:{X:1 Y:0} T:0.24911279867506186
CN:{X:1 Y:0} T:0.24911279867506186
CN:{X:1 Y:0} T:0.24911279867506186
CN:{X:1 Y:0} T:1
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-2.6650259333915787E-15
CN:{X:1 Y:0} T:-2.6650259333915787E-15
CN:{X:0 Y:1} T:-0
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-0.9713752006100137
CN:{X:1 Y:0} T:-0.9713752006100137
CN:{X:0 Y:1} T:-0
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-1.9165153336562655
CN:{X:1 Y:0} T:-1.9165153336562655
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-2.8060818458724643
CN:{X:1 Y:0} T:-2.8060818458724643
CN:{X:1 Y:0} T:-3.6672229097539666
CN:{X:1 Y:0} T:-3.6672229097539666
CN:{X:1 Y:0} T:-4.502940188240151
CN:{X:1 Y:0} T:-4.502940188240151
CN:{X:1 Y:0} T:-5.315827089457604
CN:{X:1 Y:0} T:-5.315827089457604
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-6.167538986679438
CN:{X:1 Y:0} T:-6.167538986679438
CN:{X:0 Y:1} T:-0
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-7.003114200561785
CN:{X:1 Y:0} T:-7.003114200561785
CN:{X:0 Y:1} T:-0
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-7.82363836278081
CN:{X:1 Y:0} T:-7.82363836278081
CN:{X:0 Y:1} T:-0
CN:{X:1 Y:0} T:-8.55240069062524
CN:{X:1 Y:0} T:-8.55240069062524
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567
CN:{X:1 Y:0} T:-287.9988678965567

... more

ray:({X:66.45473882150006 Y:287.5}, {X:-4.869844382231962 Y:-0.21432270223612182}) CN:{X:0 Y:1} Tn:-0 Tf:6.9724483
ray:({X:66.45473882150006 Y:287.5}, {X:-4.869844382231962 Y:-0.21432270223612182}) CN:{X:0 Y:1} Tn:-0 Tf:3.6869226
ray:({X:66.45473882150006 Y:287.5}, {X:-4.869844382231962 Y:-0.21432270223612182}) CN:{X:0 Y:1} Tn:-0 Tf:3.6869226
ray:({X:61.58489443926809 Y:287.5}, {X:-4.909128247120157 Y:-0.03928386488819502}) CN:{X:0 Y:1} Tn:-0 Tf:5.9246554
ray:({X:61.58489443926809 Y:287.5}, {X:-4.909128247120157 Y:-0.03928386488819502}) CN:{X:0 Y:1} Tn:-0 Tf:2.6654212
ray:({X:61.58489443926809 Y:287.5}, {X:-4.909128247120157 Y:-0.03928386488819502}) CN:{X:0 Y:1} Tn:-0 Tf:2.6654212
ray:({X:56.67576619214793 Y:287.5}, {X:-4.948412112008352 Y:-0.03928386488819502}) CN:{X:0 Y:1} Tn:-0 Tf:4.8855605
ray:({X:56.67576619214793 Y:287.5}, {X:-4.948412112008352 Y:-0.03928386488819502}) CN:{X:0 Y:1} Tn:-0 Tf:1.6522
ray:({X:56.67576619214793 Y:287.5}, {X:-4.948412112008352 Y:-0.03928386488819502}) CN:{X:0 Y:1} Tn:-0 Tf:1.6522

... it begins

ray:({X:36.54878310047113 Y:287.5}, {X:-5.170635207770616 Y:0}) CN:{X:1 Y:0} Tn:0.9764338 Tf:10.066226
ray:({X:36.54878310047113 Y:287.5}, {X:-5.170635207770616 Y:0}) CN:{X:1 Y:0} Tn:0.9764338 Tf:10.066226
ray:({X:36.54878310047113 Y:287.5}, {X:-5.170635207770616 Y:0}) CN:{X:1 Y:0} Tn:0.9764338 Tf:10.066226
ray:({X:36.54878310047113 Y:287.5}, {X:-5.048783100471127 Y:0}) CN:{X:1 Y:0} Tn:1 Tf:10.309174

... catastrophy

ray:({X:31.499999999999996 Y:287.5}, {X:-5.104338874411693 Y:0}) CN:{X:1 Y:0} Tn:-6.9601835E-16 Tf:9.207852
ray:({X:31.499999999999996 Y:287.5}, {X:-5.104338874411693 Y:0}) CN:{X:1 Y:0} Tn:-6.9601835E-16 Tf:9.207852
ray:({X:26.395661125588305 Y:287.5}, {X:-5.1598946483522585 Y:0}) CN:{X:1 Y:0} Tn:-0.98923314 Tf:8.11948
ray:({X:26.395661125588305 Y:287.5}, {X:-5.1598946483522585 Y:0}) CN:{X:1 Y:0} Tn:-0.98923314 Tf:8.11948
ray:({X:21.235766477236048 Y:287.5}, {X:-5.215450422292824 Y:0}) CN:{X:1 Y:0} Tn:-1.9680436 Tf:7.043642
ray:({X:21.235766477236048 Y:287.5}, {X:-5.215450422292824 Y:0}) CN:{X:1 Y:0} Tn:-1.9680436 Tf:7.043642
ray:({X:16.020316054943223 Y:287.5}, {X:-5.27100619623339 Y:0}) CN:{X:1 Y:0} Tn:-2.9367607 Tf:5.979943
ray:({X:16.020316054943223 Y:287.5}, {X:-5.27100619623339 Y:0}) CN:{X:1 Y:0} Tn:-2.9367607 Tf:5.979943
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
ray:({X:15.5 Y:287.5}, {X:-0.05555577394056588 Y:0}) CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
*/