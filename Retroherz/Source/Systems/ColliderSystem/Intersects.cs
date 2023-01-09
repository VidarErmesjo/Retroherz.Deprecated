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

using Retroherz.Components;
using Retroherz.Math;

using MathD = System.Math;

namespace Retroherz.Systems
{
	public partial class ColliderSystem
	{
		// Courtesy of One Lone Coder
        // https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
        
		//[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static bool Intersects(
			(Vector Position, Vector Direction) ray,
            (ColliderComponent collider, TransformComponent transform) target,
            out Vector contactPoint,
            out Vector contactNormal,
            out double timeHitNear,
			out double timeHitFar)
        {
            contactPoint = Vector.Zero;
            contactNormal = Vector.Zero;
            timeHitNear = 0;
			timeHitFar = 0;

            // Are we in the correct (South-East) quadrant?
			// ... then calculate out-of-bands offset
        	(bool outOfBounds, Vector offset) state = (
				target.transform.Position.X < 0 || target.transform.Position.Y < 0,
				target.collider.Size - ray.Position
			);

            // To not confuse the algorithm on off-grid bounds we shift target position and ray origin
            // to force upcomming calculations over to the positive axes.
            // - VE
            if (state.outOfBounds)
            {
                target.transform.Position += state.offset;
                ray.Position += state.offset;
            }

             // Cache division
            var inverseDirection = Vector.One / ray.Direction;

            // Calculate intersections with target boundings axes
            var targetNear = (target.transform.Position - ray.Position) * inverseDirection;
            var targetFar = (target.transform.Position + target.collider.Size - ray.Position) * inverseDirection;
          
            if (double.IsNaN(targetFar.X) || double.IsNaN(targetFar.Y)) return false;
            if (double.IsNaN(targetNear.X) || double.IsNaN(targetNear.Y)) return false;

            // Sort distances
            if (targetNear.X > targetFar.X) Utils.Swap(ref targetNear.X, ref targetFar.X);
            if (targetNear.Y > targetFar.Y) Utils.Swap(ref targetNear.Y, ref targetFar.Y);

            // Early rejection
            if (targetNear.X > targetFar.Y || targetNear.Y > targetFar.X) return false;

            // Closest 'time' will be the first contact
            timeHitNear = MathD.Max(targetNear.X, targetNear.Y);

            // Furthest 'time' is contact on opposite side of target
            timeHitFar = MathD.Min(targetFar.X, targetFar.Y);
            
            // Reject if ray directon is pointing away from object (can be usefull)
            if (timeHitFar < 0) return false;

			// For a correct calculation of contact point we shift ray origin back
            // - VE
            if (state.outOfBounds) ray.Position -= state.offset;

            // Contact point of collision from parametric line equation
            contactPoint = ray.Position + timeHitNear * ray.Direction;

            if (targetNear.X > targetNear.Y)
                contactNormal = inverseDirection.X < 0 ? Vector.UnitX : -Vector.UnitX;
            else if (targetNear.X < targetNear.Y)
                contactNormal = inverseDirection.Y < 0 ? Vector.UnitY : -Vector.UnitY;
            else
                // Note if targetNear == targetFar, collision is principly in a diagonal
                // so pointless to resolve. By returning a CN={0,0} even though its
                // considered a hit, the resolver wont change anything.

                // Diagonal case will be resolved in collision resolver. Thus we return CN={0,0}.
                // - VE
				contactNormal = Vector.Zero;

			if (state.outOfBounds)
				Console.WriteLine($"outOfBounds:{state.outOfBounds} Position:{target.transform.Position} CN:{contactNormal} tNear:{timeHitNear} tFar:{timeHitFar}");

            return true;
        }
    }
}
/*
	ANOMALY

P:{X:32.5 Y:240.5} CN:{X:0 Y:1} Tn:0.09114876 Tf:2.6401124
P:{X:32.5 Y:240.5} CN:{X:0 Y:1} Tn:0.09114876 Tf:2.6401124
P:{X:32.5 Y:240.5} CN:{X:0 Y:1} Tn:-0 Tf:1.6225778
P:{X:32.5 Y:240.5} CN:{X:0 Y:1} Tn:-0 Tf:1.6225778
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:0.806396 Tf:9.755377
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:0.806396 Tf:9.755377
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:0.806396 Tf:9.755377
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:1 Tf:12.097501
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:8.279949E-16 Tf:10.953813
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:8.279949E-16 Tf:10.953813
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:8.279949E-16 Tf:10.953813
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:0.93749815 Tf:1.2402466E+16
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0 Tf:845.9967
P:{X:0.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0.6365348 Tf:7.265573
P:{X:16.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0.6365348 Tf:24.25691
P:{X:32.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0.6365348 Tf:41.248245
P:{X:32.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0.6365348 Tf:41.248245
P:{X:16.5 Y:288.5} CN:{X:-0 Y:-1} Tn:1 Tf:24.25691
P:{X:0.5 Y:288.5} CN:{X:-0 Y:-1} Tn:1 Tf:7.265573
P:{X:0.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0 Tf:6.0146546
P:{X:16.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0 Tf:22.325537
P:{X:32.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0 Tf:38.636417
P:{X:32.5 Y:288.5} CN:{X:-0 Y:-1} Tn:0 Tf:38.636417
P:{X:288.5 Y:256.5} CN:{X:-1 Y:-0} Tn:0.88055944 Tf:10.316499
P:{X:288.5 Y:272.5} CN:{X:-1 Y:-0} Tn:0.88055944 Tf:10.316499
P:{X:288.5 Y:272.5} CN:{X:-1 Y:-0} Tn:0.88055944 Tf:10.316499
P:{X:288.5 Y:256.5} CN:{X:-1 Y:-0} Tn:1 Tf:11.715846
P:{X:288.5 Y:256.5} CN:{X:-1 Y:-0} Tn:0 Tf:10.715846
P:{X:288.5 Y:272.5} CN:{X:-1 Y:-0} Tn:0 Tf:10.715846
P:{X:288.5 Y:272.5} CN:{X:-1 Y:-0} Tn:0 Tf:10.715846
P:{X:-4 Y:44} CN:{X:1 Y:0} Tn:0.07682819 Tf:192.90543
P:{X:-4 Y:44} CN:{X:1 Y:0} Tn:0.07682819 Tf:192.90543
P:{X:-4 Y:44} CN:{X:1 Y:0} Tn:-0 Tf:216.16287
P:{X:-4 Y:44} CN:{X:1 Y:0} Tn:-0 Tf:216.16287
P:{X:32.5 Y:240.5} CN:{X:0 Y:1} Tn:0.6365348 Tf:5.8362465
P:{X:48.5 Y:240.5} CN:{X:0 Y:1} Tn:0.6365348 Tf:2.598476
P:{X:48.5 Y:240.5} CN:{X:0 Y:1} Tn:0.6365348 Tf:2.598476
P:{X:32.5 Y:240.5} CN:{X:0 Y:1} Tn:1 Tf:5.8362465
P:{X:32.5 Y:240.5} CN:{X:0 Y:1} Tn:-0 Tf:4.798104
P:{X:48.5 Y:240.5} CN:{X:0 Y:1} Tn:-0 Tf:1.5858691
P:{X:48.5 Y:240.5} CN:{X:0 Y:1} Tn:-0 Tf:1.5858691
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:0.8921441 Tf:9.925083
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:0.8921441 Tf:9.925083
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:0.8921441 Tf:9.925083
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:1 Tf:11.124978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-7.562922E-16 Tf:10.005235
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-7.562922E-16 Tf:10.005235
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-0.98831165 Tf:8.899979
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-0.98831165 Tf:8.899979
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-1.9653401 Tf:7.808707
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-1.9653401 Tf:7.808707
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-2.931472 Tf:6.7309427
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-2.931472 Tf:6.7309427
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:256.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
P:{X:-15.5 Y:272.5} CN:{X:1 Y:0} Tn:-287.99887 Tf:557.9978
*/