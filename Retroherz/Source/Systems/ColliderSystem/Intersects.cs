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
        
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Intersects(
			(Vector2 Origin, Vector2 Direction) ray,
            (ColliderComponent collider, TransformComponent transform) box,
            out Vector2 contactPoint,
            out Vector2 contactNormal,
            out float timeHitNear,
			out float timeHitFar)
        {
            contactPoint = Vector2.Zero;
            contactNormal = Vector2.Zero;
            timeHitNear = 0;
			timeHitFar = 0;

            // Are we in the correct (South-East) quadrant?
			// ... then calculate out-of-bounds offset (object size)
        	(bool outOfBounds, Vector2 offset) state = (
				box.transform.Position.X < 0 || box.transform.Position.Y < 0,
				box.collider.Size - ray.Origin
			);

            // To not confuse the algorithm on off-grid bounds we shift box position and ray origin
            // to force upcomming calculations over to the positive axes.
            // - VE
            if (state.outOfBounds)
            {
                box.transform.Position += state.offset;
                ray.Origin += state.offset;
            }

            // Cache division
            var inverseDirection = Vector2.One / ray.Direction;

            // Calculate intersections with box boundings axes
            var targetNear = (box.transform.Position - ray.Origin) * inverseDirection;
            var targetFar = (box.transform.Position + box.collider.Size - ray.Origin) * inverseDirection;
          
            if (float.IsNaN(targetFar.X) || float.IsNaN(targetFar.Y)) return false;
            if (float.IsNaN(targetNear.X) || float.IsNaN(targetNear.Y)) return false;

            // Sort distances
            if (targetNear.X > targetFar.X) Utils.Swap(ref targetNear.X, ref targetFar.X);
            if (targetNear.Y > targetFar.Y) Utils.Swap(ref targetNear.Y, ref targetFar.Y);

            // Early rejection
            if (targetNear.X > targetFar.Y || targetNear.Y > targetFar.X) return false;

            // Closest 'time' will be the first contact
            timeHitNear = MathD.Max(targetNear.X, targetNear.Y);

            // Furthest 'time' is contact on opposite side of box
            timeHitFar = MathD.Min(targetFar.X, targetFar.Y);
            
            // Reject if ray directon is pointing away from object (can be usefull)
            if (timeHitFar < 0) return false;

			// For a correct calculation of contact point we shift ray origin back
            // - VE
            if (state.outOfBounds) ray.Origin -= state.offset;

            // Contact point of collision from parametric line equation
            contactPoint = ray.Origin + timeHitNear * ray.Direction;

            if (targetNear.X > targetNear.Y)
                contactNormal = inverseDirection.X < 0 ? Vector2.UnitX : -Vector2.UnitX;
            else if (targetNear.X < targetNear.Y)
                contactNormal = inverseDirection.Y < 0 ? Vector2.UnitY : -Vector2.UnitY;
            else
                // Note if targetNear == targetFar, collision is principly in a diagonal
                // so pointless to resolve. By returning a CN={0,0} even though its
                // considered a hit, the resolver wont change anything.

                // Diagonal case will be resolved in collision resolver. Thus we return CN={0,0}.
                // - VE
				contactNormal = Vector2.Zero;

            return true;
        }
    }
}